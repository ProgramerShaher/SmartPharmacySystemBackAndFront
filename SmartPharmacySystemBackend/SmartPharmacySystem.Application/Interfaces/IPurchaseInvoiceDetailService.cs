using SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IPurchaseInvoiceDetailService
    {
        Task<PurchaseInvoiceDetailDto> CreateAsync(CreatePurchaseInvoiceDetailDto dto);
        Task UpdateAsync(int id, UpdatePurchaseInvoiceDetailDto dto);
        Task DeleteAsync(int id);
        Task<PurchaseInvoiceDetailDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseInvoiceDetailDto>> GetAllAsync();
    }
}
