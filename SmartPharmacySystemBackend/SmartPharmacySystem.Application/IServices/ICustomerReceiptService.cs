using SmartPharmacySystem.Application.DTOs.Customers;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ICustomerReceiptService
    {
        Task<CustomerReceiptDto> CreateAsync(CreateCustomerReceiptDto dto, int userId);
        Task CancelAsync(int id, int userId);
        Task<IEnumerable<CustomerReceiptDto>> GetRecentReceiptsAsync(int customerId);
        Task<Application.Wrappers.PagedResponse<CustomerReceiptDto>> GetPagedAsync(string? search, int page, int pageSize, DateTime? fromDate, DateTime? toDate);
        Task<ReceiptStatisticsDto> GetStatisticsAsync();
    }
}
