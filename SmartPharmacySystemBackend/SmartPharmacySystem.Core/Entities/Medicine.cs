using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a medicine in the pharmacy management system.
/// Medicines are the core products managed in the system.
/// </summary>
public class Medicine : BaseEntity
{

    /// <summary>
    /// Internal code for the medicine used within the system.
    /// </summary>
    public string? InternalCode { get; set; }

    /// <summary>
    /// Name of the medicine.
    /// الاسم التجاري
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Scientific name.
    /// الاسم العلمي
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// Active ingredient(s).
    /// المادة الفعالة
    /// </summary>
    public string? ActiveIngredient { get; set; }

    /// <summary>
    /// Foreign key to the category this medicine belongs to.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Manufacturer of the medicine.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Default barcode for the medicine.
    /// </summary>
    public string? DefaultBarcode { get; set; }

    /// <summary>
    /// Moving Average Cost (MAC) per unit.
    /// متوسط تكلفة الشراء
    /// </summary>
    public decimal MovingAverageCost { get; set; }

    /// <summary>
    /// Default purchase price per unit.
    /// </summary>
    public decimal DefaultPurchasePrice { get; set; }

    /// <summary>
    /// Default sale price per unit.
    /// </summary>
    public decimal DefaultSalePrice { get; set; }

    /// <summary>
    /// Minimum quantity that triggers an alert for low stock.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal MinAlertQuantity { get; set; }

    /// <summary>
    /// Point at which a reorder notification should be generated.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReorderLevel { get; set; } = 10m;

    /// <summary>
    /// Indicates if the medicine is sold by unit (true) or by quantity (false).
    /// </summary>
    public bool SoldByUnit { get; set; }

    /// <summary>
    /// Status of the medicine (Active/Inactive).
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Additional notes about the medicine.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Image URL for the medicine (used in mobile app catalog).
    /// </summary>
    public string? ImageUrl { get; set; }


    /// <summary>
    /// Name of the base unit (e.g. Pill, Ampoule, Bottle).
    /// اسم الوحدة الصغرى الأساسية (مثل حبة، أمبول، علبة/قارورة)
    /// </summary>
    public string BaseUnitName { get; set; } = "حبة";

    /// <summary>
    /// Navigation property to the category.
    /// </summary>
    public Category Category { get; set; }

    /// <summary>
    /// Collection of medicine batches for this medicine.
    /// </summary>
    public ICollection<MedicineBatch> MedicineBatches { get; set; }

    /// <summary>
    /// Collection of inventory movements for this medicine.
    /// </summary>
    public ICollection<InventoryMovement> InventoryMovements { get; set; }

    /// <summary>
    /// Collection of packaging/sub units for this medicine.
    /// </summary>
    public ICollection<MedicineUnit> MedicineUnits { get; set; } = new List<MedicineUnit>();

    /// <summary>
    /// تنويعات الصنف من مقاسات وألوان وموديلات
    /// </summary>
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    /// <summary>
    /// مدة الضمان الافتراضية بالأشهر للأجهزة الإلكترونية
    /// </summary>
    public int DefaultWarrantyMonths { get; set; } = 12;

    /// <summary>
    /// الأرقام التسلسلية للأجهزة المرتبطة بهذا الصنف
    /// </summary>
    public virtual ICollection<ProductSerialNumber> SerialNumbers { get; set; } = new List<ProductSerialNumber>();

    /// <summary>
    /// رقم القطعة الأصلي للشركة المصنعة (OEM Part Number) - خاص بقطع الغيار
    /// </summary>
    public string? OemPartNumber { get; set; }

    /// <summary>
    /// رقم القطعة التجاري أو البديل من الشركة الصانعة (Manufacturer Part Number)
    /// </summary>
    public string? ManufacturerPartNumber { get; set; }

    /// <summary>
    /// الموديلات والمركبات المتوافقة (Compatible Vehicles / Models)
    /// مثل: تويوتا كامري 2018-2024، لكزس ES 2019-2023
    /// </summary>
    public string? CompatibleVehicles { get; set; }
}