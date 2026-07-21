using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع التعيينات الوظيفية للفروع
/// Defines the contract for employee branch assignment repository operations.
/// </summary>
public interface IEmployeeBranchAssignmentRepository
{
    Task<EmployeeBranchAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<EmployeeBranchAssignment>> GetAllAsync();
    Task AddAsync(EmployeeBranchAssignment assignment);
    Task UpdateAsync(EmployeeBranchAssignment assignment);
    Task DeleteAsync(int id);
    Task<EmployeeBranchAssignment?> GetActiveAssignmentByUserIdAsync(int userId);
    Task<IEnumerable<EmployeeBranchAssignment>> GetAssignmentsByUserIdAsync(int userId);
    Task<IEnumerable<EmployeeBranchAssignment>> GetAssignmentsByBranchIdAsync(int branchId);
}
