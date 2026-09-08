using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class BackupHistoryFilterDto
{
    public BackupStatus? Status { get; set; }
    public BackupType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
