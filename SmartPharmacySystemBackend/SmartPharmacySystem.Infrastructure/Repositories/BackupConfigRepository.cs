using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class BackupConfigRepository : IBackupConfigRepository
{
    private readonly ApplicationDbContext _context;

    public BackupConfigRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BackupConfiguration?> GetConfigurationAsync()
    {
        return await _context.BackupConfigurations.FirstOrDefaultAsync();
    }

    public async Task UpdateConfigurationAsync(BackupConfiguration configuration)
    {
        var existing = await _context.BackupConfigurations.FirstOrDefaultAsync();
        if (existing == null)
        {
            await _context.BackupConfigurations.AddAsync(configuration);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(configuration);
        }
        await _context.SaveChangesAsync();
    }
}
