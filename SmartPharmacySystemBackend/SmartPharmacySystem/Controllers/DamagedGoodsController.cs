using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.DamagedGoods;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DamagedGoodsController : ControllerBase
{
    private readonly IDamagedGoodsService _damagedService;
    public DamagedGoodsController(IDamagedGoodsService damagedService) => _damagedService = damagedService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _damagedService.GetByIdAsync(id);
        return Ok(ApiResponse<DamagedGoodsRecordDto>.Succeeded(result, "تم جلب التالفة بنجاح"));
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _damagedService.GetByCodeAsync(code);
        return Ok(ApiResponse<DamagedGoodsRecordDto>.Succeeded(result, "تم جلب التالفة بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? warehouseId, [FromQuery] int? medicineId, [FromQuery] DamageType? damageType, [FromQuery] RecordStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _damagedService.GetAllAsync(warehouseId, medicineId, damageType, status, from, to);
        return Ok(ApiResponse<IEnumerable<DamagedGoodsRecordDto>>.Succeeded(result, "تم جلب التالفات"));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _damagedService.GetPendingApprovalAsync();
        return Ok(ApiResponse<IEnumerable<DamagedGoodsRecordDto>>.Succeeded(result, "تم جلب التالفات المعلقة"));
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetApproved([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _damagedService.GetApprovedAsync(from, to);
        return Ok(ApiResponse<IEnumerable<DamagedGoodsRecordDto>>.Succeeded(result, "تم جلب التالفات المعتمدة"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDamagedGoodsRecordDto dto)
    {
        var result = await _damagedService.CreateAsync(dto);
        return Ok(ApiResponse<DamagedGoodsRecordDto>.Succeeded(result, "تم تسجيل التالفة بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(DamagedGoodsRecordDto dto)
    {
        await _damagedService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث التالفة", "تم التحديث"));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] int approvedByUserId)
    {
        var result = await _damagedService.ApproveAsync(id, approvedByUserId);
        return Ok(ApiResponse<DamagedGoodsRecordDto>.Succeeded(result, "تم اعتماد التالفة"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _damagedService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف التالفة بنجاح", "تم الحذف"));
    }
}
