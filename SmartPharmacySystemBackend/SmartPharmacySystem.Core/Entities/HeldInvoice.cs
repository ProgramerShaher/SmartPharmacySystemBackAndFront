using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    /// <summary>
    /// Represents a suspended/held sales invoice in POS (Hold Invoice feature).
    /// فاتورة معلقة مؤقتاً في نقطة البيع لاستئنافها لاحقاً.
    /// </summary>
    public class HeldInvoice : BaseMultiBranchEntity
    {
        /// <summary>
        /// Unique reference / display title for the held ticket (e.g., "معلقة #1 - 14:35")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string HoldReference { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        [MaxLength(200)]
        public string? CustomerName { get; set; }

        public int? UserShiftId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; }

        public int ItemsCount { get; set; }

        /// <summary>
        /// JSON snapshot of the cart line items, units, quantities, prices, discounts, and customer.
        /// لقطة بيانات الأصناف والكميات والأسعار بصيغة JSON
        /// </summary>
        [Required]
        public string CartJson { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
