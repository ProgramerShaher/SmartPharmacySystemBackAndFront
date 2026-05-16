using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// كائن نقل البيانات للحساب المحاسبي
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public bool IsMainAccount { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// كائن إنشاء حساب جديد
/// </summary>
public class CreateAccountDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public bool IsMainAccount { get; set; }
}

/// <summary>
/// كائن تحديث حساب موجود
/// </summary>
public class UpdateAccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
