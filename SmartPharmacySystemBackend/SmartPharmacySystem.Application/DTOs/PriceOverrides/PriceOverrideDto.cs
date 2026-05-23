namespace SmartPharmacySystem.Application.DTOs.PriceOverrides;

public class PriceOverrideDto
{
    public int Id { get; set; }
    public int SaleInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal SoldPrice { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Difference { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
