using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Repository interface for MedicineBatch entity operations.
/// واجهة المستودع لعمليات كيان دفعة الدواء.
/// </summary>
public interface IMedicineBatchRepository
{
    /// <summary>
    /// Adds a new medicine batch to the database.
    /// يضيف دفعة دواء جديدة إلى قاعدة البيانات.
    /// </summary>
    Task AddAsync(MedicineBatch batch);

    /// <summary>
    /// Updates an existing medicine batch.
    /// يحدث دفعة دواء موجودة.
    /// </summary>
    Task UpdateAsync(MedicineBatch batch);

    /// <summary>
    /// Deletes a medicine batch (soft delete).
    /// يحذف دفعة دواء (حذف ناعم).
    /// </summary>
    Task DeleteAsync(MedicineBatch batch);

    /// <summary>
    /// Gets a medicine batch by its ID.
    /// يحصل على دفعة دواء بمعرفها.
    /// </summary>
    Task<MedicineBatch?> GetByIdAsync(int? id);

    /// <summary>
    /// Gets multiple medicine batches by their IDs (optimized for bulk operations).
    /// يحصل على عدة دفعات دواء بمعرفاتها (محسّن للعمليات الجماعية).
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetByIdsAsync(IEnumerable<int> ids);

    /// <summary>
    /// Gets a medicine batch by its barcode.
    /// يحصل على دفعة دواء بالباركود.
    /// </summary>
    Task<MedicineBatch?> GetByBarcodeAsync(string barcode);
    Task<MedicineBatch?> GetByBarcodeAndExpiryAsync(string barcode, DateTime expiryDate);

    /// <summary>
    /// Gets a medicine batch by medicine ID and batch number (unique combination).
    /// يحصل على دفعة دواء بمعرف الدواء ورقم الدفعة (تركيبة فريدة).
    /// </summary>
    Task<MedicineBatch?> GetByMedicineIdAndBatchNumberAsync(int medicineId, string batchNumber);

    /// <summary>
    /// Gets all batches for a specific medicine.
    /// يحصل على جميع الدفعات لدواء معين.
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetBatchesByMedicineIdAsync(int medicineId, bool includeDeleted = false);

    /// <summary>
    /// Gets all available batches (Available status, has quantity, not expired).
    /// يحصل على جميع الدفعات المتاحة (حالة متاحة، لديها كمية، غير منتهية).
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync();

    /// <summary>
    /// Gets available batches for a specific medicine ordered by expiry date (FIFO).
    /// يحصل على الدفعات المتاحة لدواء معين مرتبة حسب تاريخ انتهاء الصلاحية (الأول أولاً).
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetAvailableBatchesByMedicineIdAsync(int medicineId);

    /// <summary>
    /// Gets batches that are expiring soon (within specified days).
    /// يحصل على الدفعات التي ستنتهي قريباً (خلال أيام محددة).
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetExpiringBatchesAsync(int daysThreshold = 60);

    /// <summary>
    /// Gets batches that have already expired.
    /// يحصل على الدفعات التي انتهت صلاحيتها بالفعل.
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetExpiredBatchesAsync();

    /// <summary>
    /// Gets batches by status.
    /// يحصل على الدفعات حسب الحالة.
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetBatchesByStatusAsync(string status);

    /// <summary>
    /// Gets all batches with optional filtering.
    /// يحصل على جميع الدفعات مع خيار الفلترة.
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetAllAsync(string? searchFilter = null, bool includeDeleted = false);

    /// <summary>
    /// Checks if a batch number exists for a specific medicine.
    /// يتحقق من وجود رقم دفعة لدواء معين.
    /// </summary>
    Task<bool> BatchNumberExistsAsync(int medicineId, string batchNumber, int? excludeBatchId = null);

    /// <summary>
    /// Checks if a barcode exists.
    /// يتحقق من وجود باركود.
    /// </summary>
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeBatchId = null);

    /// <summary>
    /// Gets the batch with the nearest expiry date for a medicine (FIFO logic).
    /// يحصل على الدفعة ذات أقرب تاريخ انتهاء صلاحية لدواء معين (منطق الأول أولاً).
    /// </summary>
    Task<MedicineBatch?> GetNearestExpiryBatchAsync(int medicineId);

    /// <summary>
    /// Updates the status of all expired batches.
    /// يحدث حالة جميع الدفعات المنتهية الصلاحية.
    /// </summary>
    Task<int> UpdateExpiredBatchesStatusAsync();

    /// <summary>
    /// Gets total available quantity for a medicine across all batches.
    /// يحصل على إجمالي الكمية المتاحة لدواء عبر جميع الدفعات.
    /// </summary>
    Task<int> GetTotalAvailableQuantityAsync(int medicineId);
    /// <summary>
    /// Gets total physical quantity (RemainingQuantity) for a medicine across all batches (including expired/quarantined if > 0).
    /// </summary>
    Task<int> GetTotalQuantityAsync(int medicineId);

    /// <summary>
    /// Gets all batches including the Medicine entity (eager load).
    /// يحصل على جميع الدفعات بما في ذلك كيان الدواء (تحميل مبكر).
    /// </summary>
    Task<IEnumerable<MedicineBatch>> GetAllWithMedicineAsync();
}
