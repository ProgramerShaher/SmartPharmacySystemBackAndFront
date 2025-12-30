using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.PurchaseReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class PurchaseReturnService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PurchaseReturnService> logger,
        IStockMovementService stockMovementService,
        IFinancialService financialService,
        IAlertService alertService) : IPurchaseReturnService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<PurchaseReturnService> _logger = logger;
        private readonly IStockMovementService _stockMovementService = stockMovementService;
        private readonly IFinancialService _financialService = financialService;
        private readonly IAlertService _alertService = alertService;

        public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int userId)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(dto.PurchaseInvoiceId)
                ?? throw new KeyNotFoundException("فاتورة الشراء غير موجودة");

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن عمل مرتجع لفاتورة غير معتمدة.");

            // Date Validation: Return date must not be older than purchase date
            if (dto.ReturnDate.Date < invoice.PurchaseDate.Date)
                throw new InvalidOperationException($"عذراً، لا يمكن أن يكون تاريخ المرتجع ({dto.ReturnDate:yyyy-MM-dd}) أقدم من تاريخ الفاتورة ({invoice.PurchaseDate:yyyy-MM-dd}).");

            var entity = _mapper.Map<PurchaseReturn>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = userId;
            entity.Status = DocumentStatus.Draft;
            entity.SupplierId = invoice.SupplierId;

            decimal calculatedTotal = 0;
            foreach (var detail in entity.PurchaseReturnDetails)
            {
                var originalLine = invoice.PurchaseInvoiceDetails
                    .FirstOrDefault(d => d.BatchId == detail.BatchId && d.MedicineId == detail.MedicineId)
                    ?? throw new InvalidOperationException("هذا الصنف غير موجود في الفاتورة الأصلية.");

                var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                if (batch != null && batch.RemainingQuantity < detail.Quantity)
                    throw new InvalidOperationException($"عذراً، الكمية المتوفرة في المخزن ({batch.RemainingQuantity}) أقل من الكمية المراد إرجاعها.");

                detail.TotalReturn = detail.Quantity * originalLine.PurchasePrice;
                calculatedTotal += detail.TotalReturn;
            }

            entity.TotalAmount = calculatedTotal;

            await _unitOfWork.PurchaseReturns.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.PurchaseReturns.GetByIdAsync(entity.Id);
            return _mapper.Map<PurchaseReturnDto>(created);
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var ret = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("مرتجع الشراء غير موجود");

            if (ret.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("المرتجع بالفعل معتمد أو ملغى.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var detail in ret.PurchaseReturnDetails)
                {
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                         ?? throw new KeyNotFoundException($"الدفعة {detail.BatchId} غير موجودة");

                    if (batch.RemainingQuantity < detail.Quantity)
                        throw new InvalidOperationException($"الكمية غير كافية في الدفعة {batch.CompanyBatchNumber}. المتاح: {batch.RemainingQuantity}");

                    // Validating the "No Sale" rule: "لا يسمح بإرجاع دواء قد تم بيعه"
                    // If SoldQuantity > 0, we check if the return quantity is still available.
                    // But the specific requirement says if it WAS sold, we block it.
                    if (batch.SoldQuantity > 0)
                        throw new InvalidOperationException($"عذراً، لا يمكن إرجاع هذا الصنف '{batch.Medicine?.Name}' للمورد لوجود مبيعات مرتبطة بالدفعة.");

                    batch.RemainingQuantity -= detail.Quantity;
                    batch.Quantity -= detail.Quantity;
                    if (batch.RemainingQuantity == 0) batch.Status = "Empty";

                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                }

                ret.Status = DocumentStatus.Approved;
                ret.ApprovedBy = userId;
                ret.ApprovedAt = DateTime.UtcNow;
                await _unitOfWork.PurchaseReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();

                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.PurchaseReturn);

                // Financial Integration
                var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(ret.PurchaseInvoiceId);
                if (invoice != null)
                {
                    if (invoice.PaymentMethod == PaymentType.Cash)
                    {
                        await _financialService.ProcessTransactionAsync(1, ret.TotalAmount, FinancialTransactionType.Income, ReferenceType.PurchaseReturn, ret.Id, $"مرتجع شراء - استرداد نقدي");
                    }
                    else
                    {
                        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId);
                        if (supplier != null)
                        {
                            supplier.Balance -= ret.TotalAmount; // Decrease Debt
                            await _unitOfWork.Suppliers.UpdateAsync(supplier);
                        }
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving purchase return");
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var ret = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("المرتجع غير موجود");

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
                    foreach (var detail in ret.PurchaseReturnDetails)
                    {
                        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                        if (batch != null)
                        {
                            batch.RemainingQuantity += detail.Quantity;
                            batch.Quantity += detail.Quantity;
                            batch.Status = "Active";
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);
                        }
                    }

                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.PurchaseReturn);

                    var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(ret.PurchaseInvoiceId);
                    if (invoice != null)
                    {
                        if (invoice.PaymentMethod == PaymentType.Cash)
                        {
                            await _financialService.ProcessTransactionAsync(1, ret.TotalAmount, FinancialTransactionType.Expense, ReferenceType.PurchaseReturn, ret.Id, $"إلغاء مرتجع شراء - خصم");
                        }
                        else
                        {
                            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(invoice.SupplierId);
                            if (supplier != null)
                            {
                                supplier.Balance += ret.TotalAmount; // Increase Debt back
                                await _unitOfWork.Suppliers.UpdateAsync(supplier);
                            }
                        }
                    }
                }

                await _unitOfWork.PurchaseReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdatePurchaseReturnDto dto)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("المرتجع غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل مرتجع معتمد.");

            _mapper.Map(dto, entity);
            await _unitOfWork.PurchaseReturns.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id);
            if (entity != null && entity.Status == DocumentStatus.Draft)
            {
                await _unitOfWork.PurchaseReturns.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<PurchaseReturnDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id);
            return _mapper.Map<PurchaseReturnDto>(entity);
        }

        public async Task<IEnumerable<PurchaseReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.PurchaseReturns.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseReturnDto>>(entities);
        }
    }
}
