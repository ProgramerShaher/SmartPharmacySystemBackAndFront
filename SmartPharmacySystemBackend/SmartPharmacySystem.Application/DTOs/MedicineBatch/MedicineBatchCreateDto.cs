using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// Data Transfer Object for creating a new medicine batch.
/// كائن نقل البيانات لإنشاء دفعة دواء جديدة.
/// </summary>
public class MedicineBatchCreateDto : IValidatableObject
{
    /// <summary>
    /// Medicine ID to which this batch belongs.
    /// معرف الدواء الذي تنتمي إليه هذه الدفعة.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "Medicine ID is required | معرف الدواء مطلوب")]
    [Range(1, int.MaxValue, ErrorMessage = "Medicine ID must be a valid positive number | معرف الدواء يجب أن يكون رقماً موجباً صحيحاً")]
    public int MedicineId { get; set; }

    /// <summary>
    /// Company batch/lot number provided by the manufacturer. Must be unique per medicine.
    /// رقم دفعة/لوط الشركة المقدم من الشركة المصنعة. يجب أن يكون فريداً لكل دواء.
    /// </summary>
    /// <example>LOT-2024-001234</example>
    [Required(ErrorMessage = "Batch number is required | رقم الدفعة مطلوب")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Batch number must be between 1 and 100 characters | رقم الدفعة يجب أن يكون بين 1 و 100 حرف")]
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique barcode for this batch.
    /// الباركود الفريد لهذه الدفعة.
    /// </summary>
    /// <example>6224001234567</example>
    [Required(ErrorMessage = "Batch barcode is required | باركود الدفعة مطلوب")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Batch barcode must be between 1 and 100 characters | باركود الدفعة يجب أن يكون بين 1 و 100 حرف")]
    public string BatchBarcode { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of units in this batch. Must be greater than 0.
    /// كمية الوحدات في هذه الدفعة. يجب أن يكون أكبر من صفر.
    /// </summary>
    /// <example>100</example>
    [Required(ErrorMessage = "Quantity is required | الكمية مطلوبة")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; }

    /// <summary>
    /// Expiry date of the batch.
    /// تاريخ انتهاء صلاحية الدفعة.
    /// </summary>
    /// <example>2025-12-31</example>
    [Required(ErrorMessage = "Expiry date is required | تاريخ انتهاء الصلاحية مطلوب")]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Date when the batch was purchased/received.
    /// تاريخ شراء/استلام الدفعة.
    /// </summary>
    /// <example>2024-01-15</example>
    [Required(ErrorMessage = "Entry date is required | تاريخ الإدخال مطلوب")]
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Purchase price per unit. Must be positive.
    /// سعر الشراء للوحدة. يجب أن يكون موجباً.
    /// </summary>
    /// <example>10.50</example>
    [Required(ErrorMessage = "Unit purchase price is required | سعر الشراء للوحدة مطلوب")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Unit purchase price must be positive | سعر الشراء للوحدة يجب أن يكون موجباً")]
    public decimal UnitPurchasePrice { get; set; }

    /// <summary>
    /// Storage location within the pharmacy.
    /// موقع التخزين داخل الصيدلية.
    /// </summary>
    /// <example>Shelf A-3</example>
    [StringLength(200, ErrorMessage = "Storage location cannot exceed 200 characters | موقع التخزين لا يمكن أن يتجاوز 200 حرف")]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// ID of the user creating this batch. Set by the system.
    /// معرف المستخدم الذي يقوم بإنشاء هذه الدفعة. يتم تعيينه من قبل النظام.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Custom validation logic for complex validation rules.
    /// منطق التحقق المخصص لقواعد التحقق المعقدة.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate batch is not already expired
        if (ExpiryDate.Date < DateTime.UtcNow.Date)
        {
            results.Add(new ValidationResult(
                "Cannot create a batch that is already expired | لا يمكن إنشاء دفعة منتهية الصلاحية بالفعل",
                new[] { nameof(ExpiryDate) }));
        }

        return results;
    }
}
