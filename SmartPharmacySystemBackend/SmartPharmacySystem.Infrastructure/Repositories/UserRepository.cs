using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the user repository for data access operations.
/// This class provides concrete implementations of user data operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
    }

    public Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity != null)
        {
            _context.Users.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.Users.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, string sortBy, string sortDir, string? role, bool? isDeleted)
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(search) ||
                                     (u.FullName != null && u.FullName.ToLower().Contains(search)));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Include(u => u.Role)
                         .Where(u => u.Role.Name == role);
        }

        // Apply isDeleted filter
        if (isDeleted.HasValue)
            query = query.Where(u => u.IsDeleted == isDeleted.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(u => EF.Property<object>(u, sortBy))
            : query.OrderBy(u => EF.Property<object>(u, sortBy));

        // Apply pagination
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}