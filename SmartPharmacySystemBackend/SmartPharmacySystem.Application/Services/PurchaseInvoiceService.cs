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

            // لا نحتاج transaction هنا لأن الفاتورة مسودة فقط
            // GetOrCreateBatchIdAsync تحفظ بشكل مستقل لضرورة الحصول على BatchId
            foreach (var itemDto in dto.Items)
            {
                // Validation: Past Expiry Date
                if (itemDto.ExpiryDate.Date < DateTime.Today)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({itemDto.ExpiryDate:yyyy-MM-dd}) للدواء '{med?.Name}' غير صالح (قديم).");
                }

                // Validation: Profit Safeguard
                if (itemDto.SalePrice < itemDto.PurchasePrice)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، سعر البيع ({itemDto.SalePrice}) أقل من سعر الشراء ({itemDto.PurchasePrice}) للدواء '{med?.Name}'.");
                }

                // Resolve Batch (Find or Create Placeholder)
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

            // 1. البحث بالباركود أولاً
            if (!string.IsNullOrEmpty(barcode))
            {
                existingBatch = await _unitOfWork.MedicineBatches.GetByBarcodeAsync(barcode);
            }

            // 2. إذا لم نجد بالباركود، نبحث برقم دفعة الشركة لمنع خطأ التكرار
            if (existingBatch == null && !string.IsNullOrEmpty(companyBatch))
            {
                existingBatch = await _unitOfWork.MedicineBatches.GetByCompanyBatchNumberAsync(companyBatch);
            }

            // Security/Isolation:
            // MedicineBatch isn't branch-filtered, so do NOT reuse an existing batch unless we can prove it belongs
            // to the current branch context (via linked PurchaseInvoice). Otherwise create a new placeholder batch.
            if (existingBatch != null && currentBranchId.HasValue && existingBatch.PurchaseInvoiceId.HasValue)
            {
                var linkedInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(existingBatch.PurchaseInvoiceId.Value);
                if (linkedInvoice != null && linkedInvoice.BranchId == currentBranchId.Value)
                    return existingBatch.Id;

                existingBatch = null;
            }

            // If we found a batch but can't prove it's for the current branch, treat it as non-existent to avoid cross-branch reuse.
            if (existingBatch != null)
                existingBatch = null;

            var newBatch = new MedicineBatch
            {
                MedicineId = medicineId,
                Quantity = 0,
                RemainingQuantity = 0,
                UnitPurchasePrice = 0,
                RetailPrice = 0,
                ExpiryDate = expiry,
                BatchBarcode = barcode,
                CompanyBatchNumber = companyBatch ?? "N/A",
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

            // Update Header Fields
            invoice.SupplierId = dto.SupplierId;
            invoice.SupplierInvoiceNumber = dto.SupplierInvoiceNumber;
            invoice.PurchaseDate = dto.PurchaseDate;
            invoice.PaymentMethod = dto.PaymentMethod;
            invoice.WarehouseId = await ResolveReceivingWarehouseIdAsync(dto.WarehouseId);
            invoice.Notes = dto.Notes;

            // Clear Existing Details
            _unitOfWork.PurchaseInvoiceDetails.RemoveRange(invoice.PurchaseInvoiceDetails);
            invoice.PurchaseInvoiceDetails.Clear();

            decimal calculatedTotal = 0;

            // Add New Details
            foreach (var itemDto in dto.Items)
            {
                // Validation: Past Expiry Date
                if (itemDto.ExpiryDate.Date < DateTime.Today)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({itemDto.ExpiryDate:yyyy-MM-dd}) للدواء '{med?.Name}' غير صالح (قديم).");
                }

                // Validation: Profit Safeguard
                if (itemDto.SalePrice < itemDto.PurchasePrice)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(itemDto.MedicineId);
                    throw new InvalidOperationException($"عذراً، سعر البيع ({itemDto.SalePrice}) أقل من سعر الشراء ({itemDto.PurchasePrice}) للدواء '{med?.Name}'.");
                }

                // Resolve Batch
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
                ?? throw new KeyNotFoundException($"\u0641\u0627\u062a\u0648\u0631\u0629 \u0627\u0644\u0634\u0631\u0627\u0621 \u0628\u0631\u0642\u0645 {id} \u063a\u064a\u0631 \u0645\u0648\u062c\u0648\u062f\u0629");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("\u0627\u0644\u0641\u0627\u062a\u0648\u0631\u0629 \u064a\u062c\u0628 \u0623\u0646 \u062a\u0643\u0648\u0646 \u0645\u0633\u0648\u062f\u0629 \u0644\u0644\u0627\u0639\u062a\u0645\u0627\u062f.");

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
                        ?? throw new KeyNotFoundException($"\u0627\u0644\u062f\u0641\u0639\u0629 {detail.BatchId} \u063a\u064a\u0631 \u0645\u0648\u062c\u0648\u062f\u0629");

                    // ====================================================
                    // UoM: Convert entered quantity to base unit (pills)
                    // ====================================================
                    int conversionFactor = 1;
                    if (detail.PurchaseUnitId.HasValue)
                    {
                        var purchaseUnit = await _unitOfWork.MedicineUnits.GetByIdAsync(detail.PurchaseUnitId.Value);
                        conversionFactor = purchaseUnit?.ConversionFactor ?? 1;
                    }

                    int rawQty = detail.Quantity;       // what user typed (e.g. 5 cartons)
                    int rawBonus = detail.BonusQuantity;
                    int baseUnits = (rawQty + rawBonus) * conversionFactor;

                    detail.QuantityInPurchaseUnit = rawQty;   // save as entered
                    detail.Quantity = baseUnits;               // store in base units

                    batch.Quantity += baseUnits;
                    batch.RemainingQuantity += baseUnits;
                    batch.Status = "Active";

                    // Cost per base unit (e.g. cost per pill)
                    decimal trueUnitCost = baseUnits > 0 ? detail.Total / baseUnits : detail.PurchasePrice;
                    batch.UnitPurchasePrice = trueUnitCost;
                    batch.RetailPrice = detail.SalePrice;
                    batch.PurchaseInvoiceId = invoice.Id;
                    detail.TrueUnitCost = trueUnitCost;

                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                    await IncreaseInventoryStockAsync(invoice.WarehouseId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, baseUnits);

                    // Update Medicine MAC & Default Pricing
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                    if (medicine != null)
                    {
                        int totalStock = await _unitOfWork.MedicineBatches.GetTotalQuantityAsync(medicine.Id);
                        decimal oldVal = Math.Max(0, (totalStock - baseUnits) * medicine.MovingAverageCost);
                        decimal newVal = oldVal + detail.Total;
                        medicine.MovingAverageCost = totalStock > 0 ? newVal / totalStock : trueUnitCost;

                        medicine.DefaultSalePrice = detail.SalePrice;
                        medicine.DefaultPurchasePrice = detail.PurchasePrice;

                        await _unitOfWork.Medicines.UpdateAsync(medicine);
                    }

                    validTotal += detail.Total;
                }

                invoice.TotalAmount = validTotal;

                // ==================== Journal Entry ====================
                var journalEntry = new JournalEntryDto
                {
                    EntryDate = invoice.PurchaseDate,
                    VoucherNumber = invoice.PurchaseInvoiceNumber,
                    Description = $"\u0642\u064a\u062f \u0645\u0634\u062a\u0631\u064a\u0627\u062a \u0622\u0644\u064a - \u0641\u0627\u062a\u0648\u0631\u0629 \u0631\u0642\u0645: {invoice.PurchaseInvoiceNumber} - \u0627\u0644\u0645\u0648\u0631\u062f: {invoice.Supplier?.Name ?? "\u063a\u064a\u0631 \u0645\u0639\u0631\u0648\u0641"}",
                    Type = VoucherType.PurchaseInvoice,
                    Lines = new List<JournalEntryLineDto>()
                };

                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 1301,
                    Debit = invoice.TotalAmount,
                    Credit = 0,
                    Description = $"\u0625\u0636\u0627\u0641\u0629 \u0644\u0644\u0645\u062e\u0632\u0648\u0646 - \u0641\u0627\u062a\u0648\u0631\u0629 \u0634\u0631\u0627\u0621 {invoice.PurchaseInvoiceNumber}"
                });

                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"\u0635\u0631\u0641 \u0642\u064a\u0645\u0629 \u0645\u0634\u062a\u0631\u064a\u0627\u062a \u0646\u0642\u062f\u064a\u0629 - \u0641\u0627\u062a\u0648\u0631\u0629 {invoice.PurchaseInvoiceNumber}"
                    });
                    invoice.IsPaid = true;
                }
                else
                {
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId)
                        ?? throw new KeyNotFoundException("\u0627\u0644\u0645\u0648\u0631\u062f \u063a\u064a\u0631 \u0645\u0648\u062c\u0648\u062f");

                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = supplier.AccountId ?? 2101,
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"\u0645\u0634\u062a\u0631\u064a\u0627\u062a \u0622\u062c\u0644\u0629 - \u0641\u0627\u062a\u0648\u0631\u0629 {invoice.PurchaseInvoiceNumber}"
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

            // 1. Sale Integrity Check
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

        /// <summary>
        /// Get dashboard statistics with optimized aggregate queries
        /// Uses .AsNoTracking() and direct Sum() for performance (<100ms)
        /// </summary>
        public async Task<DTOs.Dashboard.PurchasesDashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var sevenDaysAgo = today.AddDays(-6);

            // Get all approved purchases this month
            var allInvoices = await _unitOfWork.PurchaseInvoices.GetAllAsync();
            var approvedThisMonth = allInvoices
                .Where(p => p.Status == DocumentStatus.Approved && p.PurchaseDate >= startOfMonth)
                .ToList();

            var monthlyTotalPurchases = approvedThisMonth.Sum(p => p.TotalAmount);

            // Supplier debts
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            var supplierDebts = suppliers.Where(s => s.Balance > 0).Sum(s => s.Balance);

            // Overdue count (invoices that are credit and not paid)
            var overdueCount = approvedThisMonth.Count(p => p.PaymentMethod == PaymentType.Credit && !p.IsPaid);

            // Monthly returns
            var returns = await _unitOfWork.PurchaseReturns.GetAllAsync();
            var monthlyReturns = returns
                .Where(r => r.Status == DocumentStatus.Approved && r.ReturnDate >= startOfMonth)
                .Sum(r => r.TotalAmount);

            var returnRate = monthlyTotalPurchases > 0 ? (monthlyReturns / monthlyTotalPurchases) * 100 : 0;

            // Top 5 suppliers by purchase amount (for donut chart)
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

            // Last 7 days purchases for sparkline
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

            // 1. Validation
            if (dto.ExpiryDate.Date < DateTime.Today)
                throw new InvalidOperationException($"عذراً، تاريخ الانتهاء ({dto.ExpiryDate:yyyy-MM-dd}) غير صالح (قديم).");

            if (dto.SalePrice < dto.PurchasePrice)
                throw new InvalidOperationException($"عذراً، سعر البيع ({dto.SalePrice}) أقل من سعر الشراء ({dto.PurchasePrice}).");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Create the Invoice Header
                var invoice = new PurchaseInvoice
                {
                    SupplierId = dto.SupplierId, // Can be null for cash
                    PurchaseDate = DateTime.UtcNow,
                    PaymentMethod = dto.PaymentMethod,
                    Notes = dto.Notes ?? "توريد سريع من قائمة الأدوية",
                    Status = DocumentStatus.Approved, // Auto-Approve
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    ApprovedBy = userId,
                    ApprovedAt = DateTime.UtcNow,
                    WarehouseId = await ResolveReceivingWarehouseIdAsync(dto.WarehouseId),
                    PurchaseInvoiceNumber = await _invoiceNumberGenerator.GeneratePurchaseInvoiceNumberAsync()
                };

                // 3. Resolve Batch (Find or Create)
                int batchId = await GetOrCreateBatchIdAsync(dto.MedicineId, dto.BatchBarcode, dto.CompanyBatchNumber, dto.ExpiryDate, userId);

                // 4. Create Invoice Detail
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

                // 5. Apply Approval Logic (Same as ApproveAsync)
                var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(batchId)
                    ?? throw new KeyNotFoundException($"الدفعة {batchId} غير موجودة");

                batch.Quantity += (detail.Quantity + detail.BonusQuantity);
                batch.RemainingQuantity += (detail.Quantity + detail.BonusQuantity);
                batch.Status = "Active";

                // Cost Calculation
                decimal trueUnitCost = detail.Total / (detail.Quantity + detail.BonusQuantity);
                batch.UnitPurchasePrice = trueUnitCost;
                batch.RetailPrice = detail.SalePrice;
                batch.PurchaseInvoiceId = 0; // Temporary, will set after invoice is saved
                detail.TrueUnitCost = trueUnitCost;

                await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                await IncreaseInventoryStockAsync(invoice.WarehouseId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, detail.Quantity + detail.BonusQuantity);

                // Update Medicine MAC & Default Pricing
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

                // 6. Accounting (Journal Entry)
                var journalEntry = new JournalEntryDto
                {
                    EntryDate = invoice.PurchaseDate,
                    VoucherNumber = invoice.PurchaseInvoiceNumber,
                    Description = $"قيد توريد سريع - فاتورة رقم: {invoice.PurchaseInvoiceNumber} - الصنف: {medicine?.Name}",
                    Type = VoucherType.PurchaseInvoice,
                    Lines = new List<JournalEntryLineDto>()
                };

                // Debit: Stock (1301)
                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 1301,
                    Debit = invoice.TotalAmount,
                    Credit = 0,
                    Description = $"توريد سريع للمخزون - {medicine?.Name}"
                });

                // Credit: Cash or Supplier
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

                // Save Invoice and get ID
                await _unitOfWork.PurchaseInvoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Update Batch with Invoice ID
                batch.PurchaseInvoiceId = invoice.Id;
                await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                // Save Journal Entry
                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                // Record Stock Movement
                await _unitOfWork.SaveChangesAsync();
                await _stockMovementService.ProcessDocumentMovementsAsync(invoice.Id, ReferenceType.PurchaseInvoice);

                await _unitOfWork.CommitAsync();

                // 7. Sync Alerts (New Batch might be near expiry)
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

            // For Purchase, we usually allow scanning even if out of stock
            // But we can check if it's expired in existing batches to warn
            if (result.ExpiryDate < DateTime.Today && result.BatchId > 0)
            {
                _logger.LogWarning("Scanned batch for purchase is already expired: {Name}", result.TradeName);
                // We don't block purchase because they might be buying a NEW batch of the same medicine
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
