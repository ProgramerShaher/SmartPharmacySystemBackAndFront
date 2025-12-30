using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a user in the pharmacy management system.
/// Users can perform various operations like sales, purchases, and inventory management.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// الاسم الكامل للمستخدم
    /// Full name of the user.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// اسم المستخدم لتسجيل الدخول (فريد)
    /// Username for login authentication (unique).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور المشفرة
    /// Password hash for authentication.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// معرف الدور المرتبط بالمستخدم
    /// Role ID associated with the user
    /// </summary>
    [Required]
    public int RoleId { get; set; }

    /// <summary>
    /// حالة المستخدم (نشط، غير نشط، موقوف)
    /// User status (Active, Inactive, Suspended)
    /// </summary>
    [Required]
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// البريد الإلكتروني للمستخدم
    /// Email address of the user.
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// رقم الهاتف للتواصل
    /// Phone number for contact.
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// ملاحظات إضافية عن المستخدم
    /// Additional notes about the user.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// كود إعادة تعيين كلمة المرور
    /// Reset password code/token.
    /// </summary>
    [MaxLength(100)]
    public string? ResetPasswordCode { get; set; }

    /// <summary>
    /// تاريخ آخر تسجيل دخول
    /// Last login date and time
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// تاريخ إنشاء المستخدم
    /// Date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي أنشأ هذا السجل
    /// ID of the user who created this record.
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// علامة الحذف المنطقي
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    // Navigation Properties

    /// <summary>
    /// الدور المرتبط بالمستخدم
    /// Role associated with the user
    /// </summary>
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// المستخدم الذي أنشأ هذا السجل
    /// User who created this record
    /// </summary>
    public virtual User? Creator { get; set; }

    /// <summary>
    /// المستخدمين الذين تم إنشاؤهم بواسطة هذا المستخدم
    /// Users created by this user
    /// </summary>
    public virtual ICollection<User> CreatedUsers { get; set; } = new List<User>();

    /// <summary>
    /// دفعات الأدوية التي أنشأها هذا المستخدم
    /// Medicine batches created by this user
    /// </summary>
    public virtual ICollection<MedicineBatch> CreatedBatches { get; set; } = new List<MedicineBatch>();
}
