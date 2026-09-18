using System.Threading.Tasks;
using SmartPharmacySystem.Application.DTOs.Quotations;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة إدارة عروض الأسعار والتحويل لفواتير مبيعات
/// </summary>
public interface IQuotationService
{
    Task<QuotationDto?> GetByIdAsync(int id);
    Task<QuotationPrintDto?> GetPrintDataAsync(int id);
    Task<PagedResult<QuotationDto>> GetPagedAsync(QuotationFilterDto filter);
    Task<QuotationDto> CreateAsync(CreateQuotationDto dto, int userId);
    Task<QuotationDto> UpdateAsync(int id, UpdateQuotationDto dto, int userId);
    Task<bool> DeleteAsync(int id);
    Task<QuotationDto> ChangeStatusAsync(int id, QuotationStatus newStatus, int userId);
    Task<SaleInvoiceDto> ConvertToSaleInvoiceAsync(int id, ConvertQuotationToInvoiceDto dto, int userId);
}
