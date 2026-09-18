using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities
{
    public class CustomerReceipt : BaseMultiBranchEntity
    {

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? ReferenceNo { get; set; }

        /// <summary>
        /// Optional linked sale invoice for this receipt.
        /// معرف فاتورة المبيعات المرتبطة بالسند (اختياري)
        /// </summary>
        public int? SaleInvoiceId { get; set; }

        [ForeignKey("SaleInvoiceId")]
        public virtual SaleInvoice? SaleInvoice { get; set; }

        [MaxLength(100)]
        public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

        [MaxLength(500)]
        public string? Notes { get; set; }


        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }
        public int? CancelledBy { get; set; }

    }
}
