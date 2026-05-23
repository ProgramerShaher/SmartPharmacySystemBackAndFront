using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Employees;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeDto>.Succeeded(result, "تم جلب بيانات الموظف بنجاح"));
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _employeeService.GetByCodeAsync(code);
        return Ok(ApiResponse<EmployeeDto>.Succeeded(result, "تم جلب الموظف بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId, [FromQuery] int? departmentId, [FromQuery] bool? isActive, [FromQuery] string? search)
    {
        var result = await _employeeService.GetAllAsync(branchId, departmentId, isActive, search);
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Succeeded(result, "تم جلب الموظفين بنجاح"));
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId)
    {
        var result = await _employeeService.GetByBranchIdAsync(branchId);
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Succeeded(result, "تم جلب موظفي الفرع"));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _employeeService.GetActiveEmployeesAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Succeeded(result, "تم جلب الموظفين النشطين"));
    }

    [HttpGet("count/{branchId}")]
    public async Task<IActionResult> GetCount(int branchId)
    {
        var count = await _employeeService.GetEmployeeCountAsync(branchId);
        return Ok(ApiResponse<int>.Succeeded(count, "تم جلب عدد الموظفين"));
    }

    [HttpGet("code-exists")]
    public async Task<IActionResult> CodeExists([FromQuery] string code, [FromQuery] int? excludeId)
    {
        var exists = await _employeeService.CodeExistsAsync(code, excludeId);
        return Ok(ApiResponse<bool>.Succeeded(exists, "تم التحقق من كود الموظف"));
    }

    [HttpGet("national-id-exists")]
    public async Task<IActionResult> NationalIdExists([FromQuery] string nationalId, [FromQuery] int? excludeId)
    {
        var exists = await _employeeService.NationalIdExistsAsync(nationalId, excludeId);
        return Ok(ApiResponse<bool>.Succeeded(exists, "تم التحقق من الرقم القومي"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        var result = await _employeeService.CreateAsync(dto);
        return Ok(ApiResponse<EmployeeDto>.Succeeded(result, "تم إضافة الموظف بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateEmployeeDto dto)
    {
        await _employeeService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث بيانات الموظف بنجاح", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الموظف بنجاح", "تم الحذف"));
    }
}
