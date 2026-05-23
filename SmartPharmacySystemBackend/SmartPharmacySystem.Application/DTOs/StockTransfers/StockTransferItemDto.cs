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
    public int QuantityRequested { get; set; }
    public int QuantityDispatched { get; set; }
    public int? QuantityReceived { get; set; }
    public int Variance { get; set; }
}
