using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.ProductSerialNumbers;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductSerialNumbersController : ControllerBase
    {
        private readonly IProductSerialNumberService _serialNumberService;

        public ProductSerialNumbersController(IProductSerialNumberService serialNumberService)
        {
            _serialNumberService = serialNumberService;
        }

        /// <summary>
        /// جلب الأرقام التسلسلية المتاحة في المخزون لصنف محدد (للبيع)
        /// </summary>
        [HttpGet("by-medicine/{medicineId}")]
        public async Task<IActionResult> GetInStockByMedicineId(int medicineId)
        {
            var serials = await _serialNumberService.GetInStockByMedicineIdAsync(medicineId);
            return Ok(ApiResponse<IEnumerable<ProductSerialNumberDto>>.Succeeded(serials, "تم جلب الأرقام التسلسلية المتوفرة"));
        }

        /// <summary>
        /// الاستعلام عن حالة الضمان وبيانات البيع برقم السيريال
        /// </summary>
        [HttpGet("warranty-check/{serialNumber}")]
        public async Task<IActionResult> CheckWarranty(string serialNumber)
        {
            var result = await _serialNumberService.CheckWarrantyAsync(serialNumber);
            if (result == null)
            {
                return NotFound(ApiResponse<WarrantyCheckResultDto>.Failed("لم يتم العثور على جهاز مسجل بهذا الرقم التسلسلي"));
            }
            return Ok(ApiResponse<WarrantyCheckResultDto>.Succeeded(result, "تم جلب بيانات الضمان بنجاح"));
        }

        /// <summary>
        /// تسجيل أرقام تسلسلية جديدة (عند الاستلام أو التوريد)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterSerialNumbers([FromBody] RegisterSerialNumbersDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            var result = await _serialNumberService.RegisterSerialNumbersAsync(dto, userId);
            return Ok(ApiResponse<IEnumerable<ProductSerialNumberDto>>.Succeeded(result, "تم تسجيل الأرقام التسلسلية بنجاح"));
        }
    }
}
