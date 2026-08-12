using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Inventory;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Services
{
    public class AutomatedAuditService : IAutomatedAuditService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AutomatedAuditService(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<AutomatedAuditHeaderDto>> GetAllAsync(int page = 1, int pageSize = 10, int? warehouseId = null)
        {
            var branchId = _currentUserService.GetCurrentBranchId() ?? 1;

            var query = _context.AutomatedAuditHeaders
                .Include(h => h.Warehouse)
                .Include(h => h.CreatedByUser)
                .Where(h => h.BranchId == branchId);

            if (warehouseId.HasValue)
            {
                query = query.Where(h => h.WarehouseId == warehouseId.Value);
            }

            var totalCount = await query.CountAsync();

            var audits = await query
                .OrderByDescending(h => h.AuditDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new AutomatedAuditHeaderDto
                {
                    Id = h.Id,
                    AuditCode = h.AuditCode,
                    WarehouseId = h.WarehouseId,
                    WarehouseName = h.Warehouse != null ? h.Warehouse.Name : "جرد شامل لجميع المخازن",
                    BranchId = h.BranchId,
                    CountType = h.CountType,
                    CountTypeName = GetCountTypeName(h.CountType),
                    AuditDate = h.AuditDate,
                    CreatedByUserId = h.CreatedByUserId,
                    CreatedByUserName = h.CreatedByUser!.FullName,
                    Notes = h.Notes,
                    TotalOpeningValue = h.TotalOpeningValue,
                    TotalPurchasesValue = h.TotalPurchasesValue,
                    TotalSalesValue = h.TotalSalesValue,
                    TotalDamagesValue = h.TotalDamagesValue,
                    TotalShortageValue = h.TotalShortageValue
                })
                .ToListAsync();

            return new PagedResult<AutomatedAuditHeaderDto>(audits, totalCount, page, pageSize);
        }

        public async Task<AutomatedAuditHeaderDto> GetByIdAsync(int id)
        {
            var branchId = _currentUserService.GetCurrentBranchId() ?? 1;

            var audit = await _context.AutomatedAuditHeaders
                .Include(h => h.Warehouse)
                .Include(h => h.CreatedByUser)
                .Include(h => h.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(h => h.Items)
                    .ThenInclude(i => i.Batch)
                .Include(h => h.Items)
                    .ThenInclude(i => i.Warehouse)
                .FirstOrDefaultAsync(h => h.Id == id && h.BranchId == branchId);

            if (audit == null)
                throw new KeyNotFoundException("لم يتم العثور على تقرير الجرد الآلي");

            return new AutomatedAuditHeaderDto
            {
                Id = audit.Id,
                AuditCode = audit.AuditCode,
                WarehouseId = audit.WarehouseId,
                WarehouseName = audit.Warehouse != null ? audit.Warehouse.Name : "جرد شامل للفرع",
                BranchId = audit.BranchId,
                CountType = audit.CountType,
                CountTypeName = GetCountTypeName(audit.CountType),
                AuditDate = audit.AuditDate,
                CreatedByUserId = audit.CreatedByUserId,
                CreatedByUserName = audit.CreatedByUser!.FullName,
                Notes = audit.Notes,
                TotalOpeningValue = audit.TotalOpeningValue,
                TotalPurchasesValue = audit.TotalPurchasesValue,
                TotalSalesValue = audit.TotalSalesValue,
                TotalDamagesValue = audit.TotalDamagesValue,
                TotalShortageValue = audit.TotalShortageValue,
                Items = audit.Items.Select(i => new AutomatedAuditItemDto
                {
                    Id = i.Id,
                    AutomatedAuditHeaderId = i.AutomatedAuditHeaderId,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse?.Name ?? "الفرع الرئيسي",
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine?.Name ?? "",
                    BatchId = i.BatchId,
                    BatchNumber = i.Batch?.CompanyBatchNumber ?? "",
                    Barcode = i.Batch?.BatchBarcode,
                    ExpiryDate = i.Batch?.ExpiryDate,
                    OpeningBalance = i.OpeningBalance,
                    TotalPurchases = i.TotalPurchases,
                    TotalSales = i.TotalSales,
                    TotalTransfersIn = i.TotalTransfersIn,
                    TotalTransfersOut = i.TotalTransfersOut,
                    TotalDamages = i.TotalDamages,
                    TotalAdjustments = i.TotalAdjustments,
                    TotalSalesReturns = i.TotalSalesReturns,
                    TotalPurchaseReturns = i.TotalPurchaseReturns,
                    ExpectedSystemBalance = i.ExpectedSystemBalance,
                    ActualSystemBalance = i.ActualSystemBalance,
                    Variance = i.Variance,
                    UnitCost = i.UnitCost,
                    VarianceValue = i.VarianceValue
                }).ToList()
            };
        }

        public async Task<AutomatedAuditHeaderDto> GenerateAuditAsync(GenerateAuditRequestDto request)
        {
            var branchId = _currentUserService.GetCurrentBranchId() ?? 1;
            var userId = _currentUserService.UserId ?? 1;

            if (request.WarehouseId.HasValue)
            {
                var warehouseExists = await _context.Warehouses
                    .AnyAsync(w => w.Id == request.WarehouseId.Value && w.BranchId == branchId && !w.IsDeleted);

                if (!warehouseExists)
                    throw new KeyNotFoundException("المخزن المحدد غير موجود في الفرع الحالي");
            }

            var stocksQuery = _context.InventoryStocks
                .Include(s => s.Warehouse)
                .Include(s => s.Medicine)
                .Where(s => !s.IsDeleted && s.Warehouse.BranchId == branchId);

            if (request.WarehouseId.HasValue)
            {
                stocksQuery = stocksQuery.Where(s => s.WarehouseId == request.WarehouseId.Value);
            }

            var stocks = await stocksQuery
                .OrderBy(s => s.Warehouse.Name)
                .ThenBy(s => s.Medicine.Name)
                .ThenBy(s => s.BatchNumber)
                .ToListAsync();

            var medicineIds = stocks.Select(s => s.MedicineId).Distinct().ToList();
            var batchNumbers = stocks.Select(s => s.BatchNumber).Distinct().ToList();

            var batches = await _context.MedicineBatches
                .Include(b => b.Medicine)
                .Where(b => b.BranchId == branchId
                    && medicineIds.Contains(b.MedicineId)
                    && batchNumbers.Contains(b.CompanyBatchNumber))
                .ToListAsync();

            var batchesByStockKey = batches
                .GroupBy(b => new { b.MedicineId, BatchNumber = b.CompanyBatchNumber })
                .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.EntryDate).First());

            var batchIds = batches.Select(b => b.Id).ToList();
            var movements = await _context.InventoryMovements
                .Where(m => m.BranchId == branchId && m.BatchId != null && batchIds.Contains(m.BatchId.Value))
                .ToListAsync();

            var movementsByBatch = movements.GroupBy(m => m.BatchId).ToDictionary(g => g.Key, g => g.ToList());

            var auditHeader = new AutomatedAuditHeader
            {
                AuditCode = $"AUDIT-{DateTime.Now:yyyyMMddHHmmss}",
                WarehouseId = request.WarehouseId,
                BranchId = branchId,
                CountType = request.CountType,
                AuditDate = DateTime.Now,
                CreatedByUserId = userId,
                Notes = request.Notes,
                Items = new List<AutomatedAuditItem>()
            };

            decimal totalOpening = 0;
            decimal totalPurchases = 0;
            decimal totalSales = 0;
            decimal totalDamages = 0;
            decimal totalShortages = 0;

            foreach (var stock in stocks)
            {
                batchesByStockKey.TryGetValue(
                    new { stock.MedicineId, stock.BatchNumber },
                    out var batch);

                var batchMovements = batch != null && movementsByBatch.ContainsKey(batch.Id)
                    ? movementsByBatch[batch.Id]
                    : new List<InventoryMovement>();

                int openingBalance = 0;
                int totalPurch = batchMovements.Where(m => m.MovementType == StockMovementType.Purchase).Sum(m => m.Quantity);
                int totalSale = batchMovements.Where(m => m.MovementType == StockMovementType.Sale).Sum(m => m.Quantity);
                int totalTransIn = batchMovements.Where(m => m.MovementType == StockMovementType.TransferIn).Sum(m => m.Quantity);
                int totalTransOut = batchMovements.Where(m => m.MovementType == StockMovementType.TransferOut).Sum(m => m.Quantity);
                int totalDamage = batchMovements.Where(m => m.MovementType == StockMovementType.Damage).Sum(m => m.Quantity);
                int totalSalesRet = batchMovements.Where(m => m.MovementType == StockMovementType.SalesReturn).Sum(m => m.Quantity);
                int totalPurchRet = batchMovements.Where(m => m.MovementType == StockMovementType.PurchaseReturn).Sum(m => m.Quantity);
                int totalAdj = batchMovements.Where(m => m.MovementType == StockMovementType.Adjustment).Sum(m => m.Quantity);

                var openingMovements = batchMovements.Where(m => m.ReferenceType == ReferenceType.OpeningBalance).Sum(m => m.Quantity);
                openingBalance += openingMovements;

                int expectedBalance = openingBalance + totalPurch + totalSalesRet + totalTransIn 
                                    - totalSale - totalPurchRet - totalTransOut - totalDamage + totalAdj;

                int actualBalance = stock.Quantity;
                int variance = actualBalance - expectedBalance;

                decimal cost = batch?.UnitPurchasePrice ?? 0;
                decimal retailPrice = batch?.RetailPrice ?? 0;

                var item = new AutomatedAuditItem
                {
                    WarehouseId = stock.WarehouseId,
                    MedicineId = stock.MedicineId,
                    BatchId = batch?.Id,
                    OpeningBalance = openingBalance,
                    TotalPurchases = totalPurch,
                    TotalSales = totalSale,
                    TotalTransfersIn = totalTransIn,
                    TotalTransfersOut = totalTransOut,
                    TotalDamages = totalDamage,
                    TotalAdjustments = totalAdj,
                    TotalSalesReturns = totalSalesRet,
                    TotalPurchaseReturns = totalPurchRet,
                    ExpectedSystemBalance = expectedBalance,
                    ActualSystemBalance = actualBalance,
                    Variance = variance,
                    UnitCost = cost,
                    VarianceValue = variance * cost
                };

                auditHeader.Items.Add(item);

                totalOpening += openingBalance * cost;
                totalPurchases += totalPurch * cost;
                totalSales += totalSale * retailPrice;
                totalDamages += totalDamage * cost;
                if (variance < 0) 
                    totalShortages += Math.Abs(variance) * cost;
            }

            auditHeader.TotalOpeningValue = totalOpening;
            auditHeader.TotalPurchasesValue = totalPurchases;
            auditHeader.TotalSalesValue = totalSales;
            auditHeader.TotalDamagesValue = totalDamages;
            auditHeader.TotalShortageValue = totalShortages;

            _context.AutomatedAuditHeaders.Add(auditHeader);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(auditHeader.Id);
        }

        public async Task<AuditChartDataDto> GetAuditChartsAsync(int id)
        {
            var audit = await _context.AutomatedAuditHeaders
                .Include(h => h.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (audit == null)
                throw new KeyNotFoundException("لم يتم العثور على تقرير الجرد الآلي");

            var chartData = new AuditChartDataDto();

            // Top 10 medicines with highest sales in this audit
            var topSales = audit.Items
                .GroupBy(i => i.Medicine?.Name ?? "غير معروف")
                .Select(g => new {
                    Name = g.Key,
                    Sales = g.Sum(x => x.TotalSales),
                    Purchases = g.Sum(x => x.TotalPurchases),
                    Damages = g.Sum(x => x.TotalDamages),
                    Shortages = g.Sum(x => x.Variance < 0 ? Math.Abs(x.Variance) : 0)
                })
                .OrderByDescending(x => x.Sales)
                .Take(10)
                .ToList();

            foreach (var item in topSales)
            {
                chartData.Labels.Add(item.Name);
                chartData.SalesValues.Add(item.Sales);
                chartData.PurchasesValues.Add(item.Purchases);
                chartData.DamagesValues.Add(item.Damages);
                chartData.ShortagesValues.Add(item.Shortages);
            }

            return chartData;
        }

        public async Task DeleteAsync(int id)
        {
            var branchId = _currentUserService.GetCurrentBranchId() ?? 1;
            var audit = await _context.AutomatedAuditHeaders.FirstOrDefaultAsync(h => h.Id == id && h.BranchId == branchId);
            
            if (audit == null)
                throw new KeyNotFoundException("تقرير الجرد غير موجود");

            _context.AutomatedAuditHeaders.Remove(audit);
            await _context.SaveChangesAsync();
        }

        private static string GetCountTypeName(StockCountType type)
        {
            return type switch
            {
                StockCountType.Daily => "جرد يومي",
                StockCountType.Weekly => "جرد أسبوعي",
                StockCountType.Monthly => "جرد شهري",
                StockCountType.Quarterly => "جرد ربع سنوي",
                StockCountType.Annual => "جرد سنوي",
                StockCountType.SemiAnnual => "جرد نصف سنوي",
                StockCountType.BiAnnual => "جرد نصف سنوي",
                _ => "أخرى"
            };
        }
    }
}
