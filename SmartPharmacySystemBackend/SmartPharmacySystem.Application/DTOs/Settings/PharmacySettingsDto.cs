namespace SmartPharmacySystem.Application.DTOs.Settings;

public class PharmacySettingsDto
{
    public int Id { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string? HealthMinistryLicense { get; set; }
    public string? Website { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string? InvoiceWelcomeMessage { get; set; }
    public SmartPharmacySystem.Core.Enums.BusinessType BusinessType { get; set; } = SmartPharmacySystem.Core.Enums.BusinessType.Pharmacy;
    public int? BusinessProfileId { get; set; }
    public BusinessProfileDto? BusinessProfile { get; set; }
}

public class UpdatePharmacySettingsDto
{
    public string PharmacyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegister { get; set; }
    public string? HealthMinistryLicense { get; set; }
    public string? Website { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string? InvoiceWelcomeMessage { get; set; }
    public SmartPharmacySystem.Core.Enums.BusinessType? BusinessType { get; set; }
}
