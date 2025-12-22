namespace SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لتفصيل فاتورة البيع.
/// يحتوي على جميع بيانات تفصيل فاتورة البيع للعرض بما في ذلك بيانات الفاتورة والدفعة.
/// </summary>
public class SaleInvoiceDetailDto
{
    // --- Detail Data ---
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string CompanyBatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal SalePrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalLineAmount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Profit { get; set; }
    public bool IsDeleted { get; set; }

    // --- Invoice Data ---
    public int SaleInvoiceId { get; set; }
    public DateTime SaleInvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal InvoiceTotalAmount { get; set; }
    public decimal InvoiceTotalCost { get; set; }
    public decimal InvoiceTotalProfit { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // --- Batch Data ---
    public int BatchRemainingQuantity { get; set; }
    public int BatchSoldQuantity { get; set; }
    public DateTime? BatchExpiryDate { get; set; }
}
