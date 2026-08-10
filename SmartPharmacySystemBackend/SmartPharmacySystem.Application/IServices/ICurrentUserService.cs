namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة المستخدم الحالي
/// Current user service interface
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// معرف المستخدم الحالي
    /// Current user ID
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// اسم المستخدم الحالي
    /// Current username
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// دور المستخدم الحالي
    /// Current user role
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// معرف دور المستخدم الحالي
    /// Current user role ID
    /// </summary>
    int? RoleId { get; }

    /// <summary>
    /// هل المستخدم مصادق عليه
    /// Is user authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// هل المستخدم مدير
    /// Is user admin
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// هل المستخدم صيدلي
    /// Is user pharmacist
    /// </summary>
    bool IsPharmacist { get; }

    /// <summary>
    /// معرف الفرع الحالي
    /// Current branch ID
    /// </summary>
    int? GetCurrentBranchId();

    /// <summary>
    /// معرف الموظف المرتبط بهذا الحساب (ERP)
    /// Employee ID linked to this user account
    /// </summary>
    int? EmployeeId { get; }

    /// <summary>
    /// كود الموظف
    /// Employee code
    /// </summary>
    string? EmployeeCode { get; }

    /// <summary>
    /// اسم الموظف الكامل
    /// Employee full name
    /// </summary>
    string? EmployeeName { get; }
}
