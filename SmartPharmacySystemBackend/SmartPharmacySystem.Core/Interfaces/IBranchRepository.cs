using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch?> GetByCodeAsync(string branchCode);
    Task<IEnumerable<Branch>> GetAllAsync(string? search = null, bool? isActive = null, BranchType? type = null);
    Task<Branch> AddAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string branchCode, int? excludeId = null);
    Task<IEnumerable<Branch>> GetActiveBranchesAsync();
    Task<int> GetBranchCountAsync();
    Task<IEnumerable<Branch>> GetByTypeAsync(BranchType type);
}
