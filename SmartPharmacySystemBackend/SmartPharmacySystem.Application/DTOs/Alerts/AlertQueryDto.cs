using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Alerts;

public class AlertQueryDto : BaseQueryDto
{
    public AlertSeverity? Severity { get; set; }
    public bool? IsRead { get; set; }
    public int? BatchId { get; set; }
}
