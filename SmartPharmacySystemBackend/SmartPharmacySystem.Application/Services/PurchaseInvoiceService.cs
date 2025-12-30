using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.DTOs.PurchaseInvoice;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

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
        private readonly IAlertService _alertService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public PurchaseInvoiceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PurchaseInvoiceService> logger,
            IStockMovementService stockMovementService,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IFinancialService financialService,
            IAlertService alertService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _financialService = financialService;
            _alertService = alertService;
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
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
                await _unitOfWork.CommitAsync();

                var createdInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(invoice.Id);
                return _mapper.Map<PurchaseInvoiceDto>(createdInvoice);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task<int> GetOrCreateBatchIdAsync(int medicineId, string? barcode, string? companyBatch, DateTime expiry, int userId)
        {
            MedicineBatch? existingBatch = null;
            if (!string.IsNullOrEmpty(barcode))
            {
                existingBatch = await _unitOfWork.MedicineBatches.GetByBarcodeAsync(barcode);
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

            _mapper.Map(dto, invoice);
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

                // Financial Integration
                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1,
                        amount: invoice.TotalAmount,
                        type: FinancialTransactionType.Expense,
                        referenceType: ReferenceType.PurchaseInvoice,
                        referenceId: invoice.Id,
                        description: $"شراء نقدي - فاتورة رقم: {invoice.PurchaseInvoiceNumber}");
                    invoice.IsPaid = true;
                }
                else
                {
                    var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId)
                        ?? throw new KeyNotFoundException("المورد غير موجود");
                    supplier.Balance += invoice.TotalAmount; // Increase Debt
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                    invoice.IsPaid = false;
                }

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
    }
}
