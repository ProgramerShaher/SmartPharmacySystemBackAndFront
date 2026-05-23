using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Branches;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    public BranchesController(IBranchService branchService) => _branchService = branchService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] BranchType? type)
    {
        var result = await _branchService.GetAllAsync(search, isActive, type);
        return Ok(ApiResponse<IEnumerable<BranchDto>>.Succeeded(result, "تم جلب الفروع بنجاح"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _branchService.GetByIdAsync(id);
        return Ok(ApiResponse<BranchDto>.Succeeded(result, "تم جلب الفرع بنجاح"));
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _branchService.GetByCodeAsync(code);
        return Ok(ApiResponse<BranchDto>.Succeeded(result, "تم جلب الفرع بنجاح"));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _branchService.GetActiveBranchesAsync();
        return Ok(ApiResponse<IEnumerable<BranchDto>>.Succeeded(result, "تم جلب الفروع النشطة"));
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(BranchType type)
    {
        var result = await _branchService.GetByTypeAsync(type);
        return Ok(ApiResponse<IEnumerable<BranchDto>>.Succeeded(result, "تم جلب الفروع حسب النوع"));
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _branchService.GetBranchCountAsync();
        return Ok(ApiResponse<int>.Succeeded(count, "تم جلب عدد الفروع"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBranchDto dto)
    {
        var result = await _branchService.CreateAsync(dto);
        return Ok(ApiResponse<BranchDto>.Succeeded(result, "تم إضافة الفرع بنجاح"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateBranchDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<string>.Failed("معرف الفرع غير متطابق"));
        await _branchService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث الفرع بنجاح", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _branchService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الفرع بنجاح", "تم الحذف"));
    }
}
