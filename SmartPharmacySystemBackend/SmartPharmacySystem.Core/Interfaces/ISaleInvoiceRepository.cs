using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for sale invoice repository operations.
/// This interface outlines the data access methods for managing sale invoices.
/// </summary>
public interface ISaleInvoiceRepository
{
    Task<SaleInvoice> GetByIdAsync(int id);
    Task<IEnumerable<SaleInvoice>> GetAllAsync();
    Task AddAsync(SaleInvoice entity);
    Task UpdateAsync(SaleInvoice entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}