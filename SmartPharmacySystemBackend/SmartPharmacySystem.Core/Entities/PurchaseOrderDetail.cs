using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل بنداً من بنود أمر الشراء (Purchase Order Line Item)
/// </summary>
public class PurchaseOrderDetail : BaseEntity
{
    /// <summary>
    /// معرف أمر الشراء
    /// </summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>
    /// رأس أمر الشراء
    /// </summary>
    [ForeignKey("PurchaseOrderId")]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>
    /// معرف الصنف المطلوب توريده
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// الصنف المطلوب
    /// </summary>
    [ForeignKey("MedicineId")]
    public virtual Medicine? Medicine { get; set; }

    /// <summary>
    /// معرف وحدة التوريد والشراء (كرتون، باكت، حبة...)
    /// </summary>
    public int? SaleUnitId { get; set; }

    /// <summary>
    /// وحدة الشراء
    /// </summary>
    [ForeignKey("SaleUnitId")]
    public virtual MedicineUnit? SaleUnit { get; set; }

    /// <summary>
    /// الكمية المطلوبة بالوحدة المحددة (تدعم الكسور للوزن والقياس)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// الكمية المستلمة فعلياً حتى الآن
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReceivedQuantity { get; set; } = 0;

    /// <summary>
    /// سعر الشراء المتفق عليه للوحدة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// نسبة الخصم الممنوحة من المورد على هذا البند (%)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPercentage { get; set; } = 0;

    /// <summary>
    /// قيمة الخصم المالي للبند
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة للبند (%)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// قيمة ضريبة القيمة المضافة للبند
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ الفرعي للبند قبل الضريبة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// الإجمالي النهائي للبند
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; } = 0;

    /// <summary>
    /// مواصفات خاصة أو ملاحظات للبند
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
