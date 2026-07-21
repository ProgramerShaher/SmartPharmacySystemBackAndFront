using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly ApplicationDbContext _context;

    public AttendanceRepository(ApplicationDbContext context) => _context = context;

    public async Task<Attendance?> GetByIdAsync(int id)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.WorkingBranch)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.Attendances.AsNoTracking()
            .Include(a => a.WorkingBranch)
            .Where(a => a.EmployeeId == employeeId && !a.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(a => a.CheckIn >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.CheckIn <= dateTo.Value);

        return await query.OrderBy(a => a.CheckIn).ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.Attendances.AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.WorkingBranchId == branchId && !a.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(a => a.CheckIn >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.CheckIn <= dateTo.Value);

        return await query.OrderBy(a => a.CheckIn).ToListAsync();
    }

    public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                && a.CheckIn.Date == today
                && !a.IsDeleted);
    }

    public async Task<Attendance> AddAsync(Attendance attendance)
    {
        await _context.Attendances.AddAsync(attendance);
        return attendance;
    }

    public async Task UpdateAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (attendance != null)
        {
            attendance.IsDeleted = true;
            _context.Attendances.Update(attendance);
        }
    }

    public async Task<int> GetAbsentCountAsync(int branchId, DateTime date)
    {
        var today = date.Date;
        return await _context.Attendances
            .AsNoTracking()
            .CountAsync(a => a.WorkingBranchId == branchId
                && a.CheckIn.Date == today
                && a.AttendanceStatus == Core.Enums.AttendanceStatus.Absent
                && !a.IsDeleted);
    }
}
