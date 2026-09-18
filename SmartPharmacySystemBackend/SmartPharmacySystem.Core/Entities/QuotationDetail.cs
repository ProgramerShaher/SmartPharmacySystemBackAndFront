using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل بنداً وتفصيلاً من بنود عرض السعر (Quotation Line Item)
/// </summary>
public class QuotationDetail : BaseEntity
{
    /// <summary>
    /// معرف رأس عرض السعر
    /// </summary>
    public int QuotationId { get; set; }

    /// <summary>
    /// رأس عرض السعر
    /// </summary>
    [ForeignKey("QuotationId")]
    public virtual Quotation? Quotation { get; set; }

    /// <summary>
    /// معرف الصنف / الدواء
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// الصنف المعروض
    /// </summary>
    [ForeignKey("MedicineId")]
    public virtual Medicine? Medicine { get; set; }

    /// <summary>
    /// معرف وحدة البيع المحددة (شريط، علبة، متر، كرتون...)
    /// </summary>
    public int? SaleUnitId { get; set; }

    /// <summary>
    /// وحدة البيع
    /// </summary>
    [ForeignKey("SaleUnitId")]
    public virtual MedicineUnit? SaleUnit { get; set; }

    /// <summary>
    /// الكمية المعروضة بالوحدة المحددة (تدعم الكسور للأنشطة الوزنية والقياسية)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر الوحدة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// نسبة الخصم الممنوحة على هذا البند (0-100)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercentage { get; set; } = 0;

    /// <summary>
    /// قيمة الخصم المالي للبند
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة الخاصة بالبند (%)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// قيمة ضريبة القيمة المضافة للبند
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ الفرعي للبند قبل الضريبة (بعد الخصم)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// الإجمالي النهائي للبند شامل الضريبة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; } = 0;

    /// <summary>
    /// ملاحظات أو مواصفات خاصة بهذا الصنف
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
