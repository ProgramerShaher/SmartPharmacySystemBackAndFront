namespace SmartPharmacySystem.Application.DTOs.StockMovement;

/// <summary>
/// كائن نقل البيانات لتحديث حركة مخزون.
/// يحتوي على البيانات القابلة للتحديث لحركة المخزون.
/// </summary>
public class UpdateStockMovementDto
{
    /// <summary>
    /// معرف حركة المخزون
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف الدواء
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// معرف دفعة الدواء
    /// </summary>
    public int? BatchId { get; set; }

    /// <summary>
    /// نوع الحركة
    /// </summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// التاريخ
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// المرجع
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
