using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع الحسابات المحاسبية
/// تحديث العمليات الخاصة بشجرة الحسابات
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByCodeAsync(string code);
    Task<IEnumerable<Account>> GetAllAsync();
    Task<IEnumerable<Account>> GetMainAccountsAsync();
    Task<IEnumerable<Account>> GetChildrenAsync(int parentId);
    Task AddAsync(Account entity);
    Task UpdateAsync(Account entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> CodeExistsAsync(string code);
    
    /// <summary>
    /// تحديث رصيد الحساب بناءً على الحركات المالية
    /// </summary>
    Task UpdateBalanceAsync(int accountId, decimal amount);
}
