using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface ISaleInvoicePaymentRepository
    {
        Task<SaleInvoicePayment?> GetByIdAsync(int id);
        Task<IEnumerable<SaleInvoicePayment>> GetBySaleInvoiceIdAsync(int saleInvoiceId);
        Task AddAsync(SaleInvoicePayment entity);
        Task AddRangeAsync(IEnumerable<SaleInvoicePayment> entities);
        Task UpdateAsync(SaleInvoicePayment entity);
        Task DeleteAsync(int id);
    }
}
