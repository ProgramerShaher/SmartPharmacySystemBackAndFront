using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface ISupplierPaymentRepository
    {
        Task<SupplierPayment?> GetByIdAsync(int id);
        Task<IEnumerable<SupplierPayment>> GetAllAsync(); // Maybe add filters
        Task AddAsync(SupplierPayment entity);
        Task UpdateAsync(SupplierPayment entity);
        Task<IEnumerable<SupplierPayment>> GetBySupplierIdAsync(int supplierId);
    }
}
