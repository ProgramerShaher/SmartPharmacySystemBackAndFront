using SmartPharmacySystem.Application.DTOs.Barcode;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IBarcodeService
{
    /// <summary>
    /// Executes the barcode query to find a product for a specific transaction type.
    /// </summary>
    Task<BarcodeResultDto?> GetProductByBarcodeAsync(GetProductForTransactionByBarcodeQuery query, int userId);
}
