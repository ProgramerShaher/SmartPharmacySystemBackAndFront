using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities
{
    public class CustomerReceipt : BaseEntity
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

        [MaxLength(100)]
        public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

        [MaxLength(500)]
        public string? Notes { get; set; }


        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }
        public int? CancelledBy { get; set; }

    }
}
