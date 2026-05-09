using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Barcode;

public class GetProductForTransactionByBarcodeQuery
{
    public string Barcode { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
}
