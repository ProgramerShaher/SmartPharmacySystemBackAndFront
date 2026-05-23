using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Warehouses;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    public WarehousesController(IWarehouseService warehouseService) => _warehouseService = warehouseService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId, [FromQuery] WarehouseType? type)
    {
        var result = await _warehouseService.GetAllAsync(branchId, type);
        return Ok(ApiResponse<IEnumerable<WarehouseDto>>.Succeeded(result, "تم جلب المخازن بنجاح"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _warehouseService.GetByIdAsync(id);
        return Ok(ApiResponse<WarehouseDto>.Succeeded(result, "تم جلب المخزن بنجاح"));
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranchId(int branchId)
    {
        var result = await _warehouseService.GetByBranchIdAsync(branchId);
        return Ok(ApiResponse<IEnumerable<WarehouseDto>>.Succeeded(result, "تم جلب مخازن الفرع"));
    }

    [HttpGet("branch/{branchId}/type/{type}")]
    public async Task<IActionResult> GetByBranchAndType(int branchId, WarehouseType type)
    {
        var result = await _warehouseService.GetByBranchAndTypeAsync(branchId, type);
        if (result == null) return NotFound(ApiResponse<string>.Failed("المخزن غير موجود"));
        return Ok(ApiResponse<WarehouseDto>.Succeeded(result, "تم جلب المخزن بنجاح"));
    }

    [HttpGet("count/{branchId}")]
    public async Task<IActionResult> GetCount(int branchId)
    {
        var count = await _warehouseService.GetWarehouseCountAsync(branchId);
        return Ok(ApiResponse<int>.Succeeded(count, "تم جلب عدد المخازن"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWarehouseDto dto)
    {
        var result = await _warehouseService.CreateAsync(dto);
        return Ok(ApiResponse<WarehouseDto>.Succeeded(result, "تم إضافة المخزن بنجاح"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateWarehouseDto dto)
    {
        await _warehouseService.UpdateAsync(dto, id);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث المخزن بنجاح", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _warehouseService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف المخزن بنجاح", "تم الحذف"));
    }
}
