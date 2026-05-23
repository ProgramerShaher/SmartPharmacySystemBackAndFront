using SmartPharmacySystem.Application.DTOs.Warehouses;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IWarehouseService
{
    Task<WarehouseDto> GetByIdAsync(int id);
    Task<IEnumerable<WarehouseDto>> GetAllAsync(int? branchId = null, WarehouseType? type = null);
    Task<IEnumerable<WarehouseDto>> GetByBranchIdAsync(int branchId);
    Task<WarehouseDto?> GetByBranchAndTypeAsync(int branchId, WarehouseType type);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
    Task UpdateAsync(UpdateWarehouseDto dto, int id);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetWarehouseCountAsync(int branchId);
}
