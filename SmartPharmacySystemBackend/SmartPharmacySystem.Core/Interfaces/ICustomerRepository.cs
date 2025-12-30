using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task AddAsync(Customer entity);
        Task UpdateAsync(Customer entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
        Task<IEnumerable<Customer>> GetTopDebtorsAsync(int count);
        Task UpdateBalanceAsync(int customerId, decimal amount);
    }
}
