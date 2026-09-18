namespace SmartPharmacySystem.Application.DTOs.PurchaseOrders;

/// <summary>
/// كائن نقل البيانات لبند أمر الشراء
/// </summary>
public class PurchaseOrderDetailDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? MedicineCode { get; set; }
    public string? Barcode { get; set; }
    public int? SaleUnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
}
