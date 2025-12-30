using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    /// <summary>
    /// Logs instances where a medicine was sold below its purchase cost.
    /// </summary>
    public class PriceOverride
    {
        [Key]
        public int Id { get; set; }

        public int SaleInvoiceId { get; set; }
        public int MedicineId { get; set; }
        public int BatchId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualCost { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public SaleInvoice SaleInvoice { get; set; } = null!;
        public Medicine Medicine { get; set; } = null!;
        public MedicineBatch Batch { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
