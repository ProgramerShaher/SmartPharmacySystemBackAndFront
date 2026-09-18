using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities
{
    /// <summary>
    /// Represents a payment record for a sale invoice (supporting multi-payment split).
    /// سجل دفع لفاتورة المبيعات (يدعم الدفع المتعدد المقسم: كاش + بطاقة + آجل).
    /// </summary>
    public class SaleInvoicePayment : BaseEntity
    {
        /// <summary>
        /// ID of the parent sale invoice.
        /// </summary>
        public int SaleInvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent sale invoice.
        /// </summary>
        [ForeignKey("SaleInvoiceId")]
        public virtual SaleInvoice? SaleInvoice { get; set; }

        /// <summary>
        /// Payment method (Cash, Card, BankTransfer, Check, etc.).
        /// طريقة الدفع
        /// </summary>
        [Required]
        public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

        /// <summary>
        /// Amount paid via this method.
        /// المبلغ المدفوع بهذه الطريقة
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Reference or transaction number (e.g. Card Authorization Code, Bank Transfer Reference).
        /// رقم المرجع أو العملية (مثل رقم عملية الشبكة أو رقم الحوالة)
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Optional linked accounting account.
        /// </summary>
        public int? AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        /// <summary>
        /// Optional notes for this payment line.
        /// </summary>
        [MaxLength(250)]
        public string? Notes { get; set; }
    }
}
