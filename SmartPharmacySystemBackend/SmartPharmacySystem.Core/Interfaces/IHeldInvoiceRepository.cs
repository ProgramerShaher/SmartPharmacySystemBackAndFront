using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface IHeldInvoiceRepository
    {
        Task<HeldInvoice?> GetByIdAsync(int id);
        Task<IEnumerable<HeldInvoice>> GetAllActiveAsync(int? branchId = null);
        Task AddAsync(HeldInvoice entity);
        Task UpdateAsync(HeldInvoice entity);
        Task DeleteAsync(int id);
    }
}
