using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(int id);
    Task<IEnumerable<Warehouse>> GetAllAsync(int? branchId = null, WarehouseType? type = null);
    Task<IEnumerable<Warehouse>> GetByBranchIdAsync(int branchId);
    Task<Warehouse?> GetByBranchAndTypeAsync(int branchId, WarehouseType type);
    Task<Warehouse> AddAsync(Warehouse warehouse);
    Task UpdateAsync(Warehouse warehouse);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetWarehouseCountAsync(int branchId);
}
