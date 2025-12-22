using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPurchaseReturnDetailRepository
{
    Task<PurchaseReturnDetail?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseReturnDetail>> GetAllAsync();
    Task AddAsync(PurchaseReturnDetail entity);
    Task UpdateAsync(PurchaseReturnDetail entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
