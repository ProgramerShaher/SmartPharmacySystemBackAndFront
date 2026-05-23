using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Departments;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    public DepartmentsController(IDepartmentService departmentService) => _departmentService = departmentService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        return Ok(ApiResponse<DepartmentDto>.Succeeded(result, "تم جلب القسم بنجاح"));
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _departmentService.GetByNameAsync(name);
        return Ok(ApiResponse<DepartmentDto>.Succeeded(result, "تم جلب القسم بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var result = await _departmentService.GetAllAsync(search);
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Succeeded(result, "تم جلب الأقسام بنجاح"));
    }

    [HttpGet("name-exists")]
    public async Task<IActionResult> NameExists([FromQuery] string name, [FromQuery] int? excludeId)
    {
        var exists = await _departmentService.NameExistsAsync(name, excludeId);
        return Ok(ApiResponse<bool>.Succeeded(exists, "تم التحقق من اسم القسم"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var result = await _departmentService.CreateAsync(dto);
        return Ok(ApiResponse<DepartmentDto>.Succeeded(result, "تم إضافة القسم بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateDepartmentDto dto)
    {
        await _departmentService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث القسم بنجاح", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف القسم بنجاح", "تم الحذف"));
    }
}
