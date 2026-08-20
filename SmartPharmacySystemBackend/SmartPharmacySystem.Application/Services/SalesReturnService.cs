    using AutoMapper;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Application.Services
{
    public class SalesReturnService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SalesReturnService> logger,
        IStockMovementService stockMovementService,
        IJournalEntryService journalEntryService,
        IBarcodeService barcodeService,
        IShiftService shiftService) : ISalesReturnService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SalesReturnService> _logger = logger;
        private readonly IStockMovementService _stockMovementService = stockMovementService;
        private readonly IJournalEntryService _journalEntryService = journalEntryService;
        private readonly IBarcodeService _barcodeService = barcodeService;
        private readonly IShiftService _shiftService = shiftService;

        public async Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto, int userId)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل للمرتجع.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(dto.SaleInvoiceId)
                    ?? throw new KeyNotFoundException($"فاتورة البيع {dto.SaleInvoiceId} غير موجودة");

                if (invoice.Status != DocumentStatus.Approved)
                    throw new InvalidOperationException("لا يمكن عمل مرتجع لفاتورة غير معتمدة أو ملغاة.");

                // 1. Current user must have an open shift
                var currentShiftResponse = await _shiftService.GetCurrentShiftAsync();
                if (!currentShiftResponse.Success || currentShiftResponse.Data == null)
                {
                    throw new InvalidOperationException("لا يمكنك إجراء مرتجعات. يجب فتح وردية (صندوق) أولاً.");
                }

                // 2. Validate that the current date isn't closed (Firewall check)
                // We use DateTime.UtcNow for the return date
                // _closingValidationService will be injected later

                var entity = _mapper.Map<SalesReturn>(dto);
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = userId;
                entity.Status = DocumentStatus.Draft;
                entity.CustomerId = invoice.CustomerId;
                entity.UserShiftId = currentShiftResponse.Data.Id;

                entity.TotalAmount = 0;
                entity.TotalCost = 0;
                entity.TotalProfit = 0;

                foreach (var detail in entity.SalesReturnDetails)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId)
                        ?? throw new InvalidOperationException($"الصنف {detail.MedicineId} والتشغيلة {detail.BatchId} غير موجودين في الفاتورة الأصلية.");

                    if (detail.Quantity > originalLine.RemainingQtyToReturn)
                        throw new InvalidOperationException($"الكمية المرتجعة ({detail.Quantity}) تتجاوز الكمية المتاحة للإرجاع ({originalLine.RemainingQtyToReturn}) للصنف {detail.MedicineId}.");

                    detail.UnitCost = originalLine.UnitCost;
                    detail.SalePrice = originalLine.SalePrice;
                    detail.TotalCost = detail.Quantity * detail.UnitCost;
                    detail.TotalLineAmount = detail.Quantity * detail.SalePrice;
                    detail.Profit = detail.TotalLineAmount - detail.TotalCost;

                    entity.TotalAmount += detail.TotalLineAmount;
                    entity.TotalCost += detail.TotalCost;
                    entity.TotalProfit += detail.Profit;
                }

                await _unitOfWork.SalesReturns.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return _mapper.Map<SalesReturnDto>(entity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating sales return drafted");
                throw;
            }
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("المرتجع بالفعل معتمد أو ملغى.");

            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(ret.SaleInvoiceId)
                ?? throw new KeyNotFoundException($"فاتورة البيع {ret.SaleInvoiceId} غير موجودة");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var detail in ret.SalesReturnDetails)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == detail.MedicineId && d.BatchId == detail.BatchId)
                        ?? throw new InvalidOperationException("فشل في ربط المرتجع بالفاتورة الأصلية.");

                    if (detail.Quantity > originalLine.RemainingQtyToReturn)
                        throw new InvalidOperationException($"فشل الاعتماد: الكمية المرتجعة أكبر من المتبقي. المتاح: {originalLine.RemainingQtyToReturn}");

                    // 1. Update Inventory State (Exactly the original batch)
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                        ?? throw new KeyNotFoundException($"التشغيلة {detail.BatchId} غير موجودة");

                    // Security/Isolation: ensure the batch belongs to the same branch as the original invoice (when traceable).
                    if (batch.PurchaseInvoiceId.HasValue)
                    {
                        var purchaseInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(batch.PurchaseInvoiceId.Value);
                        if (purchaseInvoice == null || purchaseInvoice.BranchId != invoice.BranchId)
                            throw new InvalidOperationException("فشل اعتماد المرتجع: التشغيلة لا تتبع نفس فرع الفاتورة الأصلية.");
                    }

                    batch.RemainingQuantity += detail.Quantity;
                    batch.SoldQuantity -= detail.Quantity;
                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                    // 2. Update Original Invoice Remaining Qty
                    originalLine.RemainingQtyToReturn -= detail.Quantity;
                    await _unitOfWork.SaleInvoiceDetails.UpdateAsync(originalLine);
                }

                // 3. Profit Adjustment on Original Invoice (Optional: but good for gross profit reporting)
                invoice.TotalAmount -= ret.TotalAmount;
                invoice.TotalProfit -= ret.TotalProfit;
                invoice.TotalCost -= ret.TotalCost;
                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);

                // 4. Financial Impact ==================== المحرك المحاسبي الاحترافي ====================
                var journalEntry = new JournalEntryDto
                {
                    EntryDate = DateTime.UtcNow,
                    VoucherNumber = $"RET-{ret.Id}",
                    Description = $"قيد مردودات مبيعات آلي - رقم المرتجع: {ret.Id} - العميل: {invoice.CustomerName ?? "نقدي"}",
                    Type = VoucherType.JournalEntry,
                    Lines = new List<JournalEntryLineDto>()
                };

                // 1. الطرف المدين (من حـ/ الإيرادات - تخفيض الإيرادات)
                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = 41, // إيرادات المبيعات
                    Debit = ret.TotalAmount,
                    Credit = 0,
                    Description = $"مردودات مبيعات لفاتورة {invoice.SaleInvoiceNumber}"
                });

                // 2. الطرف الدائن (إلى حـ/ الصندوق أو العميل)
                if (invoice.PaymentMethod == PaymentType.Credit && invoice.CustomerId.HasValue)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 2101, // ذمم العملاء
                        Debit = 0,
                        Credit = ret.TotalAmount,
                        Description = $"تخفيض مديونية العميل بمرتجع {ret.Id}"
                    });
                    await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, -ret.TotalAmount);
                }
                else
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1101, // الصندوق الرئيسي
                        Debit = 0,
                        Credit = ret.TotalAmount,
                        Description = $"استرداد نقدي لمرتجع مبيعات {ret.Id}"
                    });
                }

                // 3. عكس قيد التكلفة (لتتبع الربحية الدقيقة)
                if (ret.TotalCost > 0)
                {
                    // من حـ/ المخزون
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 1301, // مخزون الصيدلية
                        Debit = ret.TotalCost,
                        Credit = 0,
                        Description = $"زيادة المخزون بمرتجع {ret.Id}"
                    });

                    // إلى حـ/ تكلفة المشتريات
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = 51, // تكلفة البضاعة المباعة
                        Debit = 0,
                        Credit = ret.TotalCost,
                        Description = $"عكس تكلفة مبيعات المرتجع {ret.Id}"
                    });
                }

                // حفظ وترحيل القيد
                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                // 5. Update Status
                ret.Status = DocumentStatus.Approved;
                ret.ApprovedBy = userId;
                ret.ApprovedAt = DateTime.UtcNow;
                await _unitOfWork.SalesReturns.UpdateAsync(ret);

                await _unitOfWork.SaveChangesAsync();

                // 6. Movements History
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SalesReturn);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving sales return {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("المرتجع ملغى بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = ret.Status == DocumentStatus.Approved;
                ret.Status = DocumentStatus.Cancelled;
                ret.CancelledBy = userId;
                ret.CancelledAt = DateTime.UtcNow;

                if (wasApproved)
                {
                    var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(ret.SaleInvoiceId);

                    // Reverse Approved Return
                    foreach (var detail in ret.SalesReturnDetails)
                    {
                        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                        if (batch != null)
                        {
                            // Security/Isolation: ensure the batch belongs to the same branch as the original invoice (when traceable).
                            if (invoice != null && batch.PurchaseInvoiceId.HasValue)
                            {
                                var purchaseInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(batch.PurchaseInvoiceId.Value);
                                if (purchaseInvoice == null || purchaseInvoice.BranchId != invoice.BranchId)
                                    throw new InvalidOperationException("فشل إلغاء المرتجع: التشغيلة لا تتبع نفس فرع الفاتورة الأصلية.");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sales return {Id}", id);
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateSalesReturnDto dto)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("التعديل مسموح فقط لحالة مسودة.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, entity);

                // Clear & Re-insert details
                entity.SalesReturnDetails ??= new List<SalesReturnDetail>();
                if (entity.SalesReturnDetails.Any())
                {
                    foreach (var detail in entity.SalesReturnDetails.ToList())
                    {
                        await _unitOfWork.SalesReturnDetails.DeleteAsync(detail.Id);
                    }
                    entity.SalesReturnDetails.Clear();
                }

                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(entity.SaleInvoiceId);
                entity.TotalAmount = 0;
                entity.TotalCost = 0;
                entity.TotalProfit = 0;

                foreach (var itemDto in dto.Details)
                {
                    var originalLine = invoice.SaleInvoiceDetails
                        .FirstOrDefault(d => d.MedicineId == itemDto.MedicineId && d.BatchId == itemDto.BatchId)
                        ?? throw new InvalidOperationException("الصنف غير موجود في الفاتورة الأصلية.");

                    var detail = _mapper.Map<SalesReturnDetail>(itemDto);
                    detail.SalesReturnId = id;
                    detail.UnitCost = originalLine.UnitCost;
                    detail.SalePrice = originalLine.SalePrice;
                    detail.TotalCost = detail.Quantity * detail.UnitCost;
                    detail.TotalLineAmount = detail.Quantity * detail.SalePrice;
                    detail.Profit = detail.TotalLineAmount - detail.TotalCost;

                    entity.TotalAmount += detail.TotalLineAmount;
                    entity.TotalCost += detail.TotalCost;
                    entity.TotalProfit += detail.Profit;
                    entity.SalesReturnDetails.Add(detail);
                }

                await _unitOfWork.SalesReturns.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المرتجع غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف مرتجع معتمد.");

            await _unitOfWork.SalesReturns.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SalesReturnDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المرتجع غير موجود");
            return _mapper.Map<SalesReturnDto>(entity);
        }

        public async Task<IEnumerable<SalesReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.SalesReturns.GetAllAsync();
            return _mapper.Map<IEnumerable<SalesReturnDto>>(entities);
        }

        public async Task<BarcodeResultDto> ProcessBarcodeItemAsync(string barcode, int userId)
        {
            _logger.LogInformation("Processing barcode {Barcode} for sales return by user {UserId}", barcode, userId);

            var query = new GetProductForTransactionByBarcodeQuery
            {
                Barcode = barcode,
                TransactionType = TransactionType.Return
            };

            var result = await _barcodeService.GetProductByBarcodeAsync(query, userId)
                ?? throw new KeyNotFoundException("الصنف غير موجود في قاعدة البيانات.");

            // For Sales Return, we don't necessarily check stock (returning items TO stock)
            // But we might want to ensure it's not expired or something if it's being returned for resale
            // Or just return the data and let the FE handle linking it to an invoice if needed
            
            return result;
        }
    }
}
