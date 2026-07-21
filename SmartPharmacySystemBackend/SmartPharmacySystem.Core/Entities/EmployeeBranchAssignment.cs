using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل سجل تعيين الموظف/المستخدم في فرع معين
/// Represents the assignment of an employee/user to a specific branch.
/// </summary>
public class EmployeeBranchAssignment : BaseEntity
{
    /// <summary>
    /// معرف المستخدم المرتبط بالتعيين
    /// User ID associated with the assignment.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// معرف الفرع المرتبط بالتعيين
    /// Branch ID associated with the assignment.
    /// </summary>
    [Required]
    public int BranchId { get; set; }

    /// <summary>
    /// تاريخ تعيين المستخدم للفرع
    /// Date when the user was assigned to the branch.
    /// </summary>
    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// يوضح ما إذا كان هذا التعيين نشطاً حالياً
    /// Indicates whether this assignment is currently active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// المستخدم المرتبط
    /// Associated User.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// الفرع المرتبط
    /// Associated Branch.
    /// </summary>
    public virtual Branch Branch { get; set; } = null!;
}
