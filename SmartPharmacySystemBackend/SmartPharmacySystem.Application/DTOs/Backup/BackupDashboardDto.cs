using System;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class BackupDashboardDto
{
    public bool IsSubsystemHealthy { get; set; }
    public string SubsystemHealthMessage { get; set; } = string.Empty;
    public int TotalBackups { get; set; }
    public int SuccessfulBackups { get; set; }
    public int FailedBackups { get; set; }
    public DateTime? LastSuccessfulBackup { get; set; }
    public DateTime? NextScheduledBackup { get; set; }
    public BackupHistoryDto? ActiveBackup { get; set; }
}
