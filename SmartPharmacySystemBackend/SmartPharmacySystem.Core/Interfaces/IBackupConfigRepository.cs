using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IBackupConfigRepository
{
    Task<BackupConfiguration?> GetConfigurationAsync();
    Task UpdateConfigurationAsync(BackupConfiguration configuration);
}
