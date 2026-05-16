using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل إعدادات الصيدلية وتفاصيلها الأساسية مثل الاسم والشعار ومعلومات التواصل والبيانات الضريبية
/// </summary>
public class PharmacySettings : BaseEntity
{
    [Required(ErrorMessage = "اسم الصيدلية مطلوب")]
    [MaxLength(200)]
    public string PharmacyName { get; set; } = "صيدلية جديدة";

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    public string? MobileNumber { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? TaxNumber { get; set; }

    [MaxLength(100)]
    public string? CommercialRegister { get; set; }

    [MaxLength(100)]
    public string? HealthMinistryLicense { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(20)]
    public string BaseCurrency { get; set; } = "ر.س";

    [MaxLength(500)]
    public string? InvoiceWelcomeMessage { get; set; }
}
