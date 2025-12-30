namespace SmartPharmacySystem.Core.Enums
{
    public enum ExpiryAlertLevel
    {
        Normal = 1,     // < 60 days
        Medium = 2,     // < 30 days
        High = 3,       // < 14 days
        Critical = 4    // < 7 days
    }
}
