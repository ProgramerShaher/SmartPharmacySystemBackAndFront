using System;

namespace SmartPharmacySystem.Core.Enums;

public enum BackupStatus
{
    Pending,
    SqlBackupRunning,
    VerificationRunning,
    EncryptionRunning,
    Uploading,
    Retrying,
    Completed,
    Failed
}

public enum BackupType
{
    Manual,
    Automatic
}

public enum UploadStatus
{
    NotStarted,
    Uploading,
    Completed,
    Failed,
    Skipped
}

public enum BackupFrequency
{
    Daily,
    Every2Days,
    Every3Days,
    Weekly,
    Monthly,
    Custom
}

public enum BackupRetentionMode
{
    ByCount,
    ByDays
}
