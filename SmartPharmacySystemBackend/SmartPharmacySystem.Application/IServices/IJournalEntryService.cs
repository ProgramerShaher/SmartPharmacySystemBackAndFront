using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.IServices;

/// <summary>
/// واجهة خدمة القيود المحاسبية (القيود اليومية)
/// </summary>
public interface IJournalEntryService
{
    /// <summary>
    /// جلب قيد محاسبي بواسطة المعرف
    /// </summary>
    Task<JournalEntryDto> GetByIdAsync(int id);

    /// <summary>
    /// البحث في القيود المحاسبية مع الفلترة
    /// </summary>
    Task<PagedResponse<JournalEntryDto>> GetPagedAsync(int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status);

    /// <summary>
    /// إنشاء قيد محاسبي جديد (مسودة)
    /// </summary>
    Task<JournalEntryDto> CreateAsync(JournalEntryDto dto, int? userId);

    /// <summary>
    /// تحديث قيد محاسبي موجود (في حالة المسودة فقط)
    /// </summary>
    Task UpdateAsync(JournalEntryDto dto, int? userId);

    /// <summary>
    /// اعتماد وترحيل القيد المحاسبي
    /// </summary>
    Task ApproveAsync(int id, int? userId);

    /// <summary>
    /// إلغاء قيد محاسبي
    /// </summary>
    Task CancelAsync(int id, int? userId, string reason);

    /// <summary>
    /// إنشاء قيد عكسي لتصحيح خطأ محاسبي
    /// </summary>
    Task ReverseAsync(int id, int? userId, string reason);

    /// <summary>
    /// التحقق من توازن القيد (إجمالي المدين = إجمالي الدائن)
    /// </summary>
    bool IsBalanced(JournalEntryDto dto);
}
