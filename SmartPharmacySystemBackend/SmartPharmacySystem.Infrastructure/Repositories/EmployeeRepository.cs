using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context) => _context = context;

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<Employee?> GetByCodeAsync(string employeeCode)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(int? branchId = null, int? departmentId = null, bool? isActive = null, string? search = null)
    {
        var query = _context.Employees.AsNoTracking().Where(e => !e.IsDeleted).AsQueryable();

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.FullName.Contains(search) || e.EmployeeCode.Contains(search) || e.NationalId.Contains(search));

        return await query
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.BranchId == branchId && !e.IsDeleted)
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public async Task<Employee> AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        return employee;
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (employee != null)
        {
            employee.IsDeleted = true;
            _context.Employees.Update(employee);
        }
    }

    public async Task<bool> CodeExistsAsync(string employeeCode, int? excludeId = null)
    {
        var query = _context.Employees.AsNoTracking().Where(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null)
    {
        var query = _context.Employees.AsNoTracking().Where(e => e.NationalId == nationalId && !e.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<int> GetEmployeeCountAsync(int branchId)
    {
        return await _context.Employees.AsNoTracking().CountAsync(e => e.BranchId == branchId && !e.IsDeleted);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Department)
            .Where(e => e.IsActive && !e.IsDeleted)
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }
}
