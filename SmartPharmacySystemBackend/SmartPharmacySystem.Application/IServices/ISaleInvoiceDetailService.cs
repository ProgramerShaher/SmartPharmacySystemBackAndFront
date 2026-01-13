using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISaleInvoiceDetailService
    {
        Task<SaleInvoiceDetailDto> CreateAsync(CreateSaleInvoiceDetailDto dto);
        Task UpdateAsync(int id, UpdateSaleInvoiceDetailDto dto);
        Task DeleteAsync(int id);
        Task<SaleInvoiceDetailDto> GetByIdAsync(int id);
        Task<IEnumerable<SaleInvoiceDetailDto>> GetAllAsync();
    }
}
