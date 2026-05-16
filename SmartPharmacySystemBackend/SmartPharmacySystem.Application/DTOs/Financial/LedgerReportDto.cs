using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن تقرير كشف الحساب (دفتر الأستاذ)
/// </summary>
public class LedgerReportDto
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<LedgerEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// تفاصيل حركة في دفتر الأستاذ
/// </summary>
public class LedgerEntryDto
{
    public DateTime Date { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public int JournalEntryId { get; set; }
}
