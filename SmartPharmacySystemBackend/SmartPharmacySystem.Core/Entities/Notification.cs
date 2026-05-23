using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class Notification : BaseEntity
{
    [Required]
    public int UserId { get; set; }

    public int? BranchId { get; set; }

    [Required]
    public NotificationType Type { get; set; } = NotificationType.LowStock;

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    public int? ReferenceId { get; set; }

    [MaxLength(100)]
    public string? ReferenceType { get; set; }

    [Required]
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
}
