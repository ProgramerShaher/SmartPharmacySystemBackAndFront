using SmartPharmacySystem.Application.DTOs.SalesReturns;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISalesReturnService
    {
        Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto);
        Task UpdateAsync(int id, UpdateSalesReturnDto dto);
        Task DeleteAsync(int id);
        Task<SalesReturnDto> GetByIdAsync(int id);
        Task<IEnumerable<SalesReturnDto>> GetAllAsync();
        Task ApproveAsync(int id);
        Task CancelAsync(int id);
    }
}
