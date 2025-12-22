using SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IPurchaseReturnDetailService
    {
        Task<PurchaseReturnDetailDto> CreateAsync(CreatePurchaseReturnDetailDto dto);
        Task UpdateAsync(int id, UpdatePurchaseReturnDetailDto dto);
        Task DeleteAsync(int id);
        Task<PurchaseReturnDetailDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseReturnDetailDto>> GetAllAsync();
    }
}
