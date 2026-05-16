using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن تقرير قائمة الدخل (الأرباح والخسائر)
/// </summary>
public class IncomeStatementDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalRevenue - TotalExpenses;

    public List<FinancialReportItemDto> RevenueDetails { get; set; } = new();
    public List<FinancialReportItemDto> ExpenseDetails { get; set; } = new();
}

/// <summary>
/// كائن تقرير الميزانية العمومية
/// </summary>
public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    
    public decimal NetCurrentYearProfit { get; set; } // الربح المرحل من قائمة الدخل

    public List<FinancialReportItemDto> AssetDetails { get; set; } = new();
    public List<FinancialReportItemDto> LiabilityDetails { get; set; } = new();
    public List<FinancialReportItemDto> EquityDetails { get; set; } = new();
}

/// <summary>
/// بند تفصيلي في التقارير المالية
/// </summary>
public class FinancialReportItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
