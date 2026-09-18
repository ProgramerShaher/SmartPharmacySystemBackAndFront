using SmartPharmacySystem.Application.DTOs.HeldInvoices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices
{
    public interface IHeldInvoiceService
    {
        Task<HeldInvoiceDto> HoldAsync(CreateHeldInvoiceDto dto, int userId, int? branchId);
        Task<IEnumerable<HeldInvoiceDto>> GetHeldInvoicesAsync(int? branchId);
        Task<HeldInvoiceDto?> GetByIdAsync(int id);
        Task<bool> ResumeAndDeleteAsync(int id);
    }
}
