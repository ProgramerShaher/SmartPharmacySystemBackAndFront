using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    public class SupplierPayment : BaseEntity
    {

        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? ReferenceNo { get; set; } // Paper Receipt No

        public int? PurchaseInvoiceId { get; set; }

        [ForeignKey("PurchaseInvoiceId")]
        public virtual PurchaseInvoice? PurchaseInvoice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

    }
}
