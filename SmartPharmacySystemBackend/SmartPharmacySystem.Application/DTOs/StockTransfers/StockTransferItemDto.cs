using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockTransfers;

/// <summary>
/// كائن نقل البيانات لعرض بند في سند تحويل مخزني.
/// </summary>
public class StockTransferItemDto
{
    public int Id { get; set; }
    public int StockTransferId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineBarcode { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public decimal QuantityRequested { get; set; }
    public decimal QuantityDispatched { get; set; }
    public decimal? QuantityReceived { get; set; }
    public decimal Variance { get; set; }
}
