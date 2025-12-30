using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MedicineBatch entity operations.
/// تنفيذ المستودع لعمليات كيان دفعة الدواء.
/// </summary>
public class MedicineBatchRepository : IMedicineBatchRepository
{
    private readonly ApplicationDbContext _db;

    public MedicineBatchRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task AddAsync(MedicineBatch batch)
    {
        await _db.MedicineBatches.AddAsync(batch);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(MedicineBatch batch)
    {
        _db.MedicineBatches.Update(batch);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(MedicineBatch batch)
    {
        batch.IsDeleted = true;
        // batch.DeletedAt = DateTime.UtcNow; // No DeletedAt in entity
        _db.MedicineBatches.Update(batch);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<MedicineBatch?> GetByIdAsync(int? id)
    {
        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Include(b => b.InventoryMovements)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatch?> GetByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.BatchBarcode == barcode && !b.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatch?> GetByBarcodeAndExpiryAsync(string barcode, DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var dateOnly = expiryDate.Date;
        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .FirstOrDefaultAsync(b => b.BatchBarcode == barcode
                                      && b.ExpiryDate.Date == dateOnly
                                      && !b.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<MedicineBatch?> GetByMedicineIdAndBatchNumberAsync(int medicineId, string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return null;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .FirstOrDefaultAsync(b => b.MedicineId == medicineId
                                      && b.CompanyBatchNumber == batchNumber
                                      && !b.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetBatchesByMedicineIdAsync(int medicineId, bool includeDeleted = false)
    {
        var query = _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => b.MedicineId == medicineId);

        if (!includeDeleted)
            query = query.Where(b => !b.IsDeleted);

        return await query
            .OrderBy(b => b.ExpiryDate) // FIFO by expiry date
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted
                        && b.Status == "Active"
                        && b.InventoryMovements.Sum(m => m.Quantity) > 0
                        && b.ExpiryDate.Date > now)
            .OrderBy(b => b.ExpiryDate) // FIFO
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetAvailableBatchesByMedicineIdAsync(int medicineId)
    {
        var now = DateTime.UtcNow.Date;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted
                        && b.MedicineId == medicineId
                        && b.Status == "Active"
                        && b.InventoryMovements.Sum(m => m.Quantity) > 0
                        && b.ExpiryDate.Date > now)
            .OrderBy(b => b.ExpiryDate) // FIFO - nearest expiry first
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetExpiringBatchesAsync(int daysThreshold = 60)
    {
        var now = DateTime.UtcNow.Date;
        var thresholdDate = now.AddDays(daysThreshold);

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted
                        && b.ExpiryDate.Date > now
                        && b.ExpiryDate.Date <= thresholdDate
                        && b.InventoryMovements.Sum(m => m.Quantity) > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetExpiredBatchesAsync()
    {
        var now = DateTime.UtcNow.Date;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted && b.ExpiryDate.Date <= now)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetBatchesByStatusAsync(string status)
    {
        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted && b.Status == status)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetAllAsync(string? searchFilter = null, bool includeDeleted = false)
    {
        var query = _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Include(b => b.InventoryMovements)
            .AsQueryable();

        if (!includeDeleted)
            query = query.Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            var filter = searchFilter.ToLower();
            query = query.Where(b =>
                b.CompanyBatchNumber.ToLower().Contains(filter) ||
                (b.BatchBarcode != null && b.BatchBarcode.ToLower().Contains(filter)) ||
                (b.Medicine != null && b.Medicine.Name.ToLower().Contains(filter)) ||
                (b.StorageLocation != null && b.StorageLocation.ToLower().Contains(filter)));
        }

        return await query
            .OrderByDescending(b => b.EntryDate) // Use EntryDate instead of CreatedAt
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> BatchNumberExistsAsync(int medicineId, string batchNumber, int? excludeBatchId = null)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return false;

        var query = _db.MedicineBatches
            .Where(b => b.MedicineId == medicineId
                        && b.CompanyBatchNumber == batchNumber
                        && !b.IsDeleted);

        if (excludeBatchId.HasValue)
            query = query.Where(b => b.Id != excludeBatchId.Value);

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeBatchId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var query = _db.MedicineBatches
            .Where(b => b.BatchBarcode == barcode && !b.IsDeleted);

        if (excludeBatchId.HasValue)
            query = query.Where(b => b.Id != excludeBatchId.Value);

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<MedicineBatch?> GetNearestExpiryBatchAsync(int medicineId)
    {
        var now = DateTime.UtcNow.Date;

        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => !b.IsDeleted
                        && b.MedicineId == medicineId
                        && b.Status == "Active"
                        && b.InventoryMovements.Sum(m => m.Quantity) > 0
                        && b.ExpiryDate.Date > now)
            .OrderBy(b => b.ExpiryDate) // Get the one expiring soonest
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<int> UpdateExpiredBatchesStatusAsync()
    {
        var now = DateTime.UtcNow.Date;
        var thresholdDate = now.AddDays(3);

        var batchesToUpdate = await _db.MedicineBatches
            .Where(b => !b.IsDeleted
                        && b.Status == "Active"
                        && b.ExpiryDate.Date < thresholdDate)
            .ToListAsync();

        foreach (var batch in batchesToUpdate)
        {
            if (batch.ExpiryDate.Date < now)
            {
                batch.Status = "Expired";
            }
            else if (batch.ExpiryDate.Date < thresholdDate)
            {
                batch.Status = "Quarantine";
            }
        }

        return batchesToUpdate.Count;
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalAvailableQuantityAsync(int medicineId)
    {
        var now = DateTime.UtcNow.Date;

        return await _db.MedicineBatches
            .Where(b => !b.IsDeleted
                        && b.MedicineId == medicineId
                        && b.Status == "Active"
                        && b.ExpiryDate.Date > now)
            .SumAsync(b => b.InventoryMovements.Sum(m => m.Quantity));
    }
    /// <inheritdoc/>
    public async Task<int> GetTotalQuantityAsync(int medicineId)
    {
        return await _db.MedicineBatches
            .Where(b => !b.IsDeleted && b.MedicineId == medicineId)
            .SumAsync(b => b.RemainingQuantity);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MedicineBatch>> GetAllWithMedicineAsync()
    {
        return await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Include(b => b.CreatedByUser)
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }
}
