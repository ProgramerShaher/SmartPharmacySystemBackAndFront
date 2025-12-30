using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IPurchaseInvoiceService
    {
        Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto, int userId);
        Task UpdateAsync(int id, UpdatePurchaseInvoiceDto dto);
        Task DeleteAsync(int id);
        Task<PurchaseInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
        Task ApproveAsync(int id, int userId);
        Task UnapproveAsync(int id);
        Task CancelAsync(int id, int userId);
        Task PayCreditInvoiceAsync(int invoiceId, int accountId = 1);
    }
}
