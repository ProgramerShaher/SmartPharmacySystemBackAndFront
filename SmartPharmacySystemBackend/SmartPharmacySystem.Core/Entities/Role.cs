using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل دور المستخدم في النظام
/// Represents a user role in the system
/// </summary>
public class Role
{
    /// <summary>
    /// معرف الدور
    /// Role identifier
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// اسم الدور (Admin, Pharmacist)
    /// Role name (Admin, Pharmacist)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// وصف الدور
    /// Role description
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// تاريخ إنشاء الدور
    /// Role creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// مجموعة المستخدمين المرتبطين بهذا الدور
    /// Collection of users with this role
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
