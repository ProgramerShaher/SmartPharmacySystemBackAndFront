using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface ISaleInvoiceDetailRepository
{
    Task<SaleInvoiceDetail?> GetByIdAsync(int id);
    Task<IEnumerable<SaleInvoiceDetail>> GetAllAsync();
    Task AddAsync(SaleInvoiceDetail entity);
    Task UpdateAsync(SaleInvoiceDetail entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
