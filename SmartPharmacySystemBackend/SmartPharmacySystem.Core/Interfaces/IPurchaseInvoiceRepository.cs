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

    /// <summary>
    /// Gets a purchase invoice by ID with full details including Supplier, PurchaseInvoiceDetails, Medicine, and Batch.
    /// This method is optimized for purchase return operations.
    /// </summary>
    /// <param name="id">The purchase invoice ID</param>
    /// <returns>The purchase invoice with all related entities, or null if not found</returns>
    Task<PurchaseInvoice?> GetByIdWithFullDetailsAsync(int id);
}