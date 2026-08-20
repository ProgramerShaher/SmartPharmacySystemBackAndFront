using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.MonthlySalaries;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonthlySalariesController : ControllerBase
{
    private readonly IMonthlySalaryService _salaryService;
    private readonly ICurrentUserService _currentUserService;

    public MonthlySalariesController(IMonthlySalaryService salaryService, ICurrentUserService currentUserService)
    {
        _salaryService = salaryService;
        _currentUserService = currentUserService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _salaryService.GetByIdAsync(id);
        return Ok(ApiResponse<MonthlySalaryDto>.Succeeded(result, "تم جلب الراتب بنجاح"));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var result = await _salaryService.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<MonthlySalaryDto>>.Succeeded(result, "تم جلب رواتب الموظف"));
    }

    [HttpGet("by-month")]
    public async Task<IActionResult> GetByMonthYear([FromQuery] int month, [FromQuery] int year, [FromQuery] int? branchId)
    {
        var result = await _salaryService.GetByMonthYearAsync(month, year, branchId);
        return Ok(ApiResponse<IEnumerable<MonthlySalaryDto>>.Succeeded(result, "تم جلب رواتب الشهر"));
    }

    [HttpGet("employee/{employeeId}/month")]
    public async Task<IActionResult> GetByEmployeeMonthYear(int employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        var result = await _salaryService.GetByEmployeeMonthYearAsync(employeeId, month, year);
        return Ok(ApiResponse<MonthlySalaryDto?>.Succeeded(result, "تم جلب الراتب"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMonthlySalaryDto dto)
    {
        var result = await _salaryService.CreateAsync(dto);
        return Ok(ApiResponse<MonthlySalaryDto>.Succeeded(result, "تم إنشاء الراتب بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMonthlySalaryDto dto)
    {
        await _salaryService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث الراتب", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _salaryService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الراتب بنجاح", "تم الحذف"));
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PaySalary(int id, PaySalaryDto dto)
    {
        var result = await _salaryService.PaySalaryAsync(id, dto, _currentUserService.UserId);
        return Ok(ApiResponse<MonthlySalaryDto>.Succeeded(result, "تم اعتماد ودفع الراتب بنجاح"));
    }

    [HttpPost("pay-all")]
    public async Task<IActionResult> PayAll([FromQuery] int month, [FromQuery] int year, [FromQuery] int? branchId, PaySalaryDto dto)
    {
        var count = await _salaryService.PayAllAsync(month, year, branchId, dto, _currentUserService.UserId);
        return Ok(ApiResponse<int>.Succeeded(count, $"تم اعتماد ودفع {count} رواتب بنجاح"));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year, [FromQuery] int? branchId)
    {
        var result = await _salaryService.GetPayrollSummaryAsync(month, year, branchId);
        return Ok(ApiResponse<PayrollSummaryDto>.Succeeded(result, "تم جلب ملخص الرواتب"));
    }
}
