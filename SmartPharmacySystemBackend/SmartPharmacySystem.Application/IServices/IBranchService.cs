using SmartPharmacySystem.Application.DTOs.Branches;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IBranchService
{
    Task<BranchDto> GetByIdAsync(int id);
    Task<BranchDto> GetByCodeAsync(string branchCode);
    Task<IEnumerable<BranchDto>> GetAllAsync(string? search = null, bool? isActive = null, BranchType? type = null);
    Task<IEnumerable<BranchDto>> GetActiveBranchesAsync();
    Task<BranchDto> CreateAsync(CreateBranchDto dto);
    Task UpdateAsync(UpdateBranchDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<BranchDto>> GetByTypeAsync(BranchType type);
    Task<int> GetBranchCountAsync();
    Task<bool> CodeExistsAsync(string branchCode, int? excludeId = null);
}
