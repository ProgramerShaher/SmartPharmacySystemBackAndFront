using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.ProductVariants;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _variantService;

        public ProductVariantsController(IProductVariantService variantService)
        {
            _variantService = variantService;
        }

        /// <summary>
        /// جلب جميع تنويعات الصنف (المقاسات والألوان) للصنف المحدد
        /// </summary>
        [HttpGet("by-medicine/{medicineId}")]
        public async Task<IActionResult> GetByMedicineId(int medicineId)
        {
            var variants = await _variantService.GetByMedicineIdAsync(medicineId);
            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.Succeeded(variants, "تم جلب تنويعات الصنف بنجاح"));
        }

        /// <summary>
        /// جلب بيانات تنويعة محددة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var variant = await _variantService.GetByIdAsync(id);
            if (variant == null)
            {
                return NotFound(ApiResponse<ProductVariantDto>.Failed("تنويعة الصنف غير موجودة"));
            }
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(variant, "تم جلب تنويعة الصنف"));
        }

        /// <summary>
        /// جلب بيانات تنويعة بالباركود أو كود SKU
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var variant = await _variantService.GetByBarcodeOrSkuAsync(code);
            if (variant == null)
            {
                return NotFound(ApiResponse<ProductVariantDto>.Failed("لم يتم العثور على صنف مطابق للرمز"));
            }
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(variant, "تم العثور على تنويعة الصنف"));
        }

        /// <summary>
        /// إضافة تنويعة جديدة لصنف (مقاس أو لون)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantDto dto)
        {
            var created = await _variantService.CreateAsync(dto);
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(created, "تم إضافة تنويعة الصنف بنجاح"));
        }

        /// <summary>
        /// تعديل بيانات تنويعة صنف
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductVariantDto dto)
        {
            var updated = await _variantService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductVariantDto>.Succeeded(updated, "تم تحديث تنويعة الصنف بنجاح"));
        }

        /// <summary>
        /// حذف تنويعة صنف
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _variantService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Failed("تنويعة الصنف غير موجودة"));
            }
            return Ok(ApiResponse<bool>.Succeeded(true, "تم حذف تنويعة الصنف بنجاح"));
        }
    }
}
