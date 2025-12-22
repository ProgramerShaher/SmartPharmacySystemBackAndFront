using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices;

public class SaleInvoiceQueryDto : BaseQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? PaymentMethod { get; set; }
}
