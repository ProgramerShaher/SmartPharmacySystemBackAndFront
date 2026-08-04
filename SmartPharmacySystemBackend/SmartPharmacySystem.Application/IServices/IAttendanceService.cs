using SmartPharmacySystem.Application.DTOs.Attendances;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceDto> GetByIdAsync(int id);
    Task<IEnumerable<AttendanceDto>> GetByEmployeeIdAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<AttendanceDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<AttendanceDto?> GetTodayAttendanceAsync(int employeeId);
    Task<AttendanceDto> CheckInAsync(CreateAttendanceDto dto);
    Task<AttendanceDto> CheckOutAsync(int attendanceId, DateTime checkOutTime);
    Task UpdateAsync(UpdateAttendanceDto dto);
    Task DeleteAsync(int id);
    Task<int> GetAbsentCountAsync(int branchId, DateTime date);
    Task<AttendanceDto> MarkAbsentAsync(int employeeId, DateTime date);
    Task<AttendanceDto> MarkPresentAsync(int employeeId, DateTime date);
}
