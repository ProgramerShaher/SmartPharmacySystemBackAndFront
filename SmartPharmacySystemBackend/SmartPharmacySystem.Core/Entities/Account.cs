using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل حساب في شجرة الحسابات المحاسبية
/// يدعم الهيكلية الشجرية (أب - ابن)
/// </summary>
public class Account : BaseEntity
{
    /// <summary>
    /// كود الحساب (مثلاً: 111، 112) - يجب أن يكون فريداً
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// اسم الحساب (مثل: الصندوق الرئيسي)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نوع الحساب (أصل، خصم، مصروف، إيراد)
    /// </summary>
    [Required]
    public AccountType Type { get; set; }

    /// <summary>
    /// وصف إضافي للحساب
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// المعرف الفريد للحساب الأب (في حال كان حساباً فرعياً)
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// هل الحساب رئيسي (تجميعي) أم فرعي (تقيد عليه الحركات)
    /// </summary>
    public bool IsMainAccount { get; set; }

    /// <summary>
    /// الرصيد الحالي للحساب (يحدث تلقائياً مع كل قيد)
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// هل الحساب نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// الحساب الأب
    /// </summary>
    public virtual Account? Parent { get; set; }

    /// <summary>
    /// الحسابات الفرعية التابعة لهذا الحساب
    /// </summary>
    public virtual ICollection<Account> Children { get; set; } = new List<Account>();

    /// <summary>
    /// تفاصيل القيود اليومية المرتبطة بهذا الحساب
    /// </summary>
    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}
