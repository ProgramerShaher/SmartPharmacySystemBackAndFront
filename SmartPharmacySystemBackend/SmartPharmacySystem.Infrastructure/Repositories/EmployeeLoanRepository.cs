using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class EmployeeLoanRepository : IEmployeeLoanRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeLoanRepository(ApplicationDbContext context) => _context = context;

    public async Task<EmployeeLoan?> GetByIdAsync(int id)
    {
        return await _context.EmployeeLoans
            .AsNoTracking()
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
    }

    public async Task<IEnumerable<EmployeeLoan>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeLoans
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && !l.IsDeleted)
            .OrderByDescending(l => l.StartYear).ThenByDescending(l => l.StartMonth)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeLoan>> GetActiveLoansAsync(int? branchId = null)
    {
        var query = _context.EmployeeLoans.AsNoTracking()
            .Include(l => l.Employee).ThenInclude(e => e.Branch)
            .Where(l => !l.IsFullyPaid && !l.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(l => l.Employee.BranchId == branchId.Value);

        return await query.OrderBy(l => l.Employee.FullName).ToListAsync();
    }

    public async Task<IEnumerable<EmployeeLoan>> GetFullyPaidLoansAsync()
    {
        return await _context.EmployeeLoans
            .AsNoTracking()
            .Include(l => l.Employee)
            .Where(l => l.IsFullyPaid && !l.IsDeleted)
            .OrderByDescending(l => l.StartYear)
            .ToListAsync();
    }

    public async Task<EmployeeLoan> AddAsync(EmployeeLoan loan)
    {
        await _context.EmployeeLoans.AddAsync(loan);
        return loan;
    }

    public async Task UpdateAsync(EmployeeLoan loan)
    {
        _context.EmployeeLoans.Update(loan);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await _context.EmployeeLoans.FindAsync(id);
        if (loan != null)
        {
            loan.IsDeleted = true;
            _context.EmployeeLoans.Update(loan);
        }
    }

    public async Task<decimal> GetTotalRemainingAsync(int employeeId)
    {
        return await _context.EmployeeLoans
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && !l.IsFullyPaid && !l.IsDeleted)
            .SumAsync(l => l.RemainingAmount);
    }
}
