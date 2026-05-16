using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل الشيكات البنكية (الصادرة والواردة)
/// </summary>
public class Cheque : BaseEntity
{
    /// <summary>
    /// رقم الشيك
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ChequeNumber { get; set; } = string.Empty;

    /// <summary>
    /// اسم البنك الصادر منه الشيك
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ استحقاق الشيك (تاريخ الصرف)
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// مبلغ الشيك
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// نوع الشيك (صادر/وارد)
    /// </summary>
    [Required]
    public ChequeType Type { get; set; }

    /// <summary>
    /// حالة الشيك (معلق، محصل، مرتجع)
    /// </summary>
    [Required]
    public ChequeStatus Status { get; set; } = ChequeStatus.Pending;

    /// <summary>
    /// اسم الساحب / المستفيد
    /// </summary>
    [MaxLength(200)]
    public string? PersonName { get; set; }

    /// <summary>
    /// معرف الحساب البنكي المرتبط في شجرة الحسابات
    /// </summary>
    public int? BankAccountId { get; set; }

    /// <summary>
    /// معرف القيد المحاسبي الذي تم إنشاء الشيك من خلاله
    /// </summary>
    public int? JournalEntryId { get; set; }

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>
    /// الحساب البنكي المرتبط
    /// </summary>
    [ForeignKey(nameof(BankAccountId))]
    public virtual Account? BankAccount { get; set; }

    /// <summary>
    /// القيد المحاسبي المرتبط
    /// </summary>
    [ForeignKey(nameof(JournalEntryId))]
    public virtual JournalEntry? JournalEntry { get; set; }
}
