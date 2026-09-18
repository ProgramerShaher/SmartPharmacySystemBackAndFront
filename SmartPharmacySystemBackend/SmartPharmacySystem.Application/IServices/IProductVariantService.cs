using SmartPharmacySystem.Application.DTOs.ProductVariants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantDto>> GetByMedicineIdAsync(int medicineId);
        Task<ProductVariantDto?> GetByIdAsync(int id);
        Task<ProductVariantDto?> GetByBarcodeOrSkuAsync(string code);
        Task<ProductVariantDto> CreateAsync(CreateProductVariantDto dto);
        Task<ProductVariantDto> UpdateAsync(int id, UpdateProductVariantDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
