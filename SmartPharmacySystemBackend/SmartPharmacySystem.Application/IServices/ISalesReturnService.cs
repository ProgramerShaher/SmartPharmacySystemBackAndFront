using SmartPharmacySystem.Application.DTOs.SalesReturns;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISalesReturnService
    {
        Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto, int userId);
        Task UpdateAsync(int id, UpdateSalesReturnDto dto);
        Task DeleteAsync(int id);
        Task<SalesReturnDto> GetByIdAsync(int id);
        Task<IEnumerable<SalesReturnDto>> GetAllAsync();
        Task ApproveAsync(int id, int userId);
        Task CancelAsync(int id, int userId);
    }
}
