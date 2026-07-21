using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// تطبيق مستودع التعيينات الوظيفية للفروع
/// Implementation of the employee branch assignment repository.
/// </summary>
public class EmployeeBranchAssignmentRepository : IEmployeeBranchAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeBranchAssignmentRepository(ApplicationDbContext context) => _context = context;

    public async Task<EmployeeBranchAssignment?> GetByIdAsync(int id)
    {
        return await _context.Set<EmployeeBranchAssignment>()
            .Include(a => a.User)
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<EmployeeBranchAssignment>> GetAllAsync()
    {
        return await _context.Set<EmployeeBranchAssignment>()
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Include(a => a.User)
            .Include(a => a.Branch)
            .ToListAsync();
    }

    public async Task AddAsync(EmployeeBranchAssignment assignment)
    {
        await _context.Set<EmployeeBranchAssignment>().AddAsync(assignment);
    }

    public async Task UpdateAsync(EmployeeBranchAssignment assignment)
    {
        _context.Set<EmployeeBranchAssignment>().Update(assignment);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var assignment = await _context.Set<EmployeeBranchAssignment>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (assignment != null)
        {
            assignment.IsDeleted = true;
            _context.Set<EmployeeBranchAssignment>().Update(assignment);
        }
    }

    public async Task<EmployeeBranchAssignment?> GetActiveAssignmentByUserIdAsync(int userId)
    {
        return await _context.Set<EmployeeBranchAssignment>()
            .AsNoTracking()
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive && !a.IsDeleted);
    }

    public async Task<IEnumerable<EmployeeBranchAssignment>> GetAssignmentsByUserIdAsync(int userId)
    {
        return await _context.Set<EmployeeBranchAssignment>()
            .AsNoTracking()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .Include(a => a.Branch)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeBranchAssignment>> GetAssignmentsByBranchIdAsync(int branchId)
    {
        return await _context.Set<EmployeeBranchAssignment>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted)
            .Include(a => a.User)
            .ToListAsync();
    }
}
