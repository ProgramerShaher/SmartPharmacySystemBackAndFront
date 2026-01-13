using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IStockMovementService
    {
        /// <summary>
        /// Processes and creates stock movements from an approved reference document.
        /// </summary>
        Task ProcessDocumentMovementsAsync(int referenceId, ReferenceType type);

        /// <summary>
        /// Cancels movements by creating reverse movements.
        /// </summary>
        Task CancelDocumentMovementsAsync(int referenceId, ReferenceType type);

        /// <summary>
        /// Creates a manual movement (Damage or Adjustment) with required reason and approval.
        /// </summary>
        Task CreateManualMovementAsync(CreateManualMovementDto dto);

        Task<StockMovementDto> GetByIdAsync(int id);
        Task<PagedResult<StockMovementDto>> SearchAsync(BaseQueryDto query);
        Task<IEnumerable<StockCardDto>> GetStockCardAsync(int medicineId, int? batchId = null);
        Task<int> GetCurrentBalanceAsync(int medicineId, int? batchId = null);
    }
}
