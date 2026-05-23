namespace SmartPharmacySystem.Application.DTOs.StockCounts;

/// <summary>
/// كائن نقل البيانات لعرض بند في أمر الجرد.
/// </summary>
public class StockCountItemDto
{
    public int Id { get; set; }
    public int StockCountHeaderId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineBarcode { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int SystemQuantity { get; set; }
    public int? PhysicalQuantity { get; set; }
    public int Variance { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal VarianceValue { get; set; }
    public string? VarianceReason { get; set; }
}
