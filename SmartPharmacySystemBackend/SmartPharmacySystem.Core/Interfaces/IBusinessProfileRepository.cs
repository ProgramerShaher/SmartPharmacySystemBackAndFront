using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع ملفات إعدادات الأنشطة التجارية
/// </summary>
public interface IBusinessProfileRepository
{
    /// <summary>
    /// استرجاع الملف النشط والمعتمد حالياً للنظام
    /// </summary>
    Task<BusinessProfile?> GetActiveProfileAsync();

    /// <summary>
    /// استرجاع ملف نشاط معين بحسب نوع النشاط التجاري
    /// </summary>
    Task<BusinessProfile?> GetByTypeAsync(BusinessType businessType);

    /// <summary>
    /// استرجاع جميع ملفات الأنشطة المخزنة
    /// </summary>
    Task<IEnumerable<BusinessProfile>> GetAllProfilesAsync();

    /// <summary>
    /// إضافة ملف نشاط جديد
    /// </summary>
    Task AddAsync(BusinessProfile profile);

    /// <summary>
    /// تحديث ملف نشاط موجود
    /// </summary>
    Task UpdateAsync(BusinessProfile profile);
}
