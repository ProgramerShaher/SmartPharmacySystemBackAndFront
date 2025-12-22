// SmartPharmacySystem.Infrastructure/Repositories/AlertRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly ApplicationDbContext _context;

    public AlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Alert>> GetAllAsync()
    {
        return await _context.Alerts
            .Include(a => a.Batch)
                .ThenInclude(b => b.Medicine)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.ExecutionDate)
            .ToListAsync();
    }

    public async Task<Alert?> GetByIdAsync(int id)
    {
        return await _context.Alerts
            .Include(a => a.Batch)
                .ThenInclude(b => b.Medicine)
            .Where(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Alert>> GetByBatchIdAsync(int batchId)
    {
        return await _context.Alerts
            .Include(a => a.Batch)
                .ThenInclude(b => b.Medicine)
            .Where(a => a.BatchId == batchId && !a.IsDeleted)
            .OrderByDescending(a => a.ExecutionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetByStatusAsync(AlertStatus status)
    {
        return await _context.Alerts
            .Include(a => a.Batch)
                .ThenInclude(b => b.Medicine)
            .Where(a => a.Status == status && !a.IsDeleted)
            .OrderByDescending(a => a.ExecutionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetExpiringAlertsAsync(DateTime date)
    {
        return await _context.Alerts
            .Include(a => a.Batch)
                .ThenInclude(b => b.Medicine)
            .Where(a => a.ExecutionDate.Date == date.Date &&
                       !a.IsDeleted &&
                       a.Status == AlertStatus.Pending)
            .ToListAsync();
    }

    public async Task AddAsync(Alert alert)
    {
        await _context.Alerts.AddAsync(alert);
    }

    public async Task UpdateAsync(Alert alert)
    {
        _context.Entry(alert).State = EntityState.Modified;
    }

    public async Task DeleteAsync(int id)
    {
        var alert = await GetByIdAsync(id);
        if (alert != null)
        {
            alert.IsDeleted = true;
            await UpdateAsync(alert);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Alerts
            .AnyAsync(a => a.Id == id && !a.IsDeleted);
    }
}