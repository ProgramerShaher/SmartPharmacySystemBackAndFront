using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Alerts;

public class CreateAlertDto
{
    public int BatchId { get; set; }
    public AlertType AlertType { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
}
