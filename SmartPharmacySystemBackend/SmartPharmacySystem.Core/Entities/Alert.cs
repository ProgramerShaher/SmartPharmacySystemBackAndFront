using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents alerts for medicine batches.
/// Alerts notify about upcoming expiry dates.
/// </summary>
public class Alert
{
    /// <summary>
    /// Unique identifier for the alert.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Type of alert (expiry-related or other).
    /// نوع التنبيه (متعلق بالصلاحية أو غيره).
    /// </summary>
    public AlertType AlertType { get; set; }

    /// <summary>
    /// Date when the alert was executed.
    /// </summary>
    public DateTime ExecutionDate { get; set; }

    /// <summary>
    /// Status of the alert.
    /// حالة التنبيه.
    /// </summary>
    public AlertStatus Status { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Date and time when the alert was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }
}