using SmartPharmacySystem.Application.DTOs.MedicineBatch;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// Service interface for medicine batch business operations.
/// واجهة الخدمة لعمليات الأعمال الخاصة بدفعات الأدوية.
/// </summary>
public interface IMedicineBatchService
{
    // ===================== CRUD Operations =====================

    /// <summary>
    /// Creates a new medicine batch.
    /// ينشئ دفعة دواء جديدة.
    /// </summary>
    /// <param name="dto">Batch creation data</param>
    /// <returns>Created batch response</returns>
    Task<MedicineBatchResponseDto> CreateBatchAsync(MedicineBatchCreateDto dto);

    /// <summary>
    /// Updates an existing medicine batch.
    /// يحدث دفعة دواء موجودة.
    /// </summary>
    /// <param name="batchId">ID of the batch to update</param>
    /// <param name="dto">Updated batch data</param>
    /// <returns>Updated batch response</returns>
    Task<MedicineBatchResponseDto> UpdateBatchAsync(int batchId, MedicineBatchUpdateDto dto, int userId);

    /// <summary>
    /// Soft deletes a medicine batch.
    /// يحذف دفعة دواء (حذف ناعم).
    /// </summary>
    /// <param name="batchId">ID of the batch to delete</param>
    /// <param name="deletedByUserId">ID of the user performing deletion</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBatchAsync(int batchId, int deletedByUserId);

    // ===================== Query Operations =====================

    /// <summary>
    /// Gets a batch by its ID.
    /// يحصل على دفعة بمعرفها.
    /// </summary>
    Task<MedicineBatchResponseDto?> GetBatchByIdAsync(int batchId);

    /// <summary>
    /// Gets a batch by its barcode.
    /// يحصل على دفعة بالباركود.
    /// </summary>
    Task<MedicineBatchResponseDto?> GetBatchByBarcodeAsync(string barcode);

    /// <summary>
    /// Gets all batches for a specific medicine.
    /// يحصل على جميع الدفعات لدواء معين.
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByMedicineIdAsync(int medicineId);

    /// <summary>
    /// Gets all available batches (can be sold).
    /// يحصل على جميع الدفعات المتاحة (يمكن بيعها).
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesAsync();

    /// <summary>
    /// Gets available batches for a specific medicine (FIFO ordered).
    /// يحصل على الدفعات المتاحة لدواء معين (مرتبة حسب الأول أولاً).
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetAvailableBatchesByMedicineIdAsync(int medicineId);

    /// <summary>
    /// Gets all batches with optional filtering.
    /// يحصل على جميع الدفعات مع خيار الفلترة.
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetAllBatchesAsync(string? searchFilter = null);

    /// <summary>
    /// Gets batches that are expiring soon.
    /// يحصل على الدفعات التي ستنتهي قريباً.
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetExpiringBatchesAsync(int daysThreshold = 60);

    /// <summary>
    /// Gets batches that have already expired.
    /// يحصل على الدفعات التي انتهت صلاحيتها.
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetExpiredBatchesAsync();

    /// <summary>
    /// Gets batches by status.
    /// يحصل على الدفعات حسب الحالة.
    /// </summary>
    Task<IEnumerable<MedicineBatchResponseDto>> GetBatchesByStatusAsync(string status);

    // ===================== Business Operations =====================

    /// <summary>
    /// Sells from a specific batch using FIFO logic.
    /// يبيع من دفعة محددة باستخدام منطق الأول أولاً.
    /// </summary>
    /// <param name="medicineId">Medicine to sell</param>
    /// <param name="quantity">Quantity to sell</param>
    /// <param name="userId">User performing the sale</param>
    /// <returns>Sale result with batch details used</returns>
    Task<BatchSaleResultDto> SellFromBatchFIFOAsync(int medicineId, int quantity, int userId);

    /// <summary>
    /// Sells from a specific batch by ID.
    /// يبيع من دفعة محددة بالمعرف.
    /// </summary>
    /// <param name="batchId">Batch to sell from</param>
    /// <param name="quantity">Quantity to sell</param>
    /// <param name="userId">User performing the sale</param>
    /// <returns>Updated batch response</returns>
    Task<MedicineBatchResponseDto> SellFromBatchAsync(int batchId, int quantity, int userId);

    /// <summary>
    /// Returns quantity to a batch (e.g., for sales returns).
    /// يرجع الكمية إلى دفعة (مثلاً لمرتجعات المبيعات).
    /// </summary>
    Task<MedicineBatchResponseDto> ReturnToBatchAsync(int batchId, int quantity, int userId);

    /// <summary>
    /// Marks a batch as damaged.
    /// يضع علامة على دفعة كتالفة.
    /// </summary>
    Task<MedicineBatchResponseDto> MarkBatchAsDamagedAsync(int batchId, string? reason, int userId);

    /// <summary>
    /// Updates all expired batches to Expired status.
    /// يحدث جميع الدفعات المنتهية الصلاحية إلى حالة منتهية.
    /// </summary>
    /// <returns>Number of batches updated</returns>
    Task<int> UpdateExpiredBatchesAsync();

    /// <summary>
    /// Scraps a batch permanently due to expiry or damage.
    /// إعدام دفعة دواء نهائياً بسبب التلف أو انتهاء الصلاحية.
    /// </summary>
    Task ScrapBatchAsync(int batchId, int userId, string reason);

    /// <summary>
    /// Processes financial loss automatically for an expired batch.
    /// يعالج الخسارة المالية تلقائياً لدفعة منتهية الصلاحية.
    /// </summary>
    /// <param name="batchId">ID of the expired batch</param>
    Task ProcessFinancialLossAsync(int batchId);

    /// <summary>
    /// Gets total available quantity for a medicine.
    /// يحصل على إجمالي الكمية المتاحة لدواء.
    /// </summary>
    Task<int> GetTotalAvailableQuantityAsync(int medicineId);

    /// <summary>
    /// Validates if a batch can be sold from.
    /// يتحقق مما إذا كان يمكن البيع من دفعة.
    /// </summary>
    Task<BatchValidationResultDto> ValidateBatchForSaleAsync(int batchId, int quantity);
}

/// <summary>
/// Result of a FIFO batch sale operation.
/// نتيجة عملية بيع الدفعات بمنطق الأول أولاً.
/// </summary>
public class BatchSaleResultDto
{
    /// <summary>
    /// Whether the sale was successful.
    /// هل كانت عملية البيع ناجحة.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result.
    /// رسالة تصف النتيجة.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Total quantity sold.
    /// إجمالي الكمية المباعة.
    /// </summary>
    public int TotalQuantitySold { get; set; }

    /// <summary>
    /// Details of each batch used in the sale.
    /// تفاصيل كل دفعة مستخدمة في البيع.
    /// </summary>
    public List<BatchSaleDetailDto> BatchDetails { get; set; } = new();
}

/// <summary>
/// Details of a batch used in a sale.
/// تفاصيل دفعة مستخدمة في عملية بيع.
/// </summary>
public class BatchSaleDetailDto
{
    /// <summary>
    /// Batch ID.
    /// معرف الدفعة.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Company batch number.
    /// رقم دفعة الشركة.
    /// </summary>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Quantity sold from this batch.
    /// الكمية المباعة من هذه الدفعة.
    /// </summary>
    public int QuantitySold { get; set; }



    /// <summary>
    /// Expiry date of the batch.
    /// تاريخ انتهاء صلاحية الدفعة.
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Remaining quantity after sale.
    /// الكمية المتبقية بعد البيع.
    /// </summary>
    public int RemainingQuantity { get; set; }
}

/// <summary>
/// Result of batch validation for sale.
/// نتيجة التحقق من صلاحية الدفعة للبيع.
/// </summary>
public class BatchValidationResultDto
{
    /// <summary>
    /// Whether the batch is valid for sale.
    /// هل الدفعة صالحة للبيع.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors if any.
    /// أخطاء التحقق إن وجدت.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Available quantity in the batch.
    /// الكمية المتاحة في الدفعة.
    /// </summary>
    public int AvailableQuantity { get; set; }
}
