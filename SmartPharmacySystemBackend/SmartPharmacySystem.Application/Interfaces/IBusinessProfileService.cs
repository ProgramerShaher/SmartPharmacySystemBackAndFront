using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة إدارة ملفات الأنماط التجارية
/// </summary>
public interface IBusinessProfileService
{
    /// <summary>
    /// استرجاع ملف النشاط التجاري الفعال حالياً
    /// </summary>
    Task<BusinessProfileDto> GetActiveProfileAsync();

    /// <summary>
    /// تبديل نوع النشاط التجاري للنظام وتطبيق ملف إعداداته الافتراضي
    /// </summary>
    Task<BusinessProfileDto> SwitchBusinessTypeAsync(BusinessType newType);

    /// <summary>
    /// تعديل وتخصيص إعدادات الملف النشط حالياً
    /// </summary>
    Task<BusinessProfileDto> UpdateCustomProfileAsync(UpdateBusinessProfileDto dto);

    /// <summary>
    /// استرجاع الإعدادات الافتراضية لكل الأنشطة الـ 10 المدعومة للمعاينة والمقارنة
    /// </summary>
    IEnumerable<BusinessProfileDto> GetAllPresetProfiles();

    /// <summary>
    /// استرجاع الإعدادات الافتراضية لنشاط معين
    /// </summary>
    BusinessProfileDto GetPresetForType(BusinessType type);
}
