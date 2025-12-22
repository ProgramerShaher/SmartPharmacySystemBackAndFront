using SmartPharmacySystem.Application.DTOs.SalesReturnDetails;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISalesReturnDetailService
    {
        Task<SalesReturnDetailDto> CreateAsync(CreateSalesReturnDetailDto dto);
        Task UpdateAsync(int id, UpdateSalesReturnDetailDto dto);
        Task DeleteAsync(int id);
        Task<SalesReturnDetailDto> GetByIdAsync(int id);
        Task<IEnumerable<SalesReturnDetailDto>> GetAllAsync();
    }
}
