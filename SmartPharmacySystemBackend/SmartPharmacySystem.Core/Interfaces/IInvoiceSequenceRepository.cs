using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IInvoiceSequenceRepository
{
    Task<InvoiceNumberSequence?> GetSequenceAsync(string prefix, int year);
    Task AddAsync(InvoiceNumberSequence sequence);
    Task UpdateAsync(InvoiceNumberSequence sequence);
}
