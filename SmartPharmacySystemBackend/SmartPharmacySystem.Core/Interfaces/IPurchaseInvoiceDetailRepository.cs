using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPurchaseInvoiceDetailRepository
{
    Task<PurchaseInvoiceDetail?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseInvoiceDetail>> GetAllAsync();
    Task AddAsync(PurchaseInvoiceDetail entity);
    Task UpdateAsync(PurchaseInvoiceDetail entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<PurchaseInvoiceDetail>> GetDetailsByInvoiceIdAsync(int invoiceId);
}
