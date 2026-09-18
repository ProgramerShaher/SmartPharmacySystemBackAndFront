using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface IProductSerialNumberRepository
    {
        Task<ProductSerialNumber?> GetByIdAsync(int id);
        Task<ProductSerialNumber?> GetBySerialNumberAsync(string serialNumber);
        Task<IEnumerable<ProductSerialNumber>> GetInStockByMedicineIdAsync(int medicineId);
        Task<IEnumerable<ProductSerialNumber>> GetBySaleDetailIdAsync(int saleDetailId);
        Task AddRangeAsync(IEnumerable<ProductSerialNumber> entities);
        Task UpdateAsync(ProductSerialNumber entity);
        Task<bool> ExistsAsync(string serialNumber);
    }
}
