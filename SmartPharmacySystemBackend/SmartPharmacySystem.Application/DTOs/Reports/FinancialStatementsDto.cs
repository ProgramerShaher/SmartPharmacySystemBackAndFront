namespace SmartPharmacySystem.Application.DTOs.Reports;

public class TrialBalanceLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class TrialBalanceDto
{
    public List<TrialBalanceLineDto> Lines { get; set; } = new();
    public decimal TotalOpeningDebit => Lines.Sum(l => l.OpeningDebit);
    public decimal TotalOpeningCredit => Lines.Sum(l => l.OpeningCredit);
    public decimal TotalPeriodDebit => Lines.Sum(l => l.PeriodDebit);
    public decimal TotalPeriodCredit => Lines.Sum(l => l.PeriodCredit);
    public decimal TotalClosingDebit => Lines.Sum(l => l.ClosingDebit);
    public decimal TotalClosingCredit => Lines.Sum(l => l.ClosingCredit);
}

public class IncomeStatementLineDto
{
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class IncomeStatementDto
{
    public List<IncomeStatementLineDto> Revenues { get; set; } = new();
    public List<IncomeStatementLineDto> Expenses { get; set; } = new();
    public decimal TotalRevenue => Revenues.Sum(r => r.Amount);
    public decimal TotalExpense => Expenses.Sum(e => e.Amount);
    public decimal NetProfit => TotalRevenue - TotalExpense;
}

public class BalanceSheetLineDto
{
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BalanceSheetDto
{
    public List<BalanceSheetLineDto> Assets { get; set; } = new();
    public List<BalanceSheetLineDto> Liabilities { get; set; } = new();
    public List<BalanceSheetLineDto> Equity { get; set; } = new();
    public decimal TotalAssets => Assets.Sum(a => a.Amount);
    public decimal TotalLiabilities => Liabilities.Sum(l => l.Amount);
    public decimal TotalEquity => Equity.Sum(e => e.Amount);
}
