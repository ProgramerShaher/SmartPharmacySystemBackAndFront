
using System;

namespace SmartPharmacySystem.Application.DTOs.Alerts
{
    public class UnifiedAlertDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Visual Indicators
        public string StatusColor { get; set; } = string.Empty;
        public string StatusLevel { get; set; } = string.Empty; // e.g., Critical, Warning

        // Data
        public string AlertType { get; set; } = string.Empty; // Expiry vs LowStock
        public int DaysRemaining { get; set; } // For Expiry
        public int Quantity { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int MedicineId { get; set; }
        public int BatchId { get; set; }
    }
}
