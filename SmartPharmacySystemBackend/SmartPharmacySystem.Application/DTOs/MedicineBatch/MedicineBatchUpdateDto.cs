using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// Data Transfer Object for updating an existing medicine batch.
/// كائن نقل البيانات لتحديث دفعة دواء موجودة.
/// </summary>
public class MedicineBatchUpdateDto : IValidatableObject
{
    /// <summary>
    /// Quantity of units in this batch. Must be greater than 0.
    /// كمية الوحدات في هذه الدفعة. يجب أن تكون أكبر من صفر.
    /// </summary>
    /// <example>100</example>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0 | الكمية يجب أن تكون أكبر من صفر")]
    public int? Quantity { get; set; }

    /// <summary>
    /// Remaining quantity available. Cannot exceed total quantity.
    /// الكمية المتبقية المتاحة. لا يمكن أن تتجاوز الكمية الإجمالية.
    /// </summary>
    /// <example>50</example>
    [Range(0, int.MaxValue, ErrorMessage = "Remaining quantity cannot be negative | الكمية المتبقية لا يمكن أن تكون سالبة")]
    public int? RemainingQuantity { get; set; }

    /// <summary>
    /// Expiry date of the batch.
    /// تاريخ انتهاء صلاحية الدفعة.
    /// </summary>
    /// <example>2025-12-31</example>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Purchase price per unit. Must be positive.
    /// سعر الشراء للوحدة. يجب أن يكون موجباً.
    /// </summary>
    /// <example>10.50</example>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Unit purchase price must be positive | سعر الشراء للوحدة يجب أن يكون موجباً")]
    public decimal? UnitPurchasePrice { get; set; }

    /// <summary>
    /// Storage location within the pharmacy.
    /// موقع التخزين داخل الصيدلية.
    /// </summary>
    /// <example>Shelf A-3</example>
    [StringLength(200, ErrorMessage = "Storage location cannot exceed 200 characters | موقع التخزين لا يمكن أن يتجاوز 200 حرف")]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Status of the batch. Cannot set Expired batch to Available.
    /// حالة الدفعة. لا يمكن تعيين دفعة منتهية الصلاحية كمتاحة.
    /// </summary>
    /// <example>Active</example>
    public string? Status { get; set; }

    /// <summary>
    /// Custom validation logic for complex validation rules.
    /// منطق التحقق المخصص لقواعد التحقق المعقدة.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate RemainingQuantity <= Quantity if both are provided
        if (RemainingQuantity.HasValue && Quantity.HasValue && RemainingQuantity > Quantity)
        {
            results.Add(new ValidationResult(
                "Remaining quantity cannot exceed total quantity | الكمية المتبقية لا يمكن أن تتجاوز الكمية الإجمالية",
                new[] { nameof(RemainingQuantity), nameof(Quantity) }));
        }

        return results;
    }
}
