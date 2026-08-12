using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.StockCounts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockCountsController : ControllerBase
{
    private readonly IStockCountService _countService;
    public StockCountsController(IStockCountService countService) => _countService = countService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHeaderById(int id)
    {
        var result = await _countService.GetHeaderByIdAsync(id);
        return Ok(ApiResponse<StockCountHeaderDto>.Succeeded(result, "تم جلب الجرد بنجاح"));
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _countService.GetByCodeAsync(code);
        return Ok(ApiResponse<StockCountHeaderDto>.Succeeded(result, "تم جلب الجرد بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? warehouseId, [FromQuery] StockCountStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _countService.GetAllHeadersAsync(warehouseId, status, from, to);
        return Ok(ApiResponse<IEnumerable<StockCountHeaderDto>>.Succeeded(result, "تم جلب جرد المخزون"));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _countService.GetPendingApprovalAsync();
        return Ok(ApiResponse<IEnumerable<StockCountHeaderDto>>.Succeeded(result, "تم جلب الجرد المعلق"));
    }

    [HttpGet("{headerId}/items")]
    public async Task<IActionResult> GetItems(int headerId)
    {
        var result = await _countService.GetItemsByHeaderIdAsync(headerId);
        return Ok(ApiResponse<IEnumerable<StockCountItemDto>>.Succeeded(result, "تم جلب عناصر الجرد"));
    }

    [HttpPost]
    public async Task<IActionResult> CreateHeader(CreateStockCountHeaderDto dto)
    {
        var result = await _countService.CreateHeaderAsync(dto);
        return Ok(ApiResponse<StockCountHeaderDto>.Succeeded(result, "تم إنشاء الجرد بنجاح"));
    }

    [HttpPost("{headerId}/items")]
    public async Task<IActionResult> AddItem(int headerId, CreateStockCountItemDto dto)
    {
        var result = await _countService.AddItemAsync(headerId, dto);
        return Ok(ApiResponse<StockCountItemDto>.Succeeded(result, "تم إضافة عنصر للجرد"));
    }

    [HttpPut("{headerId}/items")]
    public async Task<IActionResult> UpdateItem(int headerId, UpdateStockCountItemDto dto)
    {
        await _countService.UpdateItemAsync(headerId, dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث عنصر الجرد", "تم التحديث"));
    }

    [HttpPut("{headerId}/submit")]
    public async Task<IActionResult> Submit(int headerId)
    {
        var result = await _countService.SubmitForApprovalAsync(headerId);
        return Ok(ApiResponse<StockCountHeaderDto>.Succeeded(result, "تم تقديم الجرد للمراجعة"));
    }

    [HttpPut("{headerId}/approve")]
    public async Task<IActionResult> Approve(int headerId, [FromQuery] int approvedByUserId)
    {
        var result = await _countService.ApproveAsync(headerId, approvedByUserId);
        return Ok(ApiResponse<StockCountHeaderDto>.Succeeded(result, "تم اعتماد الجرد"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHeader(int id)
    {
        await _countService.DeleteHeaderAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الجرد بنجاح", "تم الحذف"));
    }

    [HttpDelete("{headerId}/items/{medicineId}/{batchNumber}")]
    public async Task<IActionResult> DeleteItem(int headerId, int medicineId, string batchNumber)
    {
        await _countService.DeleteItemAsync(headerId, medicineId, batchNumber);
        return Ok(ApiResponse<string>.Succeeded("تم حذف عنصر الجرد", "تم الحذف"));
    }

    // ==========================================
    // Schedules
    // ==========================================

    [HttpGet("schedules")]
    public async Task<IActionResult> GetAllSchedules([FromQuery] int? warehouseId)
    {
        var result = await _countService.GetAllSchedulesAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<StockCountScheduleDto>>.Succeeded(result, "تم جلب الجداول بنجاح"));
    }

    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule(CreateStockCountScheduleDto dto)
    {
        var result = await _countService.CreateScheduleAsync(dto);
        return Ok(ApiResponse<StockCountScheduleDto>.Succeeded(result, "تم إنشاء الجدولة بنجاح"));
    }

    [HttpPut("schedules/{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, UpdateStockCountScheduleDto dto)
    {
        await _countService.UpdateScheduleAsync(id, dto);
        return Ok(ApiResponse<string>.Succeeded(null,"تم تحديث الجدولة بنجاح"));
    }

    [HttpDelete("schedules/{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        await _countService.DeleteScheduleAsync(id);
        return Ok(ApiResponse<string>.Succeeded( null, "تم حذف الجدولة بنجاح"));
    }
}
