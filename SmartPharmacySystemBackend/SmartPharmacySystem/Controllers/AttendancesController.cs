using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Attendances;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    public AttendancesController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _attendanceService.GetByIdAsync(id);
        return Ok(ApiResponse<AttendanceDto>.Succeeded(result, "تم جلب سجل الحضور"));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _attendanceService.GetByEmployeeIdAsync(employeeId, from, to);
        return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Succeeded(result, "تم جلب سجلات الموظف"));
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _attendanceService.GetByBranchIdAsync(branchId, from, to);
        return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Succeeded(result, "تم جلب سجلات الفرع"));
    }

    [HttpGet("employee/{employeeId}/today")]
    public async Task<IActionResult> GetToday(int employeeId)
    {
        var result = await _attendanceService.GetTodayAttendanceAsync(employeeId);
        return Ok(ApiResponse<AttendanceDto?>.Succeeded(result, "تم جلب تسجيل اليوم"));
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(CreateAttendanceDto dto)
    {
        var result = await _attendanceService.CheckInAsync(dto);
        return Ok(ApiResponse<AttendanceDto>.Succeeded(result, "تم تسجيل الحضور بنجاح"));
    }

    [HttpPost("{id}/check-out")]
    public async Task<IActionResult> CheckOut(int id, [FromQuery] DateTime checkOutTime)
    {
        var result = await _attendanceService.CheckOutAsync(id, checkOutTime);
        return Ok(ApiResponse<AttendanceDto>.Succeeded(result, "تم تسجيل الانصراف بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateAttendanceDto dto)
    {
        await _attendanceService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث سجل الحضور", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _attendanceService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف سجل الحضور بنجاح", "تم الحذف"));
    }
}
