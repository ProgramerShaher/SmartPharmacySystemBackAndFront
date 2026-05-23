using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IInterBranchSettlementRepository
{
    Task<InterBranchSettlement?> GetByIdAsync(int id);
    Task<IEnumerable<InterBranchSettlement>> GetAllAsync(int? fromBranchId = null, int? toBranchId = null, SettlementStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<InterBranchSettlement>> GetPendingAsync();
    Task<InterBranchSettlement> AddAsync(InterBranchSettlement settlement);
    Task UpdateAsync(InterBranchSettlement settlement);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalPendingAmountAsync(int branchId);
}
