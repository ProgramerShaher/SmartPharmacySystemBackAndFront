using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.DailyClosings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailyClosingsController : ControllerBase
{
    private readonly IDailyClosingService _closingService;
    public DailyClosingsController(IDailyClosingService closingService) => _closingService = closingService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _closingService.GetByIdAsync(id);
        return Ok(ApiResponse<DailyClosingDto>.Succeeded(result, "تم جلب الإغلاق بنجاح"));
    }

    [HttpGet("branch/{branchId}/date/{date}")]
    public async Task<IActionResult> GetByBranchDate(int branchId, DateTime date)
    {
        var result = await _closingService.GetByBranchDateAsync(branchId, date);
        return Ok(ApiResponse<DailyClosingDto?>.Succeeded(result, "تم جلب الإغلاق"));
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _closingService.GetByBranchIdAsync(branchId, from, to);
        return Ok(ApiResponse<IEnumerable<DailyClosingDto>>.Succeeded(result, "تم جلب إغلاقات الفرع"));
    }

    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(DateTime date)
    {
        var result = await _closingService.GetByDateAsync(date);
        return Ok(ApiResponse<IEnumerable<DailyClosingDto>>.Succeeded(result, "تم جلب إغلاقات التاريخ"));
    }

    [HttpGet("pending-approval")]
    public async Task<IActionResult> GetPendingApproval([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _closingService.GetPendingApprovalAsync(from, to);
        return Ok(ApiResponse<IEnumerable<DailyClosingDto>>.Succeeded(result, "تم جلب الإغلاقات المعلقة"));
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetApproved([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? branchId)
    {
        var result = await _closingService.GetApprovedAsync(from, to, branchId);
        return Ok(ApiResponse<IEnumerable<DailyClosingDto>>.Succeeded(result, "تم جلب الإغلاقات المعتمدة"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDailyClosingDto dto)
    {
        var result = await _closingService.CreateAsync(dto);
        return Ok(ApiResponse<DailyClosingDto>.Succeeded(result, "تم إنشاء الإغلاق بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(DailyClosingDto dto)
    {
        await _closingService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث الإغلاق", "تم التحديث"));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] int approvedByUserId)
    {
        var result = await _closingService.ApproveAsync(id, approvedByUserId);
        return Ok(ApiResponse<DailyClosingDto>.Succeeded(result, "تم اعتماد الإغلاق"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _closingService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الإغلاق بنجاح", "تم الحذف"));
    }
}
