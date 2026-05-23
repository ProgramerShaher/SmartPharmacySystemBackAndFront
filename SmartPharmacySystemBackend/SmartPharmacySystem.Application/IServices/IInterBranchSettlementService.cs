using SmartPharmacySystem.Application.DTOs.InterBranchSettlements;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IInterBranchSettlementService
{
    Task<InterBranchSettlementDto> GetByIdAsync(int id);
    Task<IEnumerable<InterBranchSettlementDto>> GetAllAsync(int? fromBranchId = null, int? toBranchId = null, SettlementStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<InterBranchSettlementDto>> GetPendingAsync();
    Task<InterBranchSettlementDto> CreateAsync(CreateInterBranchSettlementDto dto);
    Task<InterBranchSettlementDto> ApproveAsync(int id, int approvedByUserId);
    Task UpdateAsync(InterBranchSettlementDto dto);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalPendingAmountAsync(int branchId);
}
