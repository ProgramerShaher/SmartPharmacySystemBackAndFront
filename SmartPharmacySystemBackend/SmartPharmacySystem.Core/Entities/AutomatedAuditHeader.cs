using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    public class AutomatedAuditHeader : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string AuditCode { get; set; } = string.Empty;

        // If null, it means it's a comprehensive audit for ALL warehouses in the current branch.
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public StockCountType CountType { get; set; }

        public DateTime AuditDate { get; set; }

        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Totals for the entire audit
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOpeningValue { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPurchasesValue { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSalesValue { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDamagesValue { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalShortageValue { get; set; }

        public ICollection<AutomatedAuditItem> Items { get; set; } = new List<AutomatedAuditItem>();
    }
}
