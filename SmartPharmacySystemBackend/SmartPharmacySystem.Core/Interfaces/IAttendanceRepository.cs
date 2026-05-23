using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(int id);
    Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<Attendance>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<Attendance?> GetTodayAttendanceAsync(int employeeId);
    Task<Attendance> AddAsync(Attendance attendance);
    Task UpdateAsync(Attendance attendance);
    Task DeleteAsync(int id);
    Task<int> GetAbsentCountAsync(int branchId, DateTime date);
}
