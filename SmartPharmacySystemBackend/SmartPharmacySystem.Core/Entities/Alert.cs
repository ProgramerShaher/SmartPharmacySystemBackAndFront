using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents alerts for medicine batches using .NET 9 Primary Constructor.
/// يمثل التنبيهات لباتشات الأدوية.
/// </summary>
public class Alert(
    int batchId,
    AlertType alertType,
    AlertSeverity severity,
    string message,
    DateTime? expiryDateSnapshot = null) : BaseEntity
{
    public int BatchId { get; set; } = batchId;
    public AlertType AlertType { get; set; } = alertType;
    public AlertSeverity Severity { get; set; } = severity;
    public string Message { get; set; } = message;
    public DateTime? ExpiryDateSnapshot { get; set; } = expiryDateSnapshot;

    public bool IsRead { get; set; } = false;

    // Navigation property
    public MedicineBatch Batch { get; set; } = null!;

    /// <summary>
    /// For EF Core parameterless constructor requirement
    /// </summary>
    protected Alert() : this(0, AlertType.LowStock, AlertSeverity.Info, string.Empty) { }
}