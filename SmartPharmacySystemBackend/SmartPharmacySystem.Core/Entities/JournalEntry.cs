using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل رأس القيد المحاسبي (سند قبض، صرف، أو قيد يدوي)
/// </summary>
public class JournalEntry : BaseEntity
{
    /// <summary>
    /// رقم السند الفريد (توليد آلي)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string VoucherNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ القيد المحاسبي
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// نوع السند (صرف، قبض، قيد يومية)
    /// </summary>
    [Required]
    public VoucherType Type { get; set; }

    /// <summary>
    /// شرح عام للعملية المالية
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// إجمالي المبالغ المدينة (يجب أن يساوي الدائن)
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// إجمالي المبالغ الدائنة
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// هل القيد مرحل للأستاذ العام
    /// </summary>
    public bool IsPosted { get; set; }

    /// <summary>
    /// تاريخ الترحيل
    /// </summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// معرف المرجع (مثل معرف الفاتورة إذا كان القيد آلياً)
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// نوع المرجع (فاتورة مبيعات، مشتريات، إلخ)
    /// </summary>
    public ReferenceType? ReferenceType { get; set; }

    // Navigation Properties

    /// <summary>
    /// تفاصيل القيد (المدين والدائن لكل حساب)
    /// </summary>
    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
