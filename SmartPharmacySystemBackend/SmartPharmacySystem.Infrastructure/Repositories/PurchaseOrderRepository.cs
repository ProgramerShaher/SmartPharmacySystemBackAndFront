using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// تطبيق مستودع أوامر الشراء (Purchase Order Repository Implementation)
/// </summary>
public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(int id)
    {
        return await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(d => d.Medicine)
            .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(d => d.SaleUnit)
            .Include(po => po.Supplier)
            .Include(po => po.ConvertedPurchaseInvoice)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted);
    }

    public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(d => d.Medicine)
            .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(d => d.SaleUnit)
            .Include(po => po.Supplier)
            .Include(po => po.ConvertedPurchaseInvoice)
            .Include(po => po.Branch)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted);
    }

    public async Task<PurchaseOrder?> GetByNumberAsync(string orderNumber)
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.PurchaseOrderDetails)
                .ThenInclude(d => d.Medicine)
            .Include(po => po.Supplier)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber && !po.IsDeleted);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(
        int? branchId = null,
        int? supplierId = null,
        PurchaseOrderStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Supplier)
            .Include(po => po.PurchaseOrderDetails)
            .Where(po => !po.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(po => po.BranchId == branchId.Value);

        if (supplierId.HasValue)
            query = query.Where(po => po.SupplierId == supplierId.Value);

        if (status.HasValue)
            query = query.Where(po => po.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(po => po.OrderDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(po => po.OrderDate <= dateTo.Value);

        return await query.OrderByDescending(po => po.OrderDate).ToListAsync();
    }

    public async Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? branchId,
        int? supplierId,
        PurchaseOrderStatus? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize)
    {
        var query = _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Supplier)
            .Include(po => po.PurchaseOrderDetails)
            .Where(po => !po.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(po => po.BranchId == branchId.Value);

        if (supplierId.HasValue)
            query = query.Where(po => po.SupplierId == supplierId.Value);

        if (status.HasValue)
            query = query.Where(po => po.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(po => po.OrderDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(po => po.OrderDate <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(po =>
                po.OrderNumber.Contains(search) ||
                (po.SupplierName != null && po.SupplierName.Contains(search)) ||
                (po.SupplierContact != null && po.SupplierContact.Contains(search)) ||
                (po.Supplier != null && po.Supplier.Name.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(po => po.OrderDate)
            .ThenByDescending(po => po.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<string> GetNextOrderNumberAsync(int branchId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PO-{year}-";
        var count = await _context.PurchaseOrders
            .IgnoreQueryFilters()
            .CountAsync(po => po.BranchId == branchId && po.OrderNumber.StartsWith(prefix));

        return $"{prefix}{(count + 1):D6}";
    }

    public async Task AddAsync(PurchaseOrder order)
    {
        await _context.PurchaseOrders.AddAsync(order);
    }

    public Task UpdateAsync(PurchaseOrder order)
    {
        _context.PurchaseOrders.Update(order);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order != null)
        {
            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            _context.PurchaseOrders.Update(order);
        }
    }
}
