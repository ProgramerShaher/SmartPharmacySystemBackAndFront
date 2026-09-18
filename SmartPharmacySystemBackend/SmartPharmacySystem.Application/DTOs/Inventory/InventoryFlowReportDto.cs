using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Inventory;

public class InventoryFlowReportDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalTransfersIn { get; set; }
    public decimal TotalTransfersOut { get; set; }
    public decimal TotalDamages { get; set; }
    public decimal TotalAdjustments { get; set; }
    public decimal TotalSalesReturns { get; set; }
    public decimal TotalPurchaseReturns { get; set; }

    // (Opening + Purchases + TransfersIn + SalesReturns) - (Sales + TransfersOut + Damages + PurchaseReturns) +/- Adjustments
    public decimal ExpectedClosingBalance { get; set; }
    public decimal ActualSystemBalance { get; set; }
    
    public bool IsBalanced => ExpectedClosingBalance == ActualSystemBalance;

    public List<MovementSummaryDto> RecentMovements { get; set; } = new();
}

public class MovementSummaryDto
{
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
