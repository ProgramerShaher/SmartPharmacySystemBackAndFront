using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

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
    public string BaseCurrency { get; set; } = "ريال يمني";

    [MaxLength(500)]
    public string? InvoiceWelcomeMessage { get; set; }

    // ── Licensing ──────────────────────────────────────────────────────────
    /// <summary>
    /// Stores the HMAC-SHA256 license key that has been validated and activated
    /// for this machine. Null means the system is not yet activated.
    /// </summary>
    [MaxLength(512)]
    public string? LicenseKey { get; set; }

    // ── Business Type & Profile ───────────────────────────────────────────
    /// <summary>
    /// نوع النشاط التجاري للمنشأة (صيدلية، سوبرماركت، ملابس، إلخ)
    /// </summary>
    public BusinessType BusinessType { get; set; } = BusinessType.Pharmacy;

    /// <summary>
    /// المعرف الفريد لملف إعدادات النشاط التجاري المعتمد
    /// </summary>
    public int? BusinessProfileId { get; set; }

    /// <summary>
    /// ملف إعدادات النشاط التجاري المعتمد
    /// </summary>
    public virtual BusinessProfile? BusinessProfile { get; set; }
}
