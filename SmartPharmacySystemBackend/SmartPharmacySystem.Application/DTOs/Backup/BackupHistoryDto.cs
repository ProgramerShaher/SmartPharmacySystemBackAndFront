using System;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class BackupHistoryDto
{
    public int Id { get; set; }
    public string OccurrenceId { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public BackupType BackupType { get; set; }
    public BackupStatus Status { get; set; }
    public UploadStatus UploadStatus { get; set; }
    public bool IsRecovery { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationMessage { get; set; }
    public string? GoogleDriveWebViewLink { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TriggeredByUserName { get; set; }
    public int RetryCount { get; set; }
}
