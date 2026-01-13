using SmartPharmacySystem.Application.DTOs.SupplierPayments;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface ISupplierPaymentService
    {
        Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto dto, int userId);
        Task CancelAsync(int id, int userId); // Logical Delete / Reversal
        Task<SupplierStatementDto> GetStatementAsync(int supplierId);
        Task<IEnumerable<SupplierPaymentDto>> GetRecentPaymentsAsync(int supplierId = 0);
    }
}
