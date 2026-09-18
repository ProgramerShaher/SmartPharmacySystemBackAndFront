using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface IProductVariantRepository
    {
        Task<ProductVariant?> GetByIdAsync(int id);
        Task<IEnumerable<ProductVariant>> GetByMedicineIdAsync(int medicineId);
        Task<ProductVariant?> GetByBarcodeOrSkuAsync(string code);
        Task AddAsync(ProductVariant entity);
        Task UpdateAsync(ProductVariant entity);
        Task DeleteAsync(int id);
    }
}
