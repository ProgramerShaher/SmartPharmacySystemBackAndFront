using SmartPharmacySystem.Application.DTOs.Customers;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ICustomerReceiptService
    {
        Task<CustomerReceiptDto> CreateAsync(CreateCustomerReceiptDto dto, int userId);
        Task CancelAsync(int id, int userId);
        Task<IEnumerable<CustomerReceiptDto>> GetRecentReceiptsAsync(int customerId);
    }
}
