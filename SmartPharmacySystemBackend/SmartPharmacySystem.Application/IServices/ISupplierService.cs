using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Suppliers;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
        Task UpdateAsync(int id, UpdateSupplierDto dto);
        Task DeleteAsync(int id);
        Task<SupplierDto?> GetByIdAsync(int id);
        Task<IEnumerable<SupplierDto>> GetAllAsync();
        Task<PagedResult<SupplierDto>> SearchAsync(SupplierQueryDto query);
    }
}
