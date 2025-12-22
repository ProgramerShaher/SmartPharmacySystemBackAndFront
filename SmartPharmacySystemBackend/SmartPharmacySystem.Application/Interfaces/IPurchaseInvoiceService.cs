using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IPurchaseInvoiceService
    {
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto);
        Task UpdateAsync(int id, UpdatePurchaseInvoiceDto dto);
        Task DeleteAsync(int id);
        Task<PurchaseInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
        Task ApproveAsync(int id);
        Task CancelAsync(int id);
    }
}
