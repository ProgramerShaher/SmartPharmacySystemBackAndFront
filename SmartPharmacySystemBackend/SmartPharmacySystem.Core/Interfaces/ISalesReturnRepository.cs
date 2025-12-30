using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Interface for sales return repository operations.
/// </summary>
public interface ISalesReturnRepository
{
    Task<SalesReturn> GetByIdAsync(int id);
    Task<IEnumerable<SalesReturn>> GetAllAsync();
    Task AddAsync(SalesReturn entity);
    Task UpdateAsync(SalesReturn entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<SalesReturn>> GetBySaleInvoiceIdAsync(int saleInvoiceId);
}
