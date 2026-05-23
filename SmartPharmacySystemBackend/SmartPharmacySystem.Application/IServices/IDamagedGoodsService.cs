using SmartPharmacySystem.Application.DTOs.DamagedGoods;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IDamagedGoodsService
{
    Task<DamagedGoodsRecordDto> GetByIdAsync(int id);
    Task<DamagedGoodsRecordDto> GetByCodeAsync(string damageCode);
    Task<IEnumerable<DamagedGoodsRecordDto>> GetAllAsync(int? warehouseId = null, int? medicineId = null, DamageType? damageType = null, RecordStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<DamagedGoodsRecordDto>> GetPendingApprovalAsync();
    Task<IEnumerable<DamagedGoodsRecordDto>> GetApprovedAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<DamagedGoodsRecordDto> CreateAsync(CreateDamagedGoodsRecordDto dto);
    Task UpdateAsync(DamagedGoodsRecordDto dto);
    Task<DamagedGoodsRecordDto> ApproveAsync(int id, int approvedByUserId);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalDamageValueAsync(DateTime dateFrom, DateTime dateTo);
    Task<int> GetCountByStatusAsync(RecordStatus status);
    Task<bool> CodeExistsAsync(string damageCode, int? excludeId = null);
}
