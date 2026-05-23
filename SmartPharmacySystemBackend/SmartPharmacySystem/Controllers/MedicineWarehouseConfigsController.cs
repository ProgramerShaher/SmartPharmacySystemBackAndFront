using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicineWarehouseConfigsController : ControllerBase
{
    private readonly IMedicineWarehouseConfigService _configService;
    public MedicineWarehouseConfigsController(IMedicineWarehouseConfigService configService) => _configService = configService;

    [HttpGet("{warehouseId}/{medicineId}")]
    public async Task<IActionResult> GetById(int warehouseId, int medicineId)
    {
        var result = await _configService.GetByIdAsync(warehouseId, medicineId);
        return Ok(ApiResponse<MedicineWarehouseConfigDto>.Succeeded(result, "تم جلب الإعداد بنجاح"));
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        var result = await _configService.GetByWarehouseIdAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<MedicineWarehouseConfigDto>>.Succeeded(result, "تم جلب إعدادات المخزن"));
    }

    [HttpGet("medicine/{medicineId}")]
    public async Task<IActionResult> GetByMedicine(int medicineId)
    {
        var result = await _configService.GetByMedicineIdAsync(medicineId);
        return Ok(ApiResponse<IEnumerable<MedicineWarehouseConfigDto>>.Succeeded(result, "تم جلب إعدادات الدواء"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicineWarehouseConfigDto dto)
    {
        var result = await _configService.CreateAsync(dto);
        return Ok(ApiResponse<MedicineWarehouseConfigDto>.Succeeded(result, "تم إضافة الإعداد بنجاح"));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMedicineWarehouseConfigDto dto)
    {
        await _configService.UpdateAsync(dto);
        return Ok(ApiResponse<string>.Succeeded("تم تحديث الإعداد بنجاح", "تم التحديث"));
    }

    [HttpDelete("{warehouseId}/{medicineId}")]
    public async Task<IActionResult> Delete(int warehouseId, int medicineId)
    {
        await _configService.DeleteAsync(warehouseId, medicineId);
        return Ok(ApiResponse<string>.Succeeded("تم حذف الإعداد بنجاح", "تم الحذف"));
    }
}
