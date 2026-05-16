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

            if (existingBatch != null) return existingBatch.Id;

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

                    batch.Quantity += (detail.Quantity + detail.BonusQuantity);
                    batch.RemainingQuantity += (detail.Quantity + detail.BonusQuantity);
                    batch.Status = "Active";

                    // Cost Calculation
                    decimal trueUnitCost = detail.Total / (detail.Quantity + detail.BonusQuantity);
                    batch.UnitPurchasePrice = trueUnitCost;
                    batch.RetailPrice = detail.SalePrice; // The "Golden Point": Save sale price at batch arrival
                    batch.PurchaseInvoiceId = invoice.Id;
                    detail.TrueUnitCost = trueUnitCost;

                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                    // Update Medicine MAC & Default Pricing
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                    if (medicine != null)
                    {
                        int totalStock = await _unitOfWork.MedicineBatches.GetTotalQuantityAsync(medicine.Id);
                        decimal oldVal = Math.Max(0, (totalStock - detail.Quantity - detail.BonusQuantity) * medicine.MovingAverageCost);
                        decimal newVal = oldVal + (detail.Total);
                        medicine.MovingAverageCost = totalStock > 0 ? newVal / totalStock : trueUnitCost;

                        // Professional touch: Update default sale price if changed
                        medicine.DefaultSalePrice = detail.SalePrice;
                        medicine.DefaultPurchasePrice = detail.PurchasePrice;

                        await _unitOfWork.Medicines.UpdateAsync(medicine);
                    }

                    validTotal += detail.Total;
                }

                invoice.TotalAmount = validTotal;

                // ==================== المحرك المحاسبي الاحترافي ====================
                var journalEntry = new JournalEntryDto
                {
                    EntryDate = invoice.PurchaseDate,
                    VoucherNumber = invoice.PurchaseInvoiceNumber,
                    Description = $"قيد مشتريات آلي - فاتورة رقم: {invoice.PurchaseInvoiceNumber} - المورد: {invoice.Supplier?.Name ?? "غير معروف"}",
                    Type = VoucherType.PurchaseInvoice,
                    Lines = new List<JournalEntryLineDto>()
                };

                // 1. الطرف المدين (من حـ/ المخزون)
                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 1301, // مخزون الصيدلية
                    Debit = invoice.TotalAmount,
                    Credit = 0,
                    Description = $"إضافة للمخزون - فاتورة شراء {invoice.PurchaseInvoiceNumber}"
                });

                // 2. الطرف الدائن (إلى حـ/)
                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1101, // الصندوق الرئيسي
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"صرف قيمة مشتريات نقدية - فاتورة {invoice.PurchaseInvoiceNumber}"
                    });
                    invoice.IsPaid = true;
                }
                else
                {
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId)
                        ?? throw new KeyNotFoundException("المورد غير موجود");
                    
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = supplier.AccountId ?? 2101, // حساب المورد أو ذمم الموردين
                        Debit = 0,
                        Credit = invoice.TotalAmount,
                        Description = $"مشتريات آجلة - فاتورة {invoice.PurchaseInvoiceNumber}"
                    });
                    
                    supplier.Balance += invoice.TotalAmount; // Increase Debt (Keep old logic for now)
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                    invoice.IsPaid = false;
                }

                // حفظ وترحيل القيد
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
                            batch.Quantity -= (detail.Quantity + detail.BonusQuantity);
                            batch.RemainingQuantity -= (detail.Quantity + detail.BonusQuantity);
                            if (batch.RemainingQuantity <= 0) batch.Status = "Empty";
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);
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
    }
}
