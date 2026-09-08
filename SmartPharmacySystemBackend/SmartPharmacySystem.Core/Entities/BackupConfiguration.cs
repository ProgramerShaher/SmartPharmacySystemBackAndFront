using System;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class BackupConfiguration : BaseEntity
{
    // Scheduling
    public bool IsAutoBackupEnabled { get; set; } = false;
    public BackupFrequency Frequency { get; set; } = BackupFrequency.Daily;
    public int IntervalDays { get; set; } = 1;          // For Custom
    public TimeSpan ScheduledTime { get; set; } = new TimeSpan(2, 0, 0); // 02:00 AM
    [MaxLength(100)]
    public string Timezone { get; set; } = "Arab Standard Time";
    
    public DateTime? NextScheduledRun { get; set; }     // UTC
    public DateTime? LastRunAt { get; set; }            // UTC

    // Retention
    public BackupRetentionMode RetentionMode { get; set; } = BackupRetentionMode.ByCount;
    public int MaxBackupsToKeep { get; set; } = 30;
    public int RetentionDays { get; set; } = 30;
    public int LocalRetentionDays { get; set; } = 2;

    // Storage
    [MaxLength(255)]
    public string? GoogleDriveFolderId { get; set; }
    [MaxLength(255)]
    public string? GoogleDriveFolderName { get; set; }

    // Options
    public bool VerifyAfterBackup { get; set; } = true;
    public bool EncryptBackup { get; set; } = true;
    
    [MaxLength(500)]
    public string TempBackupPath { get; set; } = "backups\\temp";
}
