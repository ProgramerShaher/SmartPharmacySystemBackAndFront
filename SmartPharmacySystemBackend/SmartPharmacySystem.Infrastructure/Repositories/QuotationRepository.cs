using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// تطبيق مستودع عروض الأسعار
/// </summary>
public class QuotationRepository : IQuotationRepository
{
    private readonly ApplicationDbContext _context;

    public QuotationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Quotation?> GetByIdAsync(int id)
    {
        return await _context.Quotations
            .Include(q => q.QuotationDetails)
                .ThenInclude(d => d.Medicine)
            .Include(q => q.QuotationDetails)
                .ThenInclude(d => d.SaleUnit)
            .Include(q => q.Customer)
            .Include(q => q.ConvertedSaleInvoice)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
    }

    public async Task<Quotation?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Quotations
            .AsNoTracking()
            .Include(q => q.QuotationDetails)
                .ThenInclude(d => d.Medicine)
            .Include(q => q.QuotationDetails)
                .ThenInclude(d => d.SaleUnit)
            .Include(q => q.Customer)
            .Include(q => q.ConvertedSaleInvoice)
            .Include(q => q.Branch)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
    }

    public async Task<Quotation?> GetByNumberAsync(string quotationNumber)
    {
        return await _context.Quotations
            .AsNoTracking()
            .Include(q => q.QuotationDetails)
                .ThenInclude(d => d.Medicine)
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber && !q.IsDeleted);
    }

    public async Task<IEnumerable<Quotation>> GetAllAsync(
        int? branchId = null,
        int? customerId = null,
        QuotationStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.QuotationDetails)
            .Where(q => !q.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(q => q.BranchId == branchId.Value);

        if (customerId.HasValue)
            query = query.Where(q => q.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(q => q.QuotationDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(q => q.QuotationDate <= dateTo.Value);

        return await query.OrderByDescending(q => q.QuotationDate).ToListAsync();
    }

    public async Task<(IEnumerable<Quotation> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int? branchId,
        int? customerId,
        QuotationStatus? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize)
    {
        var query = _context.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.QuotationDetails)
            .Where(q => !q.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(q => q.BranchId == branchId.Value);

        if (customerId.HasValue)
            query = query.Where(q => q.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(q => q.QuotationDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(q => q.QuotationDate <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(q =>
                q.QuotationNumber.Contains(search) ||
                (q.CustomerName != null && q.CustomerName.Contains(search)) ||
                (q.CustomerPhone != null && q.CustomerPhone.Contains(search)) ||
                (q.Customer != null && q.Customer.Name.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.QuotationDate)
            .ThenByDescending(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<string> GetNextQuotationNumberAsync(int branchId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"QT-{year}-";
        var count = await _context.Quotations
            .IgnoreQueryFilters()
            .CountAsync(q => q.BranchId == branchId && q.QuotationNumber.StartsWith(prefix));

        return $"{prefix}{(count + 1):D6}";
    }

    public async Task AddAsync(Quotation quotation)
    {
        await _context.Quotations.AddAsync(quotation);
    }

    public Task UpdateAsync(Quotation quotation)
    {
        _context.Quotations.Update(quotation);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation != null)
        {
            quotation.IsDeleted = true;
            quotation.DeletedAt = DateTime.UtcNow;
            _context.Quotations.Update(quotation);
        }
    }
}
