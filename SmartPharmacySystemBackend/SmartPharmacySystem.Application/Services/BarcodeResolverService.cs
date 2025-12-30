using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class BarcodeResolverService : IBarcodeResolverService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BarcodeResolverService> _logger;

    public BarcodeResolverService(IUnitOfWork unitOfWork, ILogger<BarcodeResolverService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarcodeResultDto?> ResolveBarcodeAsync(string barcode)
    {
        _logger.LogInformation("Resolving barcode: {Barcode}", barcode);

        // 1. Try to find a specific batch by its unique barcode
        var batch = await _unitOfWork.MedicineBatches.GetByBarcodeAsync(barcode);

        if (batch != null)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(batch.MedicineId);
            return await BuildResultAsync(medicine, batch);
        }

        // 2. Try to find a medicine by its default barcode
        var medicines = await _unitOfWork.Medicines.GetAllAsync(); // This is inefficient for large DBs, should add GetByDefaultBarcodeAsync
        var medicineByDefault = medicines.FirstOrDefault(m => m.DefaultBarcode == barcode);

        if (medicineByDefault != null)
        {
            // If default barcode scanned, find the BEST batch for sale (FIFO)
            var fifoBatch = await _unitOfWork.MedicineBatches.GetNearestExpiryBatchAsync(medicineByDefault.Id);
            return await BuildResultAsync(medicineByDefault, fifoBatch);
        }

        _logger.LogWarning("Barcode not found: {Barcode}", barcode);
        return null;
    }

    public async Task<List<MedicineAlternativeDto>> GetAlternativesAsync(string activeIngredient, int excludeMedicineId)
    {
        if (string.IsNullOrWhiteSpace(activeIngredient)) return new();

        var alternatives = await _unitOfWork.Medicines.GetAlternativesAsync(activeIngredient, excludeMedicineId);

        var result = new List<MedicineAlternativeDto>();
        foreach (var alt in alternatives)
        {
            var batches = await _unitOfWork.MedicineBatches.GetAvailableBatchesByMedicineIdAsync(alt.Id);
            var totalQty = batches.Sum(b => b.RemainingQuantity);

            result.Add(new MedicineAlternativeDto
            {
                MedicineId = alt.Id,
                Name = alt.Name,
                SalePrice = alt.DefaultSalePrice,
                AvailableQuantity = totalQty
            });
        }

        return result;
    }

    private async Task<BarcodeResultDto> BuildResultAsync(Core.Entities.Medicine medicine, Core.Entities.MedicineBatch? batch)
    {
        var result = new BarcodeResultDto
        {
            MedicineId = medicine.Id,
            TradeName = medicine.Name,
            ScientificName = medicine.ScientificName,
            ActiveIngredient = medicine.ActiveIngredient,
            MovingAverageCost = medicine.MovingAverageCost
        };

        if (batch != null)
        {
            result.BatchId = batch.Id;
            result.BatchNumber = batch.CompanyBatchNumber;
            result.SalePrice = medicine.DefaultSalePrice; // Use default or specific batch price logic
            result.AvailableQuantity = batch.RemainingQuantity;
            result.ExpiryDate = batch.ExpiryDate;
            result.StorageLocation = batch.StorageLocation;
            result.IsNearExpiry = batch.IsNearExpiry;
        }

        // If out of stock or near expiry, fetch alternatives
        if (batch == null || batch.RemainingQuantity <= 0 || batch.IsNearExpiry)
        {
            if (!string.IsNullOrWhiteSpace(medicine.ActiveIngredient))
            {
                result.Alternatives = await GetAlternativesAsync(medicine.ActiveIngredient, medicine.Id);
            }
        }

        return result;
    }
}
