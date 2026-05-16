using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن نقل البيانات للشيكات
/// </summary>
public class ChequeDto
{
    public int Id { get; set; }
    public string ChequeNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public ChequeType Type { get; set; }
    public ChequeStatus Status { get; set; }
    public string? PersonName { get; set; }
    public int? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public int? JournalEntryId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// كائن إنشاء شيك جديد
/// </summary>
public class CreateChequeDto
{
    public string ChequeNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public ChequeType Type { get; set; }
    public string? PersonName { get; set; }
    public int? BankAccountId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// كائن تحديث حالة الشيك
/// </summary>
public class UpdateChequeStatusDto
{
    public int Id { get; set; }
    public ChequeStatus Status { get; set; }
}
