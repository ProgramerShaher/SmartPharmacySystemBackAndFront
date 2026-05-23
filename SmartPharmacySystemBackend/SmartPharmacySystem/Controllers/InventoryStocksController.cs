using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.InventoryStocks;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryStocksController : ControllerBase
{
    private readonly IInventoryStockService _stockService;
    public InventoryStocksController(IInventoryStockService stockService) => _stockService = stockService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _stockService.GetByIdAsync(id);
        return Ok(ApiResponse<InventoryStockDto>.Succeeded(result, "تم جلب الرصيد بنجاح"));
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        var result = await _stockService.GetByWarehouseIdAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "تم جلب أرصدة المخزن"));
    }

    [HttpGet("medicine/{medicineId}")]
    public async Task<IActionResult> GetByMedicine(int medicineId)
    {
        var result = await _stockService.GetByMedicineIdAsync(medicineId);
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "تم جلب أرصدة الدواء"));
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring([FromQuery] int days = 30)
    {
        var result = await _stockService.GetExpiringSoonAsync(days);
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "تم جلب الأدوية المنتهية قريباً"));
    }

    [HttpGet("expired")]
    public async Task<IActionResult> GetExpired()
    {
        var result = await _stockService.GetExpiredAsync();
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "تم جلب الأدوية المنتهية"));
    }

    [HttpGet("below-reorder")]
    public async Task<IActionResult> GetBelowReorder()
    {
        var result = await _stockService.GetBelowReorderLevelAsync();
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "تم جلب الأدوية أقل من حد الطلب"));
    }

    [HttpGet("quantity/{warehouseId}/{medicineId}")]
    public async Task<IActionResult> GetTotalQuantity(int warehouseId, int medicineId)
    {
        var quantity = await _stockService.GetTotalQuantityAsync(warehouseId, medicineId);
        return Ok(ApiResponse<int>.Succeeded(quantity, "تم جلب الكمية الإجمالية"));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term, [FromQuery] int? warehouseId, [FromQuery] int? medicineId)
    {
        var result = await _stockService.SearchAsync(term, warehouseId, medicineId);
        return Ok(ApiResponse<IEnumerable<InventoryStockDto>>.Succeeded(result, "نتائج البحث"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryStockDto dto)
    {
        var result = await _stockService.CreateAsync(dto);
        return Ok(ApiResponse<InventoryStockDto>.Succeeded(result, "تم إضافة الرصيد بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateInventoryStockDto dto)
    {
        await _stockService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث الرصيد", "تم التحديث"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _stockService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الرصيد بنجاح", "تم الحذف"));
    }
}
