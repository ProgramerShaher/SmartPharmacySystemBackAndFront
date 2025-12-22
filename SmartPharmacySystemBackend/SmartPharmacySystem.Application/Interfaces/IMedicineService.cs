using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IMedicineService
    {
        Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto);
        Task UpdateMedicineAsync(int id, UpdateMedicineDto dto);
        Task DeleteMedicineAsync(int id);
        Task<MedicineDto> GetMedicineByIdAsync(int id);
        Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync();
        Task<PagedResult<MedicineDto>> SearchAsync(MedicineQueryDto query);
    }
}
