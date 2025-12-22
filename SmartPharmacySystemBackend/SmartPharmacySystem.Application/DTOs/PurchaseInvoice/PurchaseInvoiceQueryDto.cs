using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceQueryDto : BaseQueryDto
{
    public int? SupplierId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
