using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace SmartPharmacySystem.Application.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StockMovementService> _logger;

        public StockMovementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockMovementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task ProcessDocumentMovementsAsync(int referenceId, ReferenceType type)
        {
            _logger.LogInformation("Processing movements for reference {ReferenceId} of type {Type}", referenceId, type);

            switch (type)
            {
                case ReferenceType.PurchaseInvoice:
                    await ProcessPurchaseInvoice(referenceId);
                    break;
                case ReferenceType.SaleInvoice:
                    await ProcessSaleInvoice(referenceId);
                    break;
                case ReferenceType.PurchaseReturn:
                    await ProcessPurchaseReturn(referenceId);
                    break;
                case ReferenceType.SalesReturn:
                    await ProcessSalesReturn(referenceId);
                    break;
                default:
                    throw new ArgumentException("نوع مرجع غير صالح للمزيد من الحركات التلقائية");
            }
        }

        public async Task CancelDocumentMovementsAsync(int referenceId, ReferenceType type)
        {
            _logger.LogInformation("Cancelling movements for reference {ReferenceId} of type {Type}", referenceId, type);
            var existingMovements = await _unitOfWork.InventoryMovements.GetMovementsByReferenceAsync(referenceId, type);

            foreach (var mov in existingMovements)
            {
                var reverseMov = new InventoryMovement(
                    mov.MedicineId,
                    mov.BatchId,
                    mov.MovementType,
                    mov.ReferenceType,
                    -mov.Quantity, // Reverse the quantity
                    mov.ReferenceId,
                    mov.ReferenceNumber,
                    mov.CreatedBy, // Should probably be current user, but keeping audit trail link
                    $"الإلفاء التلقائي للحركة رقم {mov.Id}"
                );
                await _unitOfWork.InventoryMovements.AddAsync(reverseMov);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CreateManualMovementAsync(CreateManualMovementDto dto)
        {
            if (dto.Type != StockMovementType.Damage && dto.Type != StockMovementType.Adjustment)
                throw new InvalidOperationException("الحركات اليدوية مسموحة فقط للتلف والتسويات الجردية.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new InvalidOperationException("يجب إدخال سبب للحركة اليدوية.");

            var quantity = dto.Type == StockMovementType.Damage ? -Math.Abs(dto.Quantity) : dto.Quantity;

            // Validate Stock if deduction
            if (quantity < 0)
            {
                await ValidateStockAvailability(dto.MedicineId, dto.BatchId, Math.Abs(quantity));
            }

            var movement = new InventoryMovement(
                dto.MedicineId,
                dto.BatchId,
                dto.Type,
                ReferenceType.Manual,
                quantity,
                0, // Manual doesn't have a document ID in current schema, or we use a virtual one
                "MANUAL",
                dto.ApprovedBy,
                dto.Reason
            );

            await _unitOfWork.InventoryMovements.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<StockMovementDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.InventoryMovements.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"حركة المخزون برقم {id} غير موجودة");
            return _mapper.Map<StockMovementDto>(entity);
        }

        public async Task<PagedResult<StockMovementDto>> SearchAsync(BaseQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.InventoryMovements.GetPagedAsync(
                query.Search ?? string.Empty,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDirection);

            var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(items);
            return new PagedResult<StockMovementDto>(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<int> GetCurrentBalanceAsync(int medicineId, int? batchId = null)
        {
            return await _unitOfWork.InventoryMovements.GetCurrentBalanceAsync(medicineId, batchId);
        }

        public async Task<IEnumerable<StockCardDto>> GetStockCardAsync(int medicineId, int? batchId = null)
        {
            var movements = await _unitOfWork.InventoryMovements.GetStockCardMovementsAsync(medicineId, batchId);
            var result = new List<StockCardDto>();
            int runningBalance = 0;

            foreach (var mov in movements)
            {
                runningBalance += mov.Quantity;
                result.Add(new StockCardDto
                {
                    Date = mov.Date,
                    MovementType = mov.MovementType,
                    QuantityChange = mov.Quantity,
                    ReferenceNumber = mov.ReferenceNumber,
                    RunningBalance = runningBalance,
                    Notes = mov.Notes
                });
            }

            return result.OrderByDescending(x => x.Date);
        }

        #region Private Helpers

        private async Task ProcessPurchaseInvoice(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("فاتورة الشراء غير موجودة");

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن توليد حركات مخزنية لفاتورة غير معتمدة.");

            foreach (var detail in invoice.PurchaseInvoiceDetails)
            {
                var mov = new InventoryMovement(
                    detail.MedicineId,
                    detail.BatchId,
                    StockMovementType.Purchase,
                    ReferenceType.PurchaseInvoice,
                    detail.Quantity,
                    invoice.Id,
                    invoice.SupplierInvoiceNumber ?? invoice.Id.ToString(),
                    invoice.CreatedBy,
                    "إضافة مشتريات تلقائية"
                );
                await _unitOfWork.InventoryMovements.AddAsync(mov);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ProcessSaleInvoice(int id)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("فاتورة المبيعات غير موجودة");

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن توليد حركات مخزنية لفاتورة غير معتمدة.");

            foreach (var detail in invoice.SaleInvoiceDetails)
            {
                // Validate Expiry
                var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                    ?? throw new KeyNotFoundException("التشغيلة غير موجودة");

                if (batch.IsExpired)
                    throw new InvalidOperationException($"فشل العملية: التشغيلة رقم {batch.CompanyBatchNumber} منتهية الصلاحية.");

                // Validate Stock
                await ValidateStockAvailability(detail.MedicineId, detail.BatchId, detail.Quantity);

                var mov = new InventoryMovement(
                    detail.MedicineId,
                    detail.BatchId,
                    StockMovementType.Sale,
                    ReferenceType.SaleInvoice,
                    -detail.Quantity,
                    invoice.Id,
                    invoice.Id.ToString(),
                    invoice.CreatedBy,
                    "خصم مبيعات تلقائي"
                );
                await _unitOfWork.InventoryMovements.AddAsync(mov);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ProcessPurchaseReturn(int id)
        {
            var ret = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("مرتجع المشتريات غير موجود");

            if (ret.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن توليد حركات مخزنية لمرتجع غير معتمد.");

            foreach (var detail in ret.PurchaseReturnDetails)
            {
                await ValidateStockAvailability(detail.MedicineId, detail.BatchId, detail.Quantity);

                var mov = new InventoryMovement(
                    detail.MedicineId,
                    detail.BatchId,
                    StockMovementType.PurchaseReturn,
                    ReferenceType.PurchaseReturn,
                    -detail.Quantity,
                    ret.Id,
                    ret.Id.ToString(),
                    ret.CreatedBy,
                    "خصم مرتجع مشتريات تلقائي"
                );
                await _unitOfWork.InventoryMovements.AddAsync(mov);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ProcessSalesReturn(int id)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("مرتجع المبيعات غير موجود");

            if (ret.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن توليد حركات مخزنية لمرتجع غير معتمد.");

            foreach (var detail in ret.SalesReturnDetails)
            {
                var mov = new InventoryMovement(
                    detail.MedicineId,
                    detail.BatchId,
                    StockMovementType.SalesReturn,
                    ReferenceType.SalesReturn,
                    detail.Quantity,
                    ret.Id,
                    ret.Id.ToString(),
                    1, // Default user if SalesReturn doesn't have CreatedBy in schema
                    "إضافة مرتجع مبيعات تلقائي"
                );
                await _unitOfWork.InventoryMovements.AddAsync(mov);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ValidateStockAvailability(int medicineId, int? batchId, int requestedQuantity)
        {
            var currentBalance = await GetCurrentBalanceAsync(medicineId, batchId);
            if (currentBalance < requestedQuantity)
            {
                throw new InvalidOperationException($"الرصيد المتاح غير كافٍ. المطلوب: {requestedQuantity}، المتوفر: {currentBalance}");
            }
        }

        #endregion
    }
}
