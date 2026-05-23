using SmartPharmacySystem.Application.DTOs.CustomerLedgers;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface ICustomerLedgerService
{
    Task<CustomerLedgerDto> GetByIdAsync(int id);
    Task<IEnumerable<CustomerLedgerDto>> GetByCustomerIdAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<CustomerLedgerDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task<decimal> GetCustomerBalanceAtBranchAsync(int customerId, int branchId);
    Task<CustomerLedgerDto> AddEntryAsync(CreateCustomerLedgerDto dto);
    Task<IEnumerable<CustomerLedgerDto>> SearchAsync(int? customerId = null, int? branchId = null, CustomerTransactionType? type = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
