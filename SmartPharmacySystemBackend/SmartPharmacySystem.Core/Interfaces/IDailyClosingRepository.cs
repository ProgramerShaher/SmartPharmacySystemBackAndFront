using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IDailyClosingRepository
{
    Task<DailyClosing?> GetByIdAsync(int id);
    Task<DailyClosing?> GetByBranchDateAsync(int branchId, DateTime date);
    Task<IEnumerable<DailyClosing>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<DailyClosing>> GetByDateAsync(DateTime date);
    Task<IEnumerable<DailyClosing>> GetPendingApprovalAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<DailyClosing> AddAsync(DailyClosing closing);
    Task UpdateAsync(DailyClosing closing);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int branchId, DateTime date);
    Task<IEnumerable<DailyClosing>> GetApprovedAsync(DateTime dateFrom, DateTime dateTo, int? branchId = null);
}
