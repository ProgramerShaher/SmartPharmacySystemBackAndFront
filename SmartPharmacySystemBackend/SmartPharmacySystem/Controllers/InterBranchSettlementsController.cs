using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.InterBranchSettlements;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterBranchSettlementsController : ControllerBase
{
    private readonly IInterBranchSettlementService _settlementService;
    public InterBranchSettlementsController(IInterBranchSettlementService settlementService) => _settlementService = settlementService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _settlementService.GetByIdAsync(id);
        return Ok(ApiResponse<InterBranchSettlementDto>.Succeeded(result, "تم جلب التسوية بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? fromBranchId, [FromQuery] int? toBranchId, [FromQuery] SettlementStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _settlementService.GetAllAsync(fromBranchId, toBranchId, status, from, to);
        return Ok(ApiResponse<IEnumerable<InterBranchSettlementDto>>.Succeeded(result, "تم جلب التسويات"));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _settlementService.GetPendingAsync();
        return Ok(ApiResponse<IEnumerable<InterBranchSettlementDto>>.Succeeded(result, "تم جلب التسويات المعلقة"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInterBranchSettlementDto dto)
    {
        var result = await _settlementService.CreateAsync(dto);
        return Ok(ApiResponse<InterBranchSettlementDto>.Succeeded(result, "تم إنشاء التسوية بنجاح"));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] int approvedByUserId)
    {
        var result = await _settlementService.ApproveAsync(id, approvedByUserId);
        return Ok(ApiResponse<InterBranchSettlementDto>.Succeeded(result, "تم اعتماد التسوية"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(InterBranchSettlementDto dto)
    {
        await _settlementService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث التسوية", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _settlementService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف التسوية بنجاح", "تم الحذف"));
    }
}
