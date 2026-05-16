using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل تفاصيل القيد المحاسبي (كل سطر يمثل حركة على حساب معين)
/// </summary>
public class JournalEntryLine : BaseEntity
{
    /// <summary>
    /// معرف رأس القيد التابع له
    /// </summary>
    [Required]
    public int JournalEntryId { get; set; }

    /// <summary>
    /// معرف الحساب المتأثر في شجرة الحسابات
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// المبلغ المدين (Debit)
    /// </summary>
    public decimal Debit { get; set; }

    /// <summary>
    /// المبلغ الدائن (Credit)
    /// </summary>
    public decimal Credit { get; set; }

    /// <summary>
    /// وصف خاص بهذا السطر من القيد
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation Properties

    /// <summary>
    /// رأس القيد
    /// </summary>
    [ForeignKey(nameof(JournalEntryId))]
    public virtual JournalEntry JournalEntry { get; set; } = null!;

    /// <summary>
    /// الحساب المتأثر
    /// </summary>
    [ForeignKey(nameof(AccountId))]
    public virtual Account Account { get; set; } = null!;
}
