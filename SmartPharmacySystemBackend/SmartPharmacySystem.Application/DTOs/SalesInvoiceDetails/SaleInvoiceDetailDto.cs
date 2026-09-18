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
    public decimal Quantity { get; set; }
    public decimal QuantityInSaleUnit { get; set; }
    public string SaleUnitName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalLineAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Profit { get; set; }
    public decimal RemainingQtyToReturn { get; set; }
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
    public decimal BatchRemainingQuantity { get; set; }
    public decimal BatchSoldQuantity { get; set; }
    public DateTime? BatchExpiryDate { get; set; }
}
