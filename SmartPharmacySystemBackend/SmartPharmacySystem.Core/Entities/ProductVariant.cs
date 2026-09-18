using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// تنويعات الأصناف (المقاسات، الألوان، الأطوال، الموديلات) لأنشطة الموضة والملابس والأحذية
/// Product Variant (e.g. Size, Color, SKU) for Fashion, Footwear, etc.
/// </summary>
public class ProductVariant : BaseEntity
{
    /// <summary>
    /// معرف الصنف الأساسي
    /// </summary>
    public int MedicineId { get; set; }
    public virtual Medicine Medicine { get; set; } = null!;

    /// <summary>
    /// المقاس أو الحجم (مثل: S, M, L, XL, 38, 40, 42)
    /// </summary>
    [MaxLength(50)]
    public string? Size { get; set; }

    /// <summary>
    /// اللون (مثل: أبيض، أسود، كحلي، أحمر)
    /// </summary>
    [MaxLength(50)]
    public string? Color { get; set; }

    /// <summary>
    /// كود اللون الهيكس (مثل: #000000 أو #FFFFFF)
    /// </summary>
    [MaxLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// رمز التخزين الفريد للمتغير (SKU)
    /// </summary>
    [MaxLength(100)]
    public string? Sku { get; set; }

    /// <summary>
    /// باركود خاص بالمتغير (إن وجد)
    /// </summary>
    [MaxLength(100)]
    public string? Barcode { get; set; }

    /// <summary>
    /// فارق أو زيادة السعر الإضافي لهذا المتغير عن السعر الأساسي للصنف
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal AdditionalPrice { get; set; } = 0m;

    /// <summary>
    /// رصيد المخزون المتوفر لهذا المتغير
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal StockQuantity { get; set; } = 0m;

    /// <summary>
    /// هل المتغير نشط ومتاح للبيع؟
    /// </summary>
    public bool IsActive { get; set; } = true;
}
