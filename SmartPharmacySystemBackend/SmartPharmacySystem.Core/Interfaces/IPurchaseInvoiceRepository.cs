using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for purchase invoice repository operations.
/// This interface outlines the data access methods for managing purchase invoices.
/// </summary>
public interface IPurchaseInvoiceRepository
{
    Task<PurchaseInvoice> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseInvoice>> GetAllAsync();
    Task AddAsync(PurchaseInvoice entity);
    Task UpdateAsync(PurchaseInvoice entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<PurchaseInvoice?> GetByIdWithDetailsAsync(int id);
}