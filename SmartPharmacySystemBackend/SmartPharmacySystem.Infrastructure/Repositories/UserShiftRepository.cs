using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class UserShiftRepository : IUserShiftRepository
{
    private readonly ApplicationDbContext _context;

    public UserShiftRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserShift> GetByIdAsync(int id)
    {
        return await _context.UserShifts.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<UserShift>> GetAllAsync()
    {
        return await _context.UserShifts.ToListAsync();
    }

    public async Task AddAsync(UserShift entity)
    {
        await _context.UserShifts.AddAsync(entity);
    }

    public Task UpdateAsync(UserShift entity)
    {
        _context.UserShifts.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UserShifts.FindAsync(id);
        if (entity != null)
        {
            _context.UserShifts.Remove(entity);
        }
    }
}
