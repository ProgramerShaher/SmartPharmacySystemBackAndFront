using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Alerts;

public class UpdateAlertDto
{
    public int Id { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
