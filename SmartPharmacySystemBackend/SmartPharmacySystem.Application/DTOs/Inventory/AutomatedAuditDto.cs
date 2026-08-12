using SmartPharmacySystem.Core.Enums;
using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Inventory
{
    public class AutomatedAuditHeaderDto
    {
        public int Id { get; set; }
        public string AuditCode { get; set; } = string.Empty;
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int BranchId { get; set; }
        public StockCountType CountType { get; set; }
        public string CountTypeName { get; set; } = string.Empty;
        public DateTime AuditDate { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal TotalOpeningValue { get; set; }
        public decimal TotalPurchasesValue { get; set; }
        public decimal TotalSalesValue { get; set; }
        public decimal TotalDamagesValue { get; set; }
        public decimal TotalShortageValue { get; set; }

        public IEnumerable<AutomatedAuditItemDto> Items { get; set; } = new List<AutomatedAuditItemDto>();
    }

    public class AutomatedAuditItemDto
    {
        public int Id { get; set; }
        public int AutomatedAuditHeaderId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int? BatchId { get; set; }
        public string? BatchNumber { get; set; }
        public string? Barcode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
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
        
        public int Variance { get; set; }

        public decimal UnitCost { get; set; }
        public decimal VarianceValue { get; set; }
    }

    public class GenerateAuditRequestDto
    {
        public int? WarehouseId { get; set; }
        public StockCountType CountType { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class AuditChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> SalesValues { get; set; } = new List<decimal>();
        public List<decimal> PurchasesValues { get; set; } = new List<decimal>();
        public List<decimal> DamagesValues { get; set; } = new List<decimal>();
        public List<decimal> ShortagesValues { get; set; } = new List<decimal>();
    }
}
