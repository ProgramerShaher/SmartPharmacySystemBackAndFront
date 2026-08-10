using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل دور المستخدم في النظام
/// Represents a user role in the system
/// </summary>
public class Role : BaseEntity
{

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
    
    [MaxLength(50)]
    public string? NameAr { get; set; }
    
    [MaxLength(7)]
    public string? Color { get; set; }
    
    public bool IsSystemRole { get; set; } = false;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// مجموعة المستخدمين المرتبطين بهذا الدور
    /// Collection of users with this role
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
