using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع أوامر الشراء (Purchase Orders Repository Interface)
/// </summary>
public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(int id);
    Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id);
    Task<PurchaseOrder?> GetByNumberAsync(string orderNumber);
    Task<IEnumerable<PurchaseOrder>> GetAllAsync(int? branchId = null, int? supplierId = null, PurchaseOrderStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(string? search, int? branchId, int? supplierId, PurchaseOrderStatus? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
    Task<string> GetNextOrderNumberAsync(int branchId);
    Task AddAsync(PurchaseOrder order);
    Task UpdateAsync(PurchaseOrder order);
    Task DeleteAsync(int id);
}
