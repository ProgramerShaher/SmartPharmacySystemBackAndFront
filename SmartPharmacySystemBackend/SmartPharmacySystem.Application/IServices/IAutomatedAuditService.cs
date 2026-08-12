using SmartPharmacySystem.Application.DTOs.Inventory;
using SmartPharmacySystem.Application.DTOs.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices
{
    public interface IAutomatedAuditService
    {
        Task<PagedResult<AutomatedAuditHeaderDto>> GetAllAsync(int page = 1, int pageSize = 10, int? warehouseId = null);
        Task<AutomatedAuditHeaderDto> GetByIdAsync(int id);
        Task<AutomatedAuditHeaderDto> GenerateAuditAsync(GenerateAuditRequestDto request);
        Task<AuditChartDataDto> GetAuditChartsAsync(int id);
        Task DeleteAsync(int id);
    }
}
