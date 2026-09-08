using System;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class BackupHistory : BaseEntity
{
    [MaxLength(200)]
    public string OccurrenceId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DatabaseName { get; set; } = "PharmacyDB";
    public BackupType BackupType { get; set; }

    public BackupStatus Status { get; set; } = BackupStatus.Pending;
    public UploadStatus UploadStatus { get; set; } = UploadStatus.NotStarted;

    public bool IsRecovery { get; set; } = false;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }

    [MaxLength(500)]
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    
    [MaxLength(1000)]
    public string? LocalTempPath { get; set; }
    [MaxLength(1000)]
    public string? LocalEncryptedPath { get; set; }

    public bool IsVerified { get; set; } = false;
    public string? VerificationMessage { get; set; }

    [MaxLength(255)]
    public string? GoogleDriveFileId { get; set; }
    [MaxLength(1000)]
    public string? GoogleDriveWebViewLink { get; set; }

    public string? ErrorMessage { get; set; }

    public int? TriggeredByUserId { get; set; }
    [MaxLength(200)]
    public string? TriggeredByUserName { get; set; }
    public string? Notes { get; set; }

    public int RetryCount { get; set; } = 0;
    public int? OriginalBackupId { get; set; }
}
