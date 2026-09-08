using System;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class UpdateBackupConfigDto
{
    public bool IsAutoBackupEnabled { get; set; }
    public BackupFrequency Frequency { get; set; }
    
    [Range(1, 365)]
    public int IntervalDays { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    
    [Required(ErrorMessage = "النطاق الزمني مطلوب.")]
    [MaxLength(100)]
    public string Timezone { get; set; } = string.Empty;
    
    public BackupRetentionMode RetentionMode { get; set; }
    
    [Range(1, 3650)]
    public int MaxBackupsToKeep { get; set; }
    
    [Range(1, 3650)]
    public int RetentionDays { get; set; }
    
    [Range(1, 30)]
    public int LocalRetentionDays { get; set; }
    
    [MaxLength(255)]
    public string? GoogleDriveFolderId { get; set; }
    
    [MaxLength(255)]
    public string? GoogleDriveFolderName { get; set; }
    
    public bool VerifyAfterBackup { get; set; }
    public bool EncryptBackup { get; set; }
}
