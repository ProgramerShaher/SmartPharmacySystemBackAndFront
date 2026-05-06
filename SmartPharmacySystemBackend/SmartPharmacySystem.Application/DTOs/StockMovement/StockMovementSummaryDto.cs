namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockMovementSummaryDto
{
    public decimal TotalStockValue { get; set; }
    public int NearExpiryCount { get; set; }
    public int LowStockCount { get; set; }
    public int TodayMovements { get; set; }
    public List<StockMovementTrendDto> Last30DaysTrend { get; set; } = new();
    public List<StockCategoryDistributionDto> CategoryDistribution { get; set; } = new();
}

public class StockMovementTrendDto
{
    public DateTime Date { get; set; }
    public int Additions { get; set; }
    public int Deductions { get; set; }
}

public class StockCategoryDistributionDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Value { get; set; }
}
