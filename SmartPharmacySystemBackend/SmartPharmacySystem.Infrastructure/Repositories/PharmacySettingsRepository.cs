using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class PharmacySettingsRepository : IPharmacySettingsRepository
{
    private readonly ApplicationDbContext _context;

    public PharmacySettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PharmacySettings?> GetSettingsAsync()
    {
        // There should be only one settings record, we take the first one
        return await _context.PharmacySettings.FirstOrDefaultAsync();
    }

    public async Task AddAsync(PharmacySettings settings)
    {
        await _context.PharmacySettings.AddAsync(settings);
    }

    public async Task UpdateAsync(PharmacySettings settings)
    {
        _context.Entry(settings).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}
