using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities
{
    public class AutomatedAuditItem : BaseEntity
    {
        public int AutomatedAuditHeaderId { get; set; }
        public AutomatedAuditHeader? Header { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        public int? BatchId { get; set; }
        public MedicineBatch? Batch { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPurchases { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalSales { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalTransfersIn { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalTransfersOut { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDamages { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAdjustments { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalSalesReturns { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPurchaseReturns { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExpectedSystemBalance { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal ActualSystemBalance { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Variance { get; set; } // ActualSystemBalance - ExpectedSystemBalance

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } // Store cost at time of audit

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceValue { get; set; } // Variance * UnitCost
    }
}
