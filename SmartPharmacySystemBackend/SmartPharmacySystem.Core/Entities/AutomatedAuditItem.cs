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

        public int OpeningBalance { get; set; }
        public int TotalPurchases { get; set; }
        public int TotalSales { get; set; }
        public int TotalTransfersIn { get; set; }
        public int TotalTransfersOut { get; set; }
        public int TotalDamages { get; set; }
        public int TotalAdjustments { get; set; }
        public int TotalSalesReturns { get; set; }
        public int TotalPurchaseReturns { get; set; }

        public int ExpectedSystemBalance { get; set; }
        public int ActualSystemBalance { get; set; }
        
        public int Variance { get; set; } // ActualSystemBalance - ExpectedSystemBalance

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } // Store cost at time of audit

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceValue { get; set; } // Variance * UnitCost
    }
}
