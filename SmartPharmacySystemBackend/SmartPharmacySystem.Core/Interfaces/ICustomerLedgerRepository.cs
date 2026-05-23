using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface ICustomerLedgerRepository
{
    Task<CustomerLedger?> GetByIdAsync(int id);
    Task<IEnumerable<CustomerLedger>> GetByCustomerIdAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<CustomerLedger>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task<decimal> GetCustomerBalanceAtBranchAsync(int customerId, int branchId);
    Task<CustomerLedger> AddAsync(CustomerLedger entry);
    Task DeleteAsync(int id);
    Task<IEnumerable<CustomerLedger>> SearchAsync(int? customerId = null, int? branchId = null, CustomerTransactionType? type = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
