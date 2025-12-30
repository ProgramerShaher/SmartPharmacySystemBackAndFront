using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Notifications;

public class ExpiryAlertDto
{
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public int Quantity { get; set; }
    public ExpiryAlertLevel AlertLevel { get; set; }
    public string AlertLevelText { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty; // Hex Code
}
