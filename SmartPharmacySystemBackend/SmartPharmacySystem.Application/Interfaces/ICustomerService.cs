using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> GetByIdAsync(int id);
        Task<PagedResponse<CustomerDto>> GetAllPagedAsync(string? search, int page, int pageSize);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task UpdateAsync(UpdateCustomerDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<CustomerDto>> GetTopDebtorsAsync(int count);
        Task<CustomerStatementDto> GetStatementAsync(int customerId);
    }
}
