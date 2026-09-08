using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class BackupHistoryRepository : IBackupHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public BackupHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BackupHistory?> GetByIdAsync(int id)
    {
        return await _context.BackupHistories.FindAsync(id);
    }

    public async Task<IEnumerable<BackupHistory>> GetAllAsync(int page, int pageSize)
    {
        return await _context.BackupHistories
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.BackupHistories.CountAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetByStatusAsync(BackupStatus status)
    {
        return await _context.BackupHistories
            .Where(h => h.Status == status)
            .ToListAsync();
    }

    public async Task<BackupHistory?> GetByOccurrenceIdAsync(string occurrenceId)
    {
        return await _context.BackupHistories
            .Where(h => h.OccurrenceId == occurrenceId && h.Status == BackupStatus.Completed)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetSuccessfulBackupsForRetentionAsync()
    {
        return await _context.BackupHistories
            .Where(h => h.Status == BackupStatus.Completed)
            .OrderByDescending(h => h.CompletedAt)
            .ToListAsync();
    }

    public async Task AddAsync(BackupHistory history)
    {
        await _context.BackupHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BackupHistory history)
    {
        _context.BackupHistories.Update(history);
        await _context.SaveChangesAsync();
    }
}
