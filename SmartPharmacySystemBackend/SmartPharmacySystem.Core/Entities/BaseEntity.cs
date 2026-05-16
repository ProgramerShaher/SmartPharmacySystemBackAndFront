using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// الكلاس الأساسي لكل الكيانات في النظام
/// يحتوي على الحقول المشتركة لتوحيد الهيكلية وتسهيل التتبع والرقابة
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// المعرف الفريد للسجل
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// تاريخ إنشاء السجل
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاريخ آخر تحديث للسجل
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي أنشأ السجل
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بآخر تحديث
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// علم الحذف المنطقي (Soft Delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// تاريخ الحذف (في حال تم الحذف منطقياً)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بالحذف
    /// </summary>
    public int? DeletedBy { get; set; }

    /// <summary>
    /// تنفيذ الحذف المنطقي للسجل
    /// </summary>
    public virtual void SoftDelete(int? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
