using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class BarcodeService : IBarcodeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BarcodeService> _logger;

    public BarcodeService(IUnitOfWork unitOfWork, ILogger<BarcodeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarcodeResultDto?> GetProductByBarcodeAsync(GetProductForTransactionByBarcodeQuery query, int userId)
    {
        _logger.LogInformation("Processing barcode: {Barcode} for Transaction: {Type} by User: {UserId}", 
            query.Barcode, query.TransactionType, userId);

        // 1. Search for specific batch by its unique barcode
        var batch = await _unitOfWork.MedicineBatches.GetByBarcodeAsync(query.Barcode);

        if (batch != null)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(batch.MedicineId);
            return await BuildResultAsync(medicine, batch, query.TransactionType);
        }

        // 2. Search for medicine by its default barcode
        var medicines = await _unitOfWork.Medicines.GetAllAsync(); // TODO: Optimize with GetByDefaultBarcodeAsync
        var medicineByDefault = medicines.FirstOrDefault(m => m.DefaultBarcode == query.Barcode);

        if (medicineByDefault != null)
        {
            // If default barcode scanned, find the BEST batch based on transaction type
            Core.Entities.MedicineBatch? selectedBatch = null;

            if (query.TransactionType == TransactionType.Sale || query.TransactionType == TransactionType.Return)
            {
                // For Sale/Return: Get nearest expiry (FIFO/FEFO)
                selectedBatch = await _unitOfWork.MedicineBatches.GetNearestExpiryBatchAsync(medicineByDefault.Id);
            }
            else if (query.TransactionType == TransactionType.Purchase)
            {
                // For Purchase: Maybe we just need the medicine info, but we return the latest batch if exists
                var batches = await _unitOfWork.MedicineBatches.GetAvailableBatchesByMedicineIdAsync(medicineByDefault.Id);
                selectedBatch = batches.OrderByDescending(b => b.EntryDate).FirstOrDefault();
            }

            return await BuildResultAsync(medicineByDefault, selectedBatch, query.TransactionType);
        }

        _logger.LogWarning("Barcode not found: {Barcode}", query.Barcode);
        return null;
    }

    private async Task<BarcodeResultDto> BuildResultAsync(Core.Entities.Medicine medicine, Core.Entities.MedicineBatch? batch, TransactionType type)
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
            result.AvailableQuantity = batch.RemainingQuantity;
            result.ExpiryDate = batch.ExpiryDate;
            result.IsNearExpiry = batch.IsNearExpiry;
            result.StorageLocation = batch.StorageLocation;

            // Set appropriate price based on transaction type
            result.SalePrice = type == TransactionType.Purchase 
                ? batch.UnitPurchasePrice 
                : (medicine.DefaultSalePrice > 0 ? medicine.DefaultSalePrice : batch.RetailPrice);
        }
        else
        {
            // Default price from medicine if no batch found
            result.SalePrice = medicine.DefaultSalePrice;
            result.ExpiryDate = null;
        }

        // Add alternatives if it's a sale and out of stock or near expiry
        if (type == TransactionType.Sale && (batch == null || batch.RemainingQuantity <= 0 || batch.IsNearExpiry))
        {
            if (!string.IsNullOrWhiteSpace(medicine.ActiveIngredient))
            {
                result.Alternatives = await GetAlternativesAsync(medicine.ActiveIngredient, medicine.Id);
            }
        }

        return result;
    }

    private async Task<List<MedicineAlternativeDto>> GetAlternativesAsync(string activeIngredient, int excludeMedicineId)
    {
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
}
