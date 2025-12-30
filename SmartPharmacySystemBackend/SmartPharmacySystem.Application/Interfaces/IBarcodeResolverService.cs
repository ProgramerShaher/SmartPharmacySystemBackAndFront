using SmartPharmacySystem.Application.DTOs.Barcode;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IBarcodeResolverService
{
    /// <summary>
    /// Resolves a barcode to a specific medicine and its best available batch (FIFO).
    /// Includes scientific info and alternatives if out of stock.
    /// </summary>
    Task<BarcodeResultDto?> ResolveBarcodeAsync(string barcode);

    /// <summary>
    /// Gets available alternatives for a medicine based on its active ingredient.
    /// </summary>
    Task<List<MedicineAlternativeDto>> GetAlternativesAsync(string activeIngredient, int excludeMedicineId);
}
