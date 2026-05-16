using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .Include(a => a.Parent)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<Account?> GetByCodeAsync(string code)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Code == code && !a.IsDeleted);
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetMainAccountsAsync()
    {
        return await _context.Accounts
            .Where(a => a.ParentId == null && !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetChildrenAsync(int parentId)
    {
        return await _context.Accounts
            .Where(a => a.ParentId == parentId && !a.IsDeleted)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task AddAsync(Account entity)
    {
        await _context.Accounts.AddAsync(entity);
    }

    public Task UpdateAsync(Account entity)
    {
        _context.Accounts.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            account.IsDeleted = true;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        // يجب التحقق في كامل الجدول (بما فيه المحذوف) لأن قاعدة البيانات تمنع التكرار مطلقاً
        return await _context.Accounts.AnyAsync(a => a.Code == code);
    }

    public async Task UpdateBalanceAsync(int accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            account.CurrentBalance += amount;
            
            // Recursive update for parent accounts if needed (summary accounts)
            if (account.ParentId != null)
            {
                await UpdateBalanceAsync(account.ParentId.Value, amount);
            }
        }
    }
}
