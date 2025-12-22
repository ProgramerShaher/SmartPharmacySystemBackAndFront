using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Alerts;

public class AlertQueryDto : BaseQueryDto
{
    public AlertStatus? Status { get; set; }
    public int? BatchId { get; set; }
}
