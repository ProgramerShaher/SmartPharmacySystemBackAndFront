using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class MonthlySalaryRepository : IMonthlySalaryRepository
{
    private readonly ApplicationDbContext _context;

    public MonthlySalaryRepository(ApplicationDbContext context) => _context = context;

    public async Task<MonthlySalary?> GetByIdAsync(int id)
    {
        return await _context.MonthlySalaries
            .AsNoTracking()
            .Include(s => s.Employee)
            .Include(s => s.Branch)
            .Include(s => s.PaidFromAccount)
            .Include(s => s.SalaryExpenseAccount)
            .Include(s => s.Deductions)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<MonthlySalary?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year)
    {
        return await _context.MonthlySalaries
            .AsNoTracking()
            .Include(s => s.Employee)
            .Include(s => s.Branch)
            .Include(s => s.PaidFromAccount)
            .Include(s => s.SalaryExpenseAccount)
            .Include(s => s.Deductions)
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                && s.Month == month
                && s.Year == year
                && !s.IsDeleted);
    }

    public async Task<IEnumerable<MonthlySalary>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.MonthlySalaries
            .AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.PaidFromAccount)
            .Include(s => s.SalaryExpenseAccount)
            .Include(s => s.Deductions)
            .Where(s => s.EmployeeId == employeeId && !s.IsDeleted)
            .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonthlySalary>> GetByBranchMonthYearAsync(int branchId, int month, int year)
    {
        return await _context.MonthlySalaries
            .AsNoTracking()
            .Include(s => s.Employee)
            .Include(s => s.Deductions)
            .Include(s => s.PaidFromAccount)
            .Include(s => s.SalaryExpenseAccount)
            .Where(s => s.BranchId == branchId && s.Month == month && s.Year == year && !s.IsDeleted)
            .OrderBy(s => s.Employee.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonthlySalary>> GetByMonthYearAsync(int month, int year, int? branchId = null)
    {
        var query = _context.MonthlySalaries.AsNoTracking()
            .Include(s => s.Employee)
            .Include(s => s.Branch)
            .Include(s => s.PaidFromAccount)
            .Include(s => s.SalaryExpenseAccount)
            .Include(s => s.Deductions)
            .Where(s => s.Month == month && s.Year == year && !s.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query.OrderBy(s => s.BranchId).ThenBy(s => s.Employee.FullName).ToListAsync();
    }

    public async Task<MonthlySalary> AddAsync(MonthlySalary salary)
    {
        await _context.MonthlySalaries.AddAsync(salary);
        return salary;
    }

    public async Task UpdateAsync(MonthlySalary salary)
    {
        _context.MonthlySalaries.Update(salary);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var salary = await _context.MonthlySalaries.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (salary != null)
        {
            salary.IsDeleted = true;
            _context.MonthlySalaries.Update(salary);
        }
    }

    public async Task<bool> ExistsAsync(int employeeId, int month, int year)
    {
        return await _context.MonthlySalaries
            .AsNoTracking()
            .AnyAsync(s => s.EmployeeId == employeeId && s.Month == month && s.Year == year && !s.IsDeleted);
    }

    public async Task<decimal> GetTotalPayrollAsync(int month, int year, int? branchId = null)
    {
        var query = _context.MonthlySalaries.AsNoTracking()
            .Where(s => s.Month == month && s.Year == year && !s.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query.SumAsync(s => s.BasicSalary + s.TotalAllowances + s.TotalBonuses - s.TotalDeductions);
    }
}
