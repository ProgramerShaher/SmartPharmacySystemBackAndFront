using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartPharmacySystem.Application.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<StockMovementService> _logger;

        public StockMovementService(IUnitOfWork unitOfWork, IApplicationDbContext context, IMapper mapper, ILogger<StockMovementService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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

        public async Task<PagedResult<StockMovementDto>> SearchAsync(StockMovementQueryDto query)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 25 : query.PageSize;

            var movementsQuery = _context.InventoryMovements
                .AsNoTracking()
                .Include(m => m.Medicine)
                .Include(m => m.Batch)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                movementsQuery = movementsQuery.Where(m =>
                    m.ReferenceNumber.ToLower().Contains(search) ||
                    m.Notes.ToLower().Contains(search) ||
                    m.Medicine.Name.ToLower().Contains(search) ||
                    (m.Batch != null && m.Batch.CompanyBatchNumber.ToLower().Contains(search)));
            }

            if (query.MedicineId.HasValue)
                movementsQuery = movementsQuery.Where(m => m.MedicineId == query.MedicineId.Value);

            if (query.BatchId.HasValue)
                movementsQuery = movementsQuery.Where(m => m.BatchId == query.BatchId.Value);

            if (TryParseEnum<StockMovementType>(query.MovementType, out var movementType))
                movementsQuery = movementsQuery.Where(m => m.MovementType == movementType);

            if (TryParseEnum<ReferenceType>(query.ReferenceType, out var referenceType))
                movementsQuery = movementsQuery.Where(m => m.ReferenceType == referenceType);

            if (query.CreatedBy.HasValue)
                movementsQuery = movementsQuery.Where(m => m.CreatedBy == query.CreatedBy.Value);

            if (query.StartDate.HasValue)
                movementsQuery = movementsQuery.Where(m => m.Date >= query.StartDate.Value.Date);

            if (query.EndDate.HasValue)
            {
                var endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                movementsQuery = movementsQuery.Where(m => m.Date <= endDate);
            }

            var totalCount = await movementsQuery.CountAsync();

            var movements = await movementsQuery
                .OrderByDescending(m => m.Date)
                .ThenByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.MedicineId,
                    m.BatchId,
                    m.MovementType,
                    m.ReferenceType,
                    m.Quantity,
                    m.Date,
                    m.ReferenceId,
                    m.ReferenceNumber,
                    m.CreatedBy,
                    m.Notes,
                    MedicineName = m.Medicine.Name,
                    BatchNumber = m.Batch != null ? m.Batch.CompanyBatchNumber : null,
                    CreatedByName = _context.Users
                        .Where(u => u.Id == m.CreatedBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    FinancialDescription = _context.FinancialTransactions
                        .Where(f => f.ReferenceId == m.ReferenceId && f.ReferenceType == m.ReferenceType)
                        .Select(f => f.Description)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var dtos = movements.Select(m => new StockMovementDto
            {
                Id = m.Id,
                MedicineId = m.MedicineId,
                BatchId = m.BatchId,
                MovementType = m.MovementType,
                MovementTypeLabel = GetMovementTypeLabel(m.MovementType),
                ReferenceType = m.ReferenceType,
                ReferenceTypeLabel = GetReferenceTypeLabel(m.ReferenceType),
                Quantity = m.Quantity,
                Date = m.Date,
                ReferenceId = m.ReferenceId,
                ReferenceNumber = m.ReferenceNumber,
                CreatedBy = m.CreatedBy,
                CreatedByName = m.CreatedByName,
                Notes = m.Notes,
                FinancialDescription = m.FinancialDescription,
                MedicineName = m.MedicineName,
                BatchNumber = m.BatchNumber
            }).ToList();

            return new PagedResult<StockMovementDto>(dtos, totalCount, page, pageSize);
        }

        public async Task<StockMovementSummaryDto> GetSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var start = today.AddDays(-29);
            var end = today.AddDays(1);

            var activeBatches = _context.MedicineBatches
                .AsNoTracking()
                .Where(b => !b.IsDeleted && b.RemainingQuantity > 0);

            var totalStockValue = await activeBatches
                .SumAsync(b => (decimal?)b.RemainingQuantity * b.UnitPurchasePrice) ?? 0;

            var nearExpiryCount = await activeBatches
                .CountAsync(b => b.ExpiryDate.Date >= today && b.ExpiryDate.Date <= today.AddDays(30));

            var lowStockCount = await _context.Medicines
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .CountAsync(m => m.MedicineBatches
                    .Where(b => !b.IsDeleted)
                    .Sum(b => b.RemainingQuantity) <= m.ReorderLevel);

            var todayMovements = await _context.InventoryMovements
                .AsNoTracking()
                .CountAsync(m => m.Date >= today && m.Date < end);

            var rawTrend = await _context.InventoryMovements
                .AsNoTracking()
                .Where(m => m.Date >= start && m.Date < end)
                .GroupBy(m => m.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Additions = g.Where(m => m.Quantity > 0).Sum(m => m.Quantity),
                    Deductions = g.Where(m => m.Quantity < 0).Sum(m => -m.Quantity)
                })
                .ToListAsync();

            var trendLookup = rawTrend.ToDictionary(x => x.Date.Date);
            var trend = Enumerable.Range(0, 30)
                .Select(i =>
                {
                    var date = start.AddDays(i);
                    return trendLookup.TryGetValue(date, out var item)
                        ? new StockMovementTrendDto { Date = date, Additions = item.Additions, Deductions = item.Deductions }
                        : new StockMovementTrendDto { Date = date, Additions = 0, Deductions = 0 };
                })
                .ToList();

            var categoryDistribution = await activeBatches
                .GroupBy(b => b.Medicine.Category != null ? b.Medicine.Category.Name : "بدون تصنيف")
                .Select(g => new StockCategoryDistributionDto
                {
                    CategoryName = g.Key,
                    Quantity = g.Sum(b => b.RemainingQuantity),
                    Value = g.Sum(b => b.RemainingQuantity * b.UnitPurchasePrice)
                })
                .OrderByDescending(x => x.Value)
                .Take(8)
                .ToListAsync();

            return new StockMovementSummaryDto
            {
                TotalStockValue = totalStockValue,
                NearExpiryCount = nearExpiryCount,
                LowStockCount = lowStockCount,
                TodayMovements = todayMovements,
                Last30DaysTrend = trend,
                CategoryDistribution = categoryDistribution
            };
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
                throw new InvalidOperationException("لا يمكن توليد حركات مخزنية لفاتورة غير معتمد.");

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

                // Validate Stock - REMOVED: Redundant. SaleInvoiceService already validates against MedicineBatch.RemainingQuantity.
                // Relying on InventoryMovements sum causes errors if history is out of sync.
                // await ValidateStockAvailability(detail.MedicineId, detail.BatchId, detail.Quantity);

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
                    ret.CreatedBy,
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

        private static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return int.TryParse(value, out var number)
                ? Enum.IsDefined(typeof(TEnum), number) && Enum.TryParse(number.ToString(), out result)
                : Enum.TryParse(value, true, out result);
        }

        private static string GetMovementTypeLabel(StockMovementType type) => type switch
        {
            StockMovementType.Purchase => "توريد",
            StockMovementType.Sale => "بيع",
            StockMovementType.PurchaseReturn => "مردود مشتريات",
            StockMovementType.SalesReturn => "مردود مبيعات",
            StockMovementType.Adjustment => "تعديل مخزون",
            StockMovementType.Damage => "تالف",
            StockMovementType.Expiry => "منتهي الصلاحية",
            _ => "غير معروف"
        };

        private static string GetReferenceTypeLabel(ReferenceType type) => type switch
        {
            ReferenceType.PurchaseInvoice => "فاتورة مشتريات",
            ReferenceType.SaleInvoice => "فاتورة مبيعات",
            ReferenceType.PurchaseReturn => "مردود مشتريات",
            ReferenceType.SalesReturn => "مردود مبيعات",
            ReferenceType.Manual => "يدوي",
            ReferenceType.OpeningBalance => "رصيد افتتاحي",
            ReferenceType.ManualAdjustment => "تعديل يدوي",
            ReferenceType.SupplierPayment => "سند صرف مورد",
            ReferenceType.CustomerReceipt => "سند قبض عميل",
            _ => "غير معروف"
        };

        #endregion
    }
}
