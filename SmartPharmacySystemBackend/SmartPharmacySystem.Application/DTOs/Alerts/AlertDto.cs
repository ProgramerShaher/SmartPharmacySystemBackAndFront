// SmartPharmacySystem.Application/DTOs/Alerts/AlertDtos.cs
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Alerts;

public class AlertDto
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public DateTime ExecutionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAlertDto
{
    [Required(ErrorMessage = "„⁄—› «·œ›⁄… „ÿ·Ê»")]
    public int BatchId { get; set; }

    [Required(ErrorMessage = "‰Ê⁄ «· ‰»ÌÂ „ÿ·Ê»")]
    [StringLength(50, ErrorMessage = "‰Ê⁄ «· ‰»ÌÂ ·« Ì„ﬂ‰ √‰ Ì Ã«Ê“ 50 Õ—›")]
    public string AlertType { get; set; } = string.Empty;

    [Required(ErrorMessage = " «—ÌŒ «· ‰›Ì– „ÿ·Ê»")]
    public DateTime ExecutionDate { get; set; }

    [Required(ErrorMessage = "«·—”«·… „ÿ·Ê»…")]
    [StringLength(500, ErrorMessage = "«·—”«·… ·« Ì„ﬂ‰ √‰   Ã«Ê“ 500 Õ—›")]
    public string Message { get; set; } = string.Empty;
}

public class UpdateAlertDto
{
    [Required(ErrorMessage = "„⁄—› «· ‰»ÌÂ „ÿ·Ê»")]
    public int Id { get; set; }

    [StringLength(50, ErrorMessage = "‰Ê⁄ «· ‰»ÌÂ ·« Ì„ﬂ‰ √‰ Ì Ã«Ê“ 50 Õ—›")]
    public string AlertType { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "«·Õ«·… ·« Ì„ﬂ‰ √‰   Ã«Ê“ 20 Õ—›")]
    public string Status { get; set; } = "Pending";

    [StringLength(500, ErrorMessage = "«·—”«·… ·« Ì„ﬂ‰ √‰   Ã«Ê“ 500 Õ—›")]
    public string Message { get; set; } = string.Empty;
}

public class AlertFilterDto
{
    public int? BatchId { get; set; }
    public string? Status { get; set; }
    public string? AlertType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}