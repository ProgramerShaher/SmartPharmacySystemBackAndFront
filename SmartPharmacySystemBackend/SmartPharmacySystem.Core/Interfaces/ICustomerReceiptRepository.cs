using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface ICustomerReceiptRepository
    {
        Task<CustomerReceipt?> GetByIdAsync(int id);
        Task AddAsync(CustomerReceipt entity);
        Task UpdateAsync(CustomerReceipt entity);
        Task<IEnumerable<CustomerReceipt>> GetByCustomerIdAsync(int customerId);
        Task<CustomerReceipt?> GetByIdWithCustomerAsync(int id);
    }
}
