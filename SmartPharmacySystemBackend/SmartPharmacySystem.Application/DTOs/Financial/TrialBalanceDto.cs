using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن تقرير ميزان المراجعة
/// </summary>
public class TrialBalanceDto
{
    public DateTime AsOfDate { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsBalanced => TotalDebit == TotalCredit;
    public decimal Difference => Math.Abs(TotalDebit - TotalCredit);

    public List<TrialBalanceItemDto> Items { get; set; } = new();
}

/// <summary>
/// بند في ميزان المراجعة لحساب معين
/// </summary>
public class TrialBalanceItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance => Debit - Credit;
}
