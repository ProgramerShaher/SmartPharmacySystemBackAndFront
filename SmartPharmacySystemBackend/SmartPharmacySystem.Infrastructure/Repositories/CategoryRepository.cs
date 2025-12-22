using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Category entity)
        {
            await _context.Categories.AddAsync(entity);
        }

        public Task UpdateAsync(Category entity)
        {
            _context.Categories.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                category.IsDeleted = true;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(
            string? search, int page, int pageSize, string sortBy, string sortDir)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                         (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(c => EF.Property<object>(c, sortBy))
                : query.OrderBy(c => EF.Property<object>(c, sortBy));

            // Apply pagination
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
    }
}
