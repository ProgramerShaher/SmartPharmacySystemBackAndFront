using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context) => _context = context;

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Employees.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Employees.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(d => d.Name == name && !d.IsDeleted);
    }

    public async Task<IEnumerable<Department>> GetAllAsync(string? search = null )
    {
        var query = _context.Departments
            .AsNoTracking()
            .Include(d => d.Employees.Where(e => !e.IsDeleted))
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<Department> AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (department != null)
        {
            department.IsDeleted = true;
            _context.Departments.Update(department);
        }
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Departments.AsNoTracking().Where(d => d.Name == name && !d.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
