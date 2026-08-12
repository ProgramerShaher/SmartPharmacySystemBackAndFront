using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Inventory;

public class InventoryFlowReportDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;

    public int OpeningBalance { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalSales { get; set; }
    public int TotalTransfersIn { get; set; }
    public int TotalTransfersOut { get; set; }
    public int TotalDamages { get; set; }
    public int TotalAdjustments { get; set; }
    public int TotalSalesReturns { get; set; }
    public int TotalPurchaseReturns { get; set; }

    // (Opening + Purchases + TransfersIn + SalesReturns) - (Sales + TransfersOut + Damages + PurchaseReturns) +/- Adjustments
    public int ExpectedClosingBalance { get; set; }
    public int ActualSystemBalance { get; set; }
    
    public bool IsBalanced => ExpectedClosingBalance == ActualSystemBalance;

    public List<MovementSummaryDto> RecentMovements { get; set; } = new();
}

public class MovementSummaryDto
{
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
