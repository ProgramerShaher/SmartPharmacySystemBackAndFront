using System;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class BackupConfigDto
{
    public bool IsAutoBackupEnabled { get; set; }
    public BackupFrequency Frequency { get; set; }
    public int IntervalDays { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public DateTime? NextScheduledRun { get; set; }
    public BackupRetentionMode RetentionMode { get; set; }
    public int MaxBackupsToKeep { get; set; }
    public int RetentionDays { get; set; }
    public int LocalRetentionDays { get; set; }
    public string? GoogleDriveFolderId { get; set; }
    public string? GoogleDriveFolderName { get; set; }
    public bool VerifyAfterBackup { get; set; }
    public bool EncryptBackup { get; set; }
}
