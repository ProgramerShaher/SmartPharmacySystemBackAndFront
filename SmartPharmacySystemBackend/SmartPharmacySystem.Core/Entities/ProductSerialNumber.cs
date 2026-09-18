using SmartPharmacySystem.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    /// <summary>
    /// الأرقام التسلسلية للأجهزة والإلكترونيات وتتبع الضمان
    /// Product Serial Numbers & Warranty Tracking for Electronics & Appliances
    /// </summary>
    public class ProductSerialNumber : BaseEntity
    {
        /// <summary>
        /// معرف الصنف الأساسي (الجهاز)
        /// </summary>
        public int MedicineId { get; set; }
        public virtual Medicine Medicine { get; set; } = null!;

        /// <summary>
        /// معرف الدفعة المرتبطة (إن وجدت)
        /// </summary>
        public int? BatchId { get; set; }
        public virtual MedicineBatch? Batch { get; set; }

        /// <summary>
        /// الرقم التسلسلي الفريد للجهاز (مثل IMEI أو Serial Number)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// حالة السيريال
        /// </summary>
        public SerialNumberStatus Status { get; set; } = SerialNumberStatus.InStock;

        /// <summary>
        /// معرف سطر فاتورة الشراء الذي تم توريد الجهاز من خلاله
        /// </summary>
        public int? PurchaseInvoiceDetailId { get; set; }
        public virtual PurchaseInvoiceDetail? PurchaseInvoiceDetail { get; set; }

        /// <summary>
        /// معرف سطر فاتورة البيع الذي تم بيع الجهاز من خلاله
        /// </summary>
        public int? SaleInvoiceDetailId { get; set; }
        public virtual SaleInvoiceDetail? SaleInvoiceDetail { get; set; }

        /// <summary>
        /// معرف العميل المشتري
        /// </summary>
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// تاريخ بيع الجهاز للعميل
        /// </summary>
        public DateTime? SaleDate { get; set; }

        /// <summary>
        /// مدة الضمان بالأشهر (مثال: 12 أو 24 شهر)
        /// </summary>
        public int WarrantyMonths { get; set; } = 12;

        /// <summary>
        /// تاريخ انتهاء الضمان للجهاز (محسوب من تاريخ البيع + مدة الضمان)
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// ملاحظات إضافية عن حالة الجهاز أو الضمان
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// هل الجهاز حالياً تحت فترة الضمان السارية؟
        /// </summary>
        [NotMapped]
        public bool IsWarrantyValid =>
            Status == SerialNumberStatus.Sold &&
            WarrantyExpiryDate.HasValue &&
            WarrantyExpiryDate.Value >= DateTime.UtcNow;

        /// <summary>
        /// تسجيل بيع الجهاز وتفعيل الضمان
        /// </summary>
        public void MarkAsSold(int saleInvoiceDetailId, int? customerId, int warrantyMonths)
        {
            Status = SerialNumberStatus.Sold;
            SaleInvoiceDetailId = saleInvoiceDetailId;
            CustomerId = customerId;
            SaleDate = DateTime.UtcNow;
            WarrantyMonths = warrantyMonths > 0 ? warrantyMonths : 12;
            WarrantyExpiryDate = DateTime.UtcNow.AddMonths(WarrantyMonths);
        }

        /// <summary>
        /// إرجاع الجهاز إلى المخزن
        /// </summary>
        public void MarkAsReturned()
        {
            Status = SerialNumberStatus.Returned;
            SaleInvoiceDetailId = null;
        }
    }
}
