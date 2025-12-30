using SmartPharmacySystem.Application.DTOs.SalesInvoices;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISaleInvoiceService
    {
        Task<SaleInvoiceDto> CreateAsync(CreateSaleInvoiceDto dto, int userId);
        Task UpdateAsync(int id, UpdateSaleInvoiceDto dto);
        Task DeleteAsync(int id);
        Task<SaleInvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<SaleInvoiceDto>> GetAllAsync();
        Task ApproveAsync(int id, int userId);
        Task UnapproveSalesInvoiceAsync(int id);
        Task CancelAsync(int id, int userId);
        Task ReceiveCreditPaymentAsync(int invoiceId, int accountId = 1);
    }
}
