using SmartPharmacySystem.Application.DTOs.ProductSerialNumbers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices
{
    public interface IProductSerialNumberService
    {
        Task<IEnumerable<ProductSerialNumberDto>> GetInStockByMedicineIdAsync(int medicineId);
        Task<WarrantyCheckResultDto?> CheckWarrantyAsync(string serialNumber);
        Task<IEnumerable<ProductSerialNumberDto>> RegisterSerialNumbersAsync(RegisterSerialNumbersDto dto, int userId);
        Task<bool> AssignToSaleAsync(int saleDetailId, IEnumerable<string> serialNumbers, int? customerId, int warrantyMonths);
    }
}
