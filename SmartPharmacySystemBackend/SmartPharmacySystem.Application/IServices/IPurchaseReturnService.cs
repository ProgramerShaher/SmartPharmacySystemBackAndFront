using SmartPharmacySystem.Application.DTOs.PurchaseReturns;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto, int userId);
        Task UpdateAsync(int id, UpdatePurchaseReturnDto dto);
        Task DeleteAsync(int id);
        Task<PurchaseReturnDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseReturnDto>> GetAllAsync();
        Task ApproveAsync(int id, int userId);
        Task CancelAsync(int id, int userId);
    }
}
