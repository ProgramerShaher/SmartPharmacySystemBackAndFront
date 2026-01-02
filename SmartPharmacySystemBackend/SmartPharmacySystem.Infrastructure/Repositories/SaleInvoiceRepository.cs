using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the sale invoice repository for data access operations.
/// Optimized for performance with AsNoTracking and selective loading.
/// </summary>
public class SaleInvoiceRepository : ISaleInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public SaleInvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets sale invoice by ID with full tracking for updates
    /// استخدم هذه الدالة عند الحاجة لتعديل الفاتورة
    /// </summary>
    public async Task<SaleInvoice> GetByIdAsync(int id)
    {
        return await _context.SaleInvoices
            .Include(s => s.SaleInvoiceDetails)
                .ThenInclude(d => d.Medicine)
            .Include(s => s.SaleInvoiceDetails)
                .ThenInclude(d => d.Batch)
            .Include(s => s.Creator)
            .Include(s => s.Approver)
            .Include(s => s.Canceller)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Gets sale invoice by ID for display only (no tracking)
    /// استخدم هذه الدالة عند عرض الفاتورة فقط بدون تعديل
    /// </summary>
    public async Task<SaleInvoice> GetByIdForDisplayAsync(int id)
    {
        return await _context.SaleInvoices
            .AsNoTracking()
            .Include(s => s.SaleInvoiceDetails)
                .ThenInclude(d => d.Medicine)
            .Include(s => s.SaleInvoiceDetails)
                .ThenInclude(d => d.Batch)
            .Include(s => s.Creator)
            .Include(s => s.Approver)
            .Include(s => s.Canceller)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Gets all sale invoices with pagination and AsNoTracking
    /// محسّن للأداء: يجلب الحقول الأساسية فقط
    /// </summary>
    public async Task<IEnumerable<SaleInvoice>> GetAllAsync()
    {
        return await _context.SaleInvoices
            .AsNoTracking()
            .Include(s => s.Creator)
            .Include(s => s.Approver)
            .Include(s => s.Canceller)
            .OrderByDescending(s => s.InvoiceDate)
            .ThenByDescending(s => s.Id)
            .Take(100) // Limit to prevent timeout
            .ToListAsync();
    }

    /// <summary>
    /// Gets paged sale invoices with optimized query
    /// استعلام محسّن مع Paging و AsNoTracking
    /// </summary>
    public async Task<(IEnumerable<SaleInvoice> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? customerId = null,
        string status = null)
    {
        var query = _context.SaleInvoices
            .AsNoTracking()
            .Include(s => s.Creator)
            .Where(s => !s.IsDeleted);

        // Apply filters
        if (startDate.HasValue)
            query = query.Where(s => s.InvoiceDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.InvoiceDate <= endDate.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status.ToString() == status);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting and pagination
        var items = await query
            .OrderByDescending(s => s.InvoiceDate)
            .ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(SaleInvoice entity)
    {
        await _context.SaleInvoices.AddAsync(entity);
    }

    public Task UpdateAsync(SaleInvoice entity)
    {
        _context.SaleInvoices.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.SaleInvoices.FindAsync(id);
        if (entity != null)
        {
            _context.SaleInvoices.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.SaleInvoices.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.SaleInvoices.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.SaleInvoices
            .AsNoTracking()
            .AnyAsync(si => si.Id == id && !si.IsDeleted);
    }
}