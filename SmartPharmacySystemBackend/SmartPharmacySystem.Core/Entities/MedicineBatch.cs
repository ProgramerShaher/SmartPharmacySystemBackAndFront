using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a batch (Lot) of medicine in the pharmacy system.
/// Each batch tracks specific purchase details, expiry information, and inventory status.
/// يمثل دفعة (لوط) من الدواء في نظام الصيدلية.
/// كل دفعة تتتبع تفاصيل الشراء، معلومات انتهاء الصلاحية، وحالة المخزون.
/// </summary>
public class MedicineBatch
{
    /// <summary>
    /// Primary key - Unique identifier for the medicine batch.
    /// المفتاح الأساسي - المعرف الفريد لدفعة الدواء.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the medicine this batch belongs to.
    /// المفتاح الخارجي للدواء الذي تنتمي إليه هذه الدفعة.
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// Company batch number provided by the manufacturer (Lot Number).
    /// رقم دفعة الشركة المقدم من الشركة المصنعة (رقم اللوط).
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Expiry date of the batch.
    /// تاريخ انتهاء صلاحية الدفعة.
    /// </summary>
    [Required]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Total quantity purchased in this batch.
    /// إجمالي الكمية المشتراة في هذه الدفعة.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// Remaining quantity available for sale.
    /// الكمية المتبقية المتاحة للبيع.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// Unit purchase price for this batch.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPurchasePrice { get; set; }

    /// <summary>
    /// Suggested retail/sale price for this batch.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// Linked purchase invoice ID.
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }

    /// <summary>
    /// Unique barcode for this specific batch.
    /// </summary>
    [StringLength(100)]
    public string? BatchBarcode { get; set; }

    /// <summary>
    /// Status of the batch (Active, Expired, Damaged, etc.).
    /// حالة الدفعة (نشط، منتهي، تالف، إلخ).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Storage location within the pharmacy.
    /// موقع التخزين داخل الصيدلية.
    /// </summary>
    [StringLength(100)]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Date when the batch was entered/received.
    /// تاريخ إدخال/استلام الدفعة.
    /// </summary>
    [Required]
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// ID of the user who created this batch.
    /// معرف المستخدم الذي أنشأ هذه الدفعة.
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// علامة الحذف الناعم.
    /// </summary>
    public bool IsDeleted { get; set; }

    // ===================== Navigation Properties =====================

    /// <summary>
    /// Navigation property to the medicine.
    /// خاصية التنقل للدواء.
    /// </summary>
    public Medicine Medicine { get; set; } = null!;

    /// <summary>
    /// Navigation property to the user who created this batch.
    /// خاصية التنقل للمستخدم الذي أنشأ هذه الدفعة.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Collection of purchase invoice details.
    /// مجموعة تفاصيل فواتير الشراء.
    /// </summary>
    public ICollection<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; } = new List<PurchaseInvoiceDetail>();

    /// <summary>
    /// Collection of sale invoice details.
    /// مجموعة تفاصيل فواتير البيع.
    /// </summary>
    public ICollection<SaleInvoiceDetail> SaleInvoiceDetails { get; set; } = new List<SaleInvoiceDetail>();

    /// <summary>
    /// Collection of inventory movements.
    /// مجموعة حركات المخزون.
    /// </summary>
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    /// <summary>
    /// Collection of alerts.
    /// مجموعة التنبيهات.
    /// </summary>
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    /// <summary>
    /// Collection of sales return details.
    /// مجموعة تفاصيل مرتجعات المبيعات.
    /// </summary>
    public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; } = new List<SalesReturnDetail>();

    /// <summary>
    /// Collection of purchase return details.
    /// مجموعة تفاصيل مرتجعات المشتريات.
    /// </summary>
    public ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();

    // ===================== Computed Properties =====================

    /// <summary>
    /// Checks if the batch has expired.
    /// يتحقق إذا كانت الدفعة منتهية الصلاحية.
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiryDate.Date < DateTime.UtcNow.Date;

    /// <summary>
    /// Checks if the batch is expiring soon (within 30 days).
    /// يتحقق إذا كانت الدفعة ستنتهي قريباً (خلال 30 يوماً).
    /// </summary>
    [NotMapped]
    public bool IsExpiringSoon => !IsExpired && (ExpiryDate.Date - DateTime.UtcNow.Date).Days <= 30;

    /// <summary>
    /// Checks if the batch is near expiry (within 3 days).
    /// CRITICAL: Sale is blocked if this is true.
    /// </summary>
    [NotMapped]
    public bool IsNearExpiry => !IsExpired && (ExpiryDate.Date - DateTime.UtcNow.Date).Days < 3;

    /// <summary>
    /// Calculates the days until expiry. Negative value means already expired.
    /// يحسب عدد الأيام حتى انتهاء الصلاحية.
    /// </summary>
    [NotMapped]
    public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.UtcNow.Date).Days;

    /// <summary>
    /// Checks if the batch can be sold.
    /// يتحقق إذا كان يمكن بيع الدفعة.
    /// </summary>
    [NotMapped]
    public bool IsSellable => RemainingQuantity > 0 && !IsExpired && !IsNearExpiry && Status == "Active";

    /// <summary>
    /// Total quantity sold from this batch.
    /// إجمالي الكمية المباعة من هذه الدفعة.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int SoldQuantity { get; set; }

    // ===================== Business Logic Methods =====================

    /// <summary>
    /// Marks the batch as expired.
    /// يضع علامة على الدفعة كمنتهية الصلاحية.
    /// </summary>
    public void MarkAsExpired()
    {
        Status = "Expired";
    }

    /// <summary>
    /// Marks the batch as quarantine (near expiry).
    /// </summary>
    public void MarkAsQuarantine()
    {
        Status = "Quarantine";
    }

    /// <summary>
    /// Marks the batch as damaged.
    /// يضع علامة على الدفعة كتالفة.
    /// </summary>
    public void MarkAsDamaged()
    {
        Status = "Damaged";
    }

    /// <summary>
    /// Performs soft delete on the batch.
    /// ينفذ الحذف الناعم على الدفعة.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
    }
}