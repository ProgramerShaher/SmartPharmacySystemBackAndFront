namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a purchase invoice.
/// Each detail line corresponds to a specific medicine batch purchased.
/// </summary>
public class PurchaseInvoiceDetail : BaseEntity
{

    /// <summary>
    /// Foreign key to the purchase invoice.
    /// </summary>
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Quantity purchased (stored in Base Unit = smallest unit, e.g. pills).
    /// الكمية المشتراة محوّلة إلى أصغر وحدة (حبة). محسوبة تلقائياً.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Quantity as entered by the user in the selected purchase unit (e.g. 5 cartons).
    /// الكمية كما أدخلها المستخدم بالوحدة المحددة (مثلاً: 5 كراتين).
    /// </summary>
    public int QuantityInPurchaseUnit { get; set; }

    /// <summary>
    /// The unit used when purchasing (FK to MedicineUnit). Null = base unit.
    /// الوحدة المستخدمة في الشراء (مثلاً: كرتون). إذا كانت null تعني الوحدة الأساسية.
    /// </summary>
    public int? PurchaseUnitId { get; set; }

    /// <summary>
    /// Free quantity received (Bonus).
    /// </summary>
    public int BonusQuantity { get; set; }

    /// <summary>
    /// Purchase price per unit.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Sale price per unit recorded at purchase.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Total amount for this line (Quantity * PurchasePrice).
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Calculated true cost per unit after bonus.
    /// </summary>
    public decimal TrueUnitCost { get; set; }

    /// <summary>
    /// Storage location for this item in the warehouse (e.g. Shelf 1, Rack A)
    /// موقع التخزين الخاص بهذا الصنف في المخزن (مثل رف 1، ممر أ).
    /// </summary>
    public string? StorageLocation { get; set; }


    /// <summary>
    /// Navigation property to the purchase invoice.
    /// </summary>
    public PurchaseInvoice PurchaseInvoice { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }

    /// <summary>
    /// Navigation property to the purchase unit (MedicineUnit).
    /// خاصية التنقل لوحدة الشراء المستخدمة.
    /// </summary>
    public MedicineUnit? PurchaseUnit { get; set; }
}