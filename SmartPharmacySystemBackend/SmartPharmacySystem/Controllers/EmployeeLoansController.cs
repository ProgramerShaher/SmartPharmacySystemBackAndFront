using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.EmployeeLoans;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeLoansController : ControllerBase
{
    private readonly IEmployeeLoanService _loanService;
    public EmployeeLoansController(IEmployeeLoanService loanService) => _loanService = loanService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _loanService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeLoanDto>.Succeeded(result, "تم جلب السلفة بنجاح"));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var result = await _loanService.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<EmployeeLoanDto>>.Succeeded(result, "تم جلب سلف الموظف"));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] int? branchId)
    {
        var result = await _loanService.GetActiveLoansAsync(branchId);
        return Ok(ApiResponse<IEnumerable<EmployeeLoanDto>>.Succeeded(result, "تم جلب السلف النشطة"));
    }

    [HttpGet("paid")]
    public async Task<IActionResult> GetFullyPaid()
    {
        var result = await _loanService.GetFullyPaidLoansAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeLoanDto>>.Succeeded(result, "تم جلب السلف المسددة"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeLoanDto dto)
    {
        var result = await _loanService.CreateAsync(dto);
        return Ok(ApiResponse<EmployeeLoanDto>.Succeeded(result, "تم إضافة السلفة بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateEmployeeLoanDto dto)
    {
        await _loanService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث السلفة", "تم التحديث"));
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> RecordPayment(int id, [FromQuery] decimal amount)
    {
        var result = await _loanService.RecordPaymentAsync(id, amount);
        return Ok(ApiResponse<EmployeeLoanDto>.Succeeded(result, "تم تسجيل الدفعة بنجاح"));
    }

    [HttpGet("employee/{employeeId}/remaining")]
    public async Task<IActionResult> GetRemaining(int employeeId)
    {
        var balance = await _loanService.GetTotalRemainingAsync(employeeId);
        return Ok(ApiResponse<decimal>.Succeeded(balance, "تم جلب الرصيد المتبقي"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _loanService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف السلفة بنجاح", "تم الحذف"));
    }
}
