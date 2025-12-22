namespace SmartPharmacySystem.Application.DTOs.Suppliers;

/// <summary>
/// كائن نقل البيانات لتحديث بيانات مورد.
/// يحتوي على البيانات القابلة للتحديث للمورد.
/// </summary>
public class UpdateSupplierDto
{
    /// <summary>
    /// معرف المورد
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// اسم المورد
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// العنوان
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
