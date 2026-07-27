namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a sale invoice.
/// Each detail line corresponds to a specific medicine batch sold.
/// </summary>
public class SaleInvoiceDetail : BaseEntity
{

    /// <summary>
    /// Foreign key to the sale invoice.
    /// </summary>
    public int SaleInvoiceId { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Quantity sold (stored in Base Unit = smallest unit, e.g. pills). Auto-calculated.
    /// الكمية المباعة محوّلة إلى أصغر وحدة (حبة). محسوبة تلقائياً من وحدة البيع.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Quantity as entered by the cashier in the selected sale unit (e.g. 2 strips).
    /// الكمية كما أدخلها الكاشير بالوحدة المحددة (مثلاً: 2 شريط).
    /// </summary>
    public int QuantityInSaleUnit { get; set; }

    /// <summary>
    /// The unit used when selling (FK to MedicineUnit). Null = base unit.
    /// الوحدة المستخدمة في البيع (مثلاً: شريط). إذا كانت null تعني الوحدة الأساسية.
    /// </summary>
    public int? SaleUnitId { get; set; }

    /// <summary>
    /// Sale price per unit.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Unit cost per item.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total sale amount for this line (Quantity * SalePrice).
    /// </summary>
    public decimal TotalLineAmount { get; set; }

    /// <summary>
    /// Total cost for this line.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Profit for this line.
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// Quantity remaining that can be returned.
    /// </summary>
    public int RemainingQtyToReturn { get; set; }


    /// <summary>
    /// Navigation property to the sale invoice.
    /// </summary>
    public SaleInvoice SaleInvoice { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }

    /// <summary>
    /// Navigation property to the sale unit (MedicineUnit).
    /// خاصية التنقل لوحدة البيع المستخدمة.
    /// </summary>
    public MedicineUnit? SaleUnit { get; set; }
}