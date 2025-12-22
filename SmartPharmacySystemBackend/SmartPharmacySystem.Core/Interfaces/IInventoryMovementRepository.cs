using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Interface for inventory movement repository operations.
/// </summary>
public interface IInventoryMovementRepository
{
    Task<InventoryMovement> GetByIdAsync(int id);
    Task<IEnumerable<InventoryMovement>> GetAllAsync();
    Task AddAsync(InventoryMovement entity);
    Task<bool> ExistsAsync(int id);
    Task<(IEnumerable<InventoryMovement> Items, int TotalCount)> GetPagedAsync(string search, int page, int pageSize, string sortBy, string sortDirection);

    Task<int> GetCurrentBalanceAsync(int medicineId, int? batchId = null);
    Task<IEnumerable<InventoryMovement>> GetStockCardMovementsAsync(int medicineId, int? batchId = null);
    Task<IEnumerable<InventoryMovement>> GetMovementsByReferenceAsync(int referenceId, SmartPharmacySystem.Core.Enums.ReferenceType type);
}
