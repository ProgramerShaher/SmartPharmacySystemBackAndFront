using SmartPharmacySystem.Application.DTOs.InvoiceSequences;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IInvoiceSequenceService
{
    Task<InvoiceSequenceDto> GetNextNumberAsync(string prefix, int year);
}
