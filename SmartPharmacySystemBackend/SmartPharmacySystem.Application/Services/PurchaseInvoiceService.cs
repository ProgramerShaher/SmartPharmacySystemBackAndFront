using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.DTOs.PurchaseInvoice;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.DTOs.Financial;

namespace SmartPharmacySystem.Application.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseInvoiceService> _logger;
        private readonly IStockMovementService _stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IFinancialService _financialService;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IAlertService _alertService;
        private readonly IBarcodeService _barcodeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public PurchaseInvoiceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PurchaseInvoiceService> logger,
            IStockMovementService stockMovementService,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IFinancialService financialService,
            IJournalEntryService journalEntryService,
            IAlertService alertService,
            IBarcodeService barcodeService,
            ICurrentUserService currentUserService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _financialService = financialService;
            _journalEntryService = journalEntryService;
            _alertService = alertService;
            _barcodeService = barcodeService;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto, int userId)
        {
            if (dto.Items == null || !dto.Items.Any())
                throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل للفاتورة.");

            var invoice = _mapper.Map<PurchaseInvoice>(dto);
            invoice.CreatedAt = DateTime.UtcNow;
            invoice.CreatedBy = userId;
            invoice.Status = DocumentStatus.Draft;
            invoice.WarehouseId = await ResolveReceivingWarehouseIdAsync(dto.WarehouseId);
            invoice.PurchaseInvoiceDetails = new List<PurchaseInvoiceDetail>();

            decimal calculatedTotal = 0;

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.ExpiryDate.Date < DateTime.Today)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({itemDto.ExpiryDate:yyyy-MM-dd}) للدواء '{med?.Name}' غير صالح (قديم).");
                }

                if (itemDto.SalePrice < itemDto.PurchasePrice)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، سعر البيع ({itemDto.SalePrice}) أقل من سعر الشراء ({itemDto.PurchasePrice}) للدواء '{med?.Name}'.");
                }

                int batchId = await GetOrCreateBatchIdAsync(itemDto.MedicineId, itemDto.BatchBarcode, itemDto.CompanyBatchNumber, itemDto.ExpiryDate, userId);

                var detail = _mapper.Map<PurchaseInvoiceDetail>(itemDto);
                detail.BatchId = batchId;
                detail.Total = itemDto.Quantity * itemDto.PurchasePrice;

                calculatedTotal += detail.Total;
                invoice.PurchaseInvoiceDetails.Add(detail);
            }

            invoice.TotalAmount = calculatedTotal;
            invoice.PurchaseInvoiceNumber = await _invoiceNumberGenerator.GeneratePurchaseInvoiceNumberAsync();

            await _unitOfWork.PurchaseInvoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            var createdInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(invoice.Id);
            return _mapper.Map<PurchaseInvoiceDto>(createdInvoice);
        }

        private async Task<int> GetOrCreateBatchIdAsync(int medicineId, string? barcode, string? companyBatch, DateTime expiry, int? userId)
        {
            MedicineBatch? existingBatch = null;
            var currentBranchId = _currentUserService.GetCurrentBranchId();

            if (!string.IsNullOrEmpty(barcode))
            {
                existingBatch = await _unitOfWork.MedicineBatches.GetByBarcodeAsync(barcode);
            }

            if (existingBatch == null && !string.IsNullOrWhiteSpace(companyBatch))
            {
                existingBatch = await _unitOfWork.MedicineBatches.GetByCompanyBatchNumberAsync(companyBatch);
            }

            if (existingBatch != null && currentBranchId.HasValue && existingBatch.PurchaseInvoiceId.HasValue)
            {
                var linkedInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(existingBatch.PurchaseInvoiceId.Value);
                if (linkedInvoice != null && linkedInvoice.BranchId == currentBranchId.Value)
                    return existingBatch.Id;
            }

            // If we are creating a new batch, we must ensure CompanyBatchNumber is strictly unique!
            if (string.IsNullOrWhiteSpace(companyBatch) || companyBatch == "N/A" || existingBatch != null)
            {
                var baseBatch = string.IsNullOrWhiteSpace(companyBatch) || companyBatch == "N/A" ? $"BAT-{DateTime.UtcNow:yyMMdd}" : companyBatch;
                companyBatch = $"{baseBatch}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            }

            var newBatch = new MedicineBatch
            {
                MedicineId = medicineId,
                Quantity = 0,
                RemainingQuantity = 0,
                UnitPurchasePrice = 0,
                RetailPrice = 0,
                ExpiryDate = expiry,
                BatchBarcode = barcode,
                CompanyBatchNumber = companyBatch,
                Status = "Incoming",
                EntryDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.MedicineBatches.AddAsync(newBatch);
            await _unitOfWork.SaveChangesAsync();
            return newBatch.Id;
        }

        public async Task UpdateAsync(int id, UpdatePurchaseInvoiceDto dto)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل فاتورة تم اعتمادها أو إلغاؤها.");

            invoice.SupplierId = dto.SupplierId;
            invoice.SupplierInvoiceNumber = dto.SupplierInvoiceNumber;
            invoice.PurchaseDate = dto.PurchaseDate;
            invoice.PaymentMethod = dto.PaymentMethod;
            invoice.WarehouseId = await ResolveReceivingWarehouseIdAsync(dto.WarehouseId);
            invoice.Notes = dto.Notes;

            _unitOfWork.PurchaseInvoiceDetails.RemoveRange(invoice.PurchaseInvoiceDetails);
            invoice.PurchaseInvoiceDetails.Clear();

            decimal calculatedTotal = 0;

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.ExpiryDate.Date < DateTime.Today)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({itemDto.ExpiryDate:yyyy-MM-dd}) للدواء '{med?.Name}' غير صالح (قديم).");
                }

                if (itemDto.SalePrice < itemDto.PurchasePrice)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، سعر البيع ({itemDto.SalePrice}) أقل من سعر الشراء ({itemDto.PurchasePrice}) للدواء '{med?.Name}'.");
                }

                int batchId = await GetOrCreateBatchIdAsync(itemDto.MedicineId, itemDto.BatchBarcode, itemDto.CompanyBatchNumber, itemDto.ExpiryDate, invoice.CreatedBy);

                var detail = _mapper.Map<PurchaseInvoiceDetail>(itemDto);
                detail.BatchId = batchId;
                detail.PurchaseInvoiceId = invoice.Id;
                detail.Total = itemDto.Quantity * itemDto.PurchasePrice;

                calculatedTotal += detail.Total;
                invoice.PurchaseInvoiceDetails.Add(detail);
            }

            invoice.TotalAmount = calculatedTotal;

            await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PurchaseInvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");
            return _mapper.Map<PurchaseInvoiceDto>(invoice);
        }

        public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
        {
            var invoices = await _unitOfWork.PurchaseInvoices.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseInvoiceDto>>(invoices);
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("الفاتورة يجب أن تكون مسودة للاعتماد.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                invoice.Status = DocumentStatus.Approved;
                invoice.ApprovedBy = userId;
                invoice.ApprovedAt = DateTime.UtcNow;
                decimal validTotal = 0;

                foreach (var detail in invoice.PurchaseInvoiceDetails)
                {
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                        ?? throw new KeyNotFoundException($"الدفعة {detail.BatchId} غير موجودة");

                    int conversionFactor = 1;
                    if (detail.PurchaseUnitId.HasValue)
                    {
                        var purchaseUnit = await _unitOfWork.MedicineUnits.GetByIdAsync(detail.PurchaseUnitId.Value);
                        conversionFactor = purchaseUnit?.ConversionFactor ?? 1;
                    }

                    int rawQty = detail.Quantity;
                    int rawBonus = detail.BonusQuantity;
                    int baseUnits = (rawQty + rawBonus) * conversionFactor;

                    detail.QuantityInPurchaseUnit = rawQty;
                    detail.Quantity = baseUnits;

                    batch.Quantity += baseUnits;
                    batch.RemainingQuantity += baseUnits;
                    batch.Status = "Active";

                    decimal trueUnitCost = baseUnits > 0 ? detail.Total / baseUnits : detail.PurchasePrice;
                    batch.UnitPurchasePrice = trueUnitCost;
                    decimal perBaseUnitSalePrice = conversionFactor > 0 ? detail.SalePrice / conversionFactor : detail.SalePrice;
                    decimal perBaseUnitPurchasePrice = conversionFactor > 0 ? detail.PurchasePrice / conversionFactor : detail.PurchasePrice;
                    
                    batch.RetailPrice = perBaseUnitSalePrice;
                    batch.PurchaseInvoiceId = invoice.Id;
                    detail.TrueUnitCost = trueUnitCost;

                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                    await IncreaseInventoryStockAsync(invoice.WarehouseId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, baseUnits);

                    // Update Unit prices if a specific unit was used
                    if (detail.PurchaseUnitId.HasValue)
                    {
                        var unit = await _unitOfWork.MedicineUnits.GetByIdAsync(detail.PurchaseUnitId.Value);
                        if (unit != null)
                        {
                            unit.DefaultPurchasePrice = detail.PurchasePrice;
                            unit.DefaultSalePrice = detail.SalePrice;
                            await _unitOfWork.MedicineUnits.UpdateAsync(unit);
                        }
                    }

                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                    if (medicine != null)
                    {
                        int totalStock = await _unitOfWork.MedicineBatches.GetTotalQuantityAsync(medicine.Id);
                        decimal oldVal = Math.Max(0, (totalStock - baseUnits) * medicine.MovingAverageCost);
                        decimal newVal = oldVal + detail.Total;
                        medicine.MovingAverageCost = totalStock > 0 ? newVal / totalStock : trueUnitCost;

                        medicine.DefaultSalePrice = perBaseUnitSalePrice;
                        medicine.DefaultPurchasePrice = perBaseUnitPurchasePrice;

                        await _unitOfWork.Medicines.UpdateAsync(medicine);
                    }

                    validTotal += detail.Total;
                }

                invoice.TotalAmount = validTotal;

                var journalEntry = new JournalEntryDto
                {
                    EntryDate = invoice.PurchaseDate,
                    VoucherNumber = invoice.PurchaseInvoiceNumber,
                    Description = $"قيد مشتريات آلي - فاتورة رقم: {invoice.PurchaseInvoiceNumber} - المورد: {invoice.Supplier?.Name ?? "غير معروف"}",
                    Type = VoucherType.PurchaseInvoice,
                    Lines = new List<JournalEntryLineDto>()
                };

                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 1301,
                    Debit = invoice.TotalAmount,
                    Credit = 0,
                    Description = $"إضافة للمخزون - فاتورة شراء {invoice.PurchaseInvoiceNumber}"
                });

                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"صرف قيمة مشتريات نقدية - فاتورة {invoice.PurchaseInvoiceNumber}"
                    });
                    invoice.IsPaid = true;
                    
                    // NEW: Record Expense for Cash Purchase!
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1, 
                        amount: invoice.TotalAmount, 
                        type: FinancialTransactionType.Expense, 
                        referenceType: ReferenceType.PurchaseInvoice, 
                        referenceId: invoice.Id, 
                        description: $"مشتريات نقدية - فاتورة {invoice.PurchaseInvoiceNumber}");
                }
                else
                {
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId)
                        ?? throw new KeyNotFoundException("المورد غير موجود");

                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = supplier.AccountId ?? 2101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"مشتريات آجلة - فاتورة {invoice.PurchaseInvoiceNumber}"
                    });

                    supplier.Balance += invoice.TotalAmount;
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                    invoice.IsPaid = false;
                }

                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.PurchaseInvoice);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("الفاتورة ملغاة بالفعل.");

            foreach (var detail in invoice.PurchaseInvoiceDetails)
            {
                var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                if (batch != null && batch.SoldQuantity > 0)
                {
                    throw new InvalidOperationException($"عذراً، لا يمكن إلغاء الفاتورة لأن الصنف '{batch.Medicine?.Name}' قد تم البدء ببيعه (تم بيع {batch.SoldQuantity}). يجب استخدام شاشة المرتجع.");
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                bool wasApproved = invoice.Status == DocumentStatus.Approved;
                invoice.Status = DocumentStatus.Cancelled;
                invoice.CancelledBy = userId;
                invoice.CancelledAt = DateTime.UtcNow;

                if (wasApproved)
                {
                    foreach (var detail in invoice.PurchaseInvoiceDetails)
                    {
                        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                        if (batch != null)
                        {
                            var quantity = detail.Quantity + detail.BonusQuantity;
                            batch.Quantity -= quantity;
                            batch.RemainingQuantity -= quantity;
                            if (batch.RemainingQuantity <= 0) batch.Status = "Empty";
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                            await DecreaseInventoryStockAsync(invoice.WarehouseId, detail.MedicineId, batch.CompanyBatchNumber, quantity);
                        }
                    }

                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.PurchaseInvoice);

                    if (invoice.PaymentMethod == PaymentType.Cash && invoice.IsPaid)
                    {
                        await _financialService.ProcessTransactionAsync(
                           accountId: 1,
                           amount: invoice.TotalAmount,
                           type: FinancialTransactionType.Income,
                           referenceType: ReferenceType.PurchaseInvoice,
                           referenceId: invoice.Id,
                           description: $"إلغاء فاتورة شراء (استرداد) - رقم: {invoice.PurchaseInvoiceNumber}");
                    }
                    else if (invoice.PaymentMethod == PaymentType.Credit)
                    {
                        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId);
                        if (supplier != null)
                        {
                            supplier.Balance -= invoice.TotalAmount; // Decrease Debt
                            await _unitOfWork.Suppliers.UpdateAsync(supplier);
                        }
                    }
                }

                await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UnapproveAsync(int id)
        {
            throw new InvalidOperationException("لا يمكن إلغاء الاعتماد مباشرة. يرجى استخدام خيار الإلغاء (Cancel) أو المرتجع (Return).");
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف فاتورة معتمدة أو ملغاة.");

            await _unitOfWork.PurchaseInvoices.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PayCreditInvoiceAsync(int invoiceId, int accountId = 1)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException($"الفاتورة غير موجودة");

            if (invoice.Status != DocumentStatus.Approved || invoice.PaymentMethod != PaymentType.Credit || invoice.IsPaid)
                throw new InvalidOperationException("الفاتورة غير قابلة للسداد.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _financialService.ProcessTransactionAsync(accountId, invoice.TotalAmount, FinancialTransactionType.Expense, ReferenceType.PurchaseInvoice, invoice.Id, $"سداد فاتورة: {invoice.PurchaseInvoiceNumber}");

                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId);
                if (supplier != null)
                {
                    supplier.Balance -= invoice.TotalAmount;
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                }

                invoice.IsPaid = true;
                await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<DTOs.Dashboard.PurchasesDashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var sevenDaysAgo = today.AddDays(-6);

            var allInvoices = await _unitOfWork.PurchaseInvoices.GetAllAsync();
            var approvedThisMonth = allInvoices
                .Where(p => p.Status == DocumentStatus.Approved && p.PurchaseDate >= startOfMonth)
                .ToList();

            var monthlyTotalPurchases = approvedThisMonth.Sum(p => p.TotalAmount);

            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            var supplierDebts = suppliers.Where(s => s.Balance > 0).Sum(s => s.Balance);

            var overdueCount = approvedThisMonth.Count(p => p.PaymentMethod == PaymentType.Credit && !p.IsPaid);

            var returns = await _unitOfWork.PurchaseReturns.GetAllAsync();
            var monthlyReturns = returns
                .Where(r => r.Status == DocumentStatus.Approved && r.ReturnDate >= startOfMonth)
                .Sum(r => r.TotalAmount);

            var returnRate = monthlyTotalPurchases > 0 ? (monthlyReturns / monthlyTotalPurchases) * 100 : 0;

            var supplierDistribution = approvedThisMonth
                .GroupBy(p => p.SupplierId)
                .Select(g => new { SupplierId = g.Key, TotalAmount = g.Sum(p => p.TotalAmount) })
                .OrderByDescending(x => x.TotalAmount)
                .Take(5)
                .ToList();

            var supplierNames = await _unitOfWork.Suppliers.GetAllAsync();
            var supplierDict = supplierNames.ToDictionary(s => s.Id, s => s.Name);

            var distribution = supplierDistribution.Select(sd => new DTOs.Dashboard.SupplierDistributionItem
            {
                SupplierName = supplierDict.GetValueOrDefault(sd.SupplierId, "غير معروف"),
                TotalAmount = sd.TotalAmount
            }).ToList();

            var last7DaysPurchases = new List<decimal>();
            var recentInvoices = allInvoices
                .Where(p => p.Status == DocumentStatus.Approved && p.PurchaseDate >= sevenDaysAgo)
                .ToList();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayTotal = recentInvoices
                    .Where(p => p.PurchaseDate.Date == date)
                    .Sum(p => p.TotalAmount);
                last7DaysPurchases.Add(dayTotal);
            }

            return new DTOs.Dashboard.PurchasesDashboardStatsDto
            {
                MonthlyTotalPurchases = monthlyTotalPurchases,
                SupplierDebts = supplierDebts,
                OverdueCount = overdueCount,
                MonthlyReturnsAmount = monthlyReturns,
                ReturnRate = returnRate,
                SupplierDistribution = distribution,
                Last7DaysPurchases = last7DaysPurchases
            };
        }

        public async Task<PurchaseInvoiceDto> CreateQuickPurchaseAsync(QuickPurchaseDto dto, int userId)
        {
            _logger.LogInformation("Creating Quick Purchase for medicine {MedicineId} by user {UserId}", dto.MedicineId, userId);

            if (dto.ExpiryDate.Date < DateTime.Today)
                throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({dto.ExpiryDate:yyyy-MM-dd}) غير صالح (قديم).");

            if (dto.SalePrice < dto.PurchasePrice)
                throw new InvalidOperationException($"عذراً، سعر البيع ({dto.SalePrice}) أقل من سعر الشراء ({dto.PurchasePrice}).");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = new PurchaseInvoice
                {
                    SupplierId = dto.SupplierId,
                    PurchaseDate = DateTime.UtcNow,
                    PaymentMethod = dto.PaymentMethod,
                    Notes = dto.Notes ?? "توريد سريع من قائمة الأدوية",
                    Status = DocumentStatus.Approved,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    ApprovedBy = userId,
                    ApprovedAt = DateTime.UtcNow,
                    WarehouseId = await ResolveReceivingWarehouseIdAsync(dto.WarehouseId),
                    PurchaseInvoiceNumber = await _invoiceNumberGenerator.GeneratePurchaseInvoiceNumberAsync()
                };

                int batchId = await GetOrCreateBatchIdAsync(dto.MedicineId, dto.BatchBarcode, dto.CompanyBatchNumber, dto.ExpiryDate, userId);

                var detail = new PurchaseInvoiceDetail
                {
                    MedicineId = dto.MedicineId,
                    BatchId = batchId,
                    Quantity = dto.Quantity,
                    BonusQuantity = dto.BonusQuantity,
                    PurchasePrice = dto.PurchasePrice,
                    SalePrice = dto.SalePrice,
                    Total = dto.Quantity * dto.PurchasePrice
                };

                invoice.PurchaseInvoiceDetails = new List<PurchaseInvoiceDetail> { detail };
                invoice.TotalAmount = detail.Total;

                var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(batchId)
                    ?? throw new KeyNotFoundException($"الدفعة {batchId} غير موجودة");

                batch.Quantity += (detail.Quantity + detail.BonusQuantity);
                batch.RemainingQuantity += (detail.Quantity + detail.BonusQuantity);
                batch.Status = "Active";

                decimal trueUnitCost = detail.Total / (detail.Quantity + detail.BonusQuantity);
                batch.UnitPurchasePrice = trueUnitCost;
                batch.RetailPrice = detail.SalePrice;
                batch.PurchaseInvoiceId = 0;
                detail.TrueUnitCost = trueUnitCost;

                await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                await IncreaseInventoryStockAsync(invoice.WarehouseId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, detail.Quantity + detail.BonusQuantity);

                var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                if (medicine != null)
                {
                    int totalStock = await _unitOfWork.MedicineBatches.GetTotalQuantityAsync(medicine.Id);
                    decimal oldVal = Math.Max(0, (totalStock - detail.Quantity - detail.BonusQuantity) * medicine.MovingAverageCost);
                    decimal newVal = oldVal + (detail.Total);
                    medicine.MovingAverageCost = totalStock > 0 ? newVal / totalStock : trueUnitCost;

                    medicine.DefaultSalePrice = detail.SalePrice;
                    medicine.DefaultPurchasePrice = detail.PurchasePrice;

                    await _unitOfWork.Medicines.UpdateAsync(medicine);
                }

                var journalEntry = new JournalEntryDto
                {
                    EntryDate = invoice.PurchaseDate,
                    VoucherNumber = invoice.PurchaseInvoiceNumber,
                    Description = $"قيد توريد سريع - فاتورة رقم: {invoice.PurchaseInvoiceNumber} - الصنف: {medicine?.Name}",
                    Type = VoucherType.PurchaseInvoice,
                    Lines = new List<JournalEntryLineDto>()
                };

                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 1301,
                    Debit = invoice.TotalAmount,
                    Credit = 0,
                    Description = $"توريد سريع للمخزون - {medicine?.Name}"
                });

                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"صرف نقدي لتوريد سريع - {medicine?.Name}"
                    });
                    invoice.IsPaid = true;

                    // NEW: Record Expense for Cash Purchase!
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1, 
                        amount: invoice.TotalAmount, 
                        type: FinancialTransactionType.Expense, 
                        referenceType: ReferenceType.PurchaseInvoice, 
                        referenceId: invoice.Id, 
                        description: $"مشتريات نقدية سريع - {medicine?.Name}");
                }
                else
                {
                    if (invoice.SupplierId <= 0) throw new InvalidOperationException("يجب اختيار مورد للشراء الآجل.");
                    
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId)
                        ?? throw new KeyNotFoundException("المورد غير موجود");
                    
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = supplier.AccountId ?? 2101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"توريد آجل سريع - {medicine?.Name}"
                    });
                    
                    supplier.Balance += invoice.TotalAmount;
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                    invoice.IsPaid = false;
                }

                await _unitOfWork.PurchaseInvoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                batch.PurchaseInvoiceId = invoice.Id;
                await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                await _unitOfWork.SaveChangesAsync();
                await _stockMovementService.ProcessDocumentMovementsAsync(invoice.Id, ReferenceType.PurchaseInvoice);

                await _unitOfWork.CommitAsync();

                try
                {
                    await _alertService.SyncMedicineAlertsAsync(dto.MedicineId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync alerts for medicine {MedicineId} after quick purchase", dto.MedicineId);
                }

                var finalInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(invoice.Id);
                return _mapper.Map<PurchaseInvoiceDto>(finalInvoice);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during Quick Purchase for medicine {MedicineId}", dto.MedicineId);
                throw;
            }
        }

        public async Task<BarcodeResultDto> ProcessBarcodeItemAsync(string barcode, int userId)
        {
            _logger.LogInformation("Processing barcode {Barcode} for purchase by user {UserId}", barcode, userId);

            var query = new GetProductForTransactionByBarcodeQuery
            {
                Barcode = barcode,
                TransactionType = TransactionType.Purchase
            };

            var result = await _barcodeService.GetProductByBarcodeAsync(query, userId)
                ?? throw new KeyNotFoundException("الصنف غير موجود في قاعدة البيانات. يرجى إضافة الصنف في شاشة الأدوية أولاً.");

            if (result.ExpiryDate < DateTime.Today && result.BatchId > 0)
            {
                _logger.LogWarning("Scanned batch for purchase is already expired: {Name}", result.TradeName);
            }

            return result;
        }

        private async Task<int> ResolveReceivingWarehouseIdAsync(int? requestedWarehouseId)
        {
            if (requestedWarehouseId.HasValue && requestedWarehouseId.Value > 0)
            {
                var selectedWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(requestedWarehouseId.Value)
                    ?? throw new KeyNotFoundException($"Warehouse {requestedWarehouseId.Value} was not found.");
                return selectedWarehouse.Id;
            }

            var currentBranchId = _currentUserService.GetCurrentBranchId()
                ?? throw new InvalidOperationException("Cannot resolve default receiving warehouse without a current branch.");

            var mainWarehouse = await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(currentBranchId, WarehouseType.Main);
            if (mainWarehouse != null)
                return mainWarehouse.Id;

            var branchWarehouse = await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(currentBranchId, WarehouseType.Branch);
            if (branchWarehouse != null)
                return branchWarehouse.Id;

            throw new InvalidOperationException($"No receiving warehouse is configured for branch {currentBranchId}.");
        }

        private async Task IncreaseInventoryStockAsync(int warehouseId, int medicineId, string batchNumber, DateTime expiryDate, int quantity)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByWarehouseMedicineBatchAsync(warehouseId, medicineId, batchNumber);
            if (stock == null)
            {
                stock = new InventoryStock
                {
                    WarehouseId = warehouseId,
                    MedicineId = medicineId,
                    BatchNumber = batchNumber,
                    ExpiryDate = expiryDate,
                    Quantity = quantity
                };
                await _unitOfWork.InventoryStocks.AddAsync(stock);
                return;
            }

            stock.Quantity += quantity;
            stock.ExpiryDate = expiryDate;
            await _unitOfWork.InventoryStocks.UpdateAsync(stock);
        }

        private async Task DecreaseInventoryStockAsync(int warehouseId, int medicineId, string batchNumber, int quantity)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByWarehouseMedicineBatchAsync(warehouseId, medicineId, batchNumber)
                ?? throw new InvalidOperationException($"Inventory stock for medicine {medicineId}, batch '{batchNumber}', warehouse {warehouseId} was not found.");

            if (stock.Quantity < quantity)
                throw new InvalidOperationException($"Inventory stock for medicine {medicineId}, batch '{batchNumber}', warehouse {warehouseId} is not enough to reverse the purchase.");

            stock.Quantity -= quantity;
            await _unitOfWork.InventoryStocks.UpdateAsync(stock);
        }
    }
}
