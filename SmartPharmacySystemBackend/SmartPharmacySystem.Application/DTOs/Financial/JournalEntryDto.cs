using SmartPharmacySystem.Core.Enums;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن نقل البيانات للقيد المحاسبي
/// </summary>
public class JournalEntryDto
{
    public int Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public VoucherType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsPosted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// كائن تفاصيل سطر القيد المحاسبي
/// </summary>
public class JournalEntryLineDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// كائن إنشاء قيد جديد
/// </summary>
public class CreateJournalEntryDto
{
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public VoucherType Type { get; set; } = VoucherType.JournalEntry;
    public string Description { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public ReferenceType? ReferenceType { get; set; }
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// كائن إنشاء سطر قيد جديد
/// </summary>
public class CreateJournalEntryLineDto
{
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
}
