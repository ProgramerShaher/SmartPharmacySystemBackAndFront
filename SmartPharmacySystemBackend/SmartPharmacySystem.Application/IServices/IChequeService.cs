using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.IServices;

/// <summary>
/// واجهة خدمة إدارة الشيكات (البنكية)
/// </summary>
public interface IChequeService
{
    /// <summary>
    /// جلب بيانات شيك بواسطة المعرف
    /// </summary>
    Task<ChequeDto> GetByIdAsync(int id);

    /// <summary>
    /// البحث في الشيكات مع الفلترة
    /// </summary>
    Task<PagedResponse<ChequeDto>> GetPagedAsync(int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status, string? type);

    /// <summary>
    /// تسجيل شيك جديد في النظام
    /// </summary>
    Task<ChequeDto> CreateAsync(ChequeDto dto, int userId);

    /// <summary>
    /// تحديث حالة الشيك (مثل: تحصيل، رفض، ملغى)
    /// </summary>
    Task UpdateStatusAsync(int id, string status, int userId, string? notes = null);

    /// <summary>
    /// جلب الشيكات المستحقة قريباً للتنبيه
    /// </summary>
    Task<IEnumerable<ChequeDto>> GetDueChequesAsync(int daysAhead);

    /// <summary>
    /// إلغاء شيك
    /// </summary>
    Task CancelAsync(int id, int userId, string reason);
}
