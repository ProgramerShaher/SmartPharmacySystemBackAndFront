using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IDamagedGoodsRepository
{
    Task<DamagedGoodsRecord?> GetByIdAsync(int id);
    Task<DamagedGoodsRecord?> GetByCodeAsync(string damageCode);
    Task<IEnumerable<DamagedGoodsRecord>> GetAllAsync(int? warehouseId = null, int? medicineId = null, DamageType? damageType = null, RecordStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<DamagedGoodsRecord>> GetPendingApprovalAsync();
    Task<IEnumerable<DamagedGoodsRecord>> GetApprovedAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<DamagedGoodsRecord> AddAsync(DamagedGoodsRecord record);
    Task UpdateAsync(DamagedGoodsRecord record);
    Task DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string damageCode, int? excludeId = null);
    Task<decimal> GetTotalDamageValueAsync(DateTime dateFrom, DateTime dateTo);
    Task<int> GetCountByStatusAsync(RecordStatus status);
}
