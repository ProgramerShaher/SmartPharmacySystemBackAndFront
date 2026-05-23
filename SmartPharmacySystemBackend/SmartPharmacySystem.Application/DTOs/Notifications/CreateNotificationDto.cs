using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Notifications;

/// <summary>
/// كائن نقل البيانات لإنشاء إشعار جديد.
/// </summary>
public class CreateNotificationDto
{
    [Required(ErrorMessage = "المستخدم مطلوب")]
    public int UserId { get; set; }

    public int? BranchId { get; set; }

    [Required(ErrorMessage = "نوع الإشعار مطلوب")]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [MaxLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "المحتوى مطلوب")]
    [MaxLength(1000, ErrorMessage = "المحتوى يجب أن لا يتجاوز 1000 حرف")]
    public string Body { get; set; } = string.Empty;

    public int? ReferenceId { get; set; }

    [MaxLength(100, ErrorMessage = "نوع المرجع يجب أن لا يتجاوز 100 حرف")]
    public string? ReferenceType { get; set; }
}
