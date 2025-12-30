using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPurchaseReturnRepository
{
    Task<PurchaseReturn?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseReturn>> GetAllAsync();
    Task AddAsync(PurchaseReturn entity);
    Task UpdateAsync(PurchaseReturn entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id); // Adding for consistency
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<PurchaseReturn>> GetByPurchaseInvoiceIdAsync(int purchaseInvoiceId);
}
