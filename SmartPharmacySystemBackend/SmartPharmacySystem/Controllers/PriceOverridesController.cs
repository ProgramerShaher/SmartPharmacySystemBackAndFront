using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.PriceOverrides;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceOverridesController : ControllerBase
{
    private readonly IPriceOverrideService _priceService;
    public PriceOverridesController(IPriceOverrideService priceService) => _priceService = priceService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _priceService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PriceOverrideDto>>.Succeeded(result, "تم جلب تجاوزات الأسعار"));
    }

    [HttpGet("medicine/{medicineId}")]
    public async Task<IActionResult> GetByMedicine(int medicineId)
    {
        var result = await _priceService.GetByMedicineIdAsync(medicineId);
        return Ok(ApiResponse<IEnumerable<PriceOverrideDto>>.Succeeded(result, "تم جلب تجاوزات الدواء"));
    }
}
