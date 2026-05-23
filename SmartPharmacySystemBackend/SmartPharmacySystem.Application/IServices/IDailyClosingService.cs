using SmartPharmacySystem.Application.DTOs.DailyClosings;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IDailyClosingService
{
    Task<DailyClosingDto> GetByIdAsync(int id);
    Task<DailyClosingDto?> GetByBranchDateAsync(int branchId, DateTime date);
    Task<IEnumerable<DailyClosingDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<DailyClosingDto>> GetByDateAsync(DateTime date);
    Task<IEnumerable<DailyClosingDto>> GetPendingApprovalAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<DailyClosingDto> CreateAsync(CreateDailyClosingDto dto);
    Task UpdateAsync(DailyClosingDto dto);
    Task<DailyClosingDto> ApproveAsync(int id, int approvedByUserId);
    Task DeleteAsync(int id);
    Task<IEnumerable<DailyClosingDto>> GetApprovedAsync(DateTime dateFrom, DateTime dateTo, int? branchId = null);
    Task<bool> ExistsAsync(int branchId, DateTime date);
}
