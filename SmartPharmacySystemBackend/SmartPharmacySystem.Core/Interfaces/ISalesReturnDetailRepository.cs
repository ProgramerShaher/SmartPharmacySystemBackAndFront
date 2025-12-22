using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface ISalesReturnDetailRepository
{
    Task<SalesReturnDetail?> GetByIdAsync(int id);
    Task<IEnumerable<SalesReturnDetail>> GetAllAsync();
    Task AddAsync(SalesReturnDetail entity);
    Task UpdateAsync(SalesReturnDetail entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
