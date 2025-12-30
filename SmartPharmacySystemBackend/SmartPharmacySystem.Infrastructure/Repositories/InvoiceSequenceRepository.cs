using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class InvoiceSequenceRepository(ApplicationDbContext context) : IInvoiceSequenceRepository
{
    public async Task<InvoiceNumberSequence?> GetSequenceAsync(string prefix, int year)
    {
        return await context.InvoiceSequences
            .FirstOrDefaultAsync(s => s.Prefix == prefix && s.Year == year);
    }

    public async Task AddAsync(InvoiceNumberSequence sequence)
    {
        await context.InvoiceSequences.AddAsync(sequence);
    }

    public async Task UpdateAsync(InvoiceNumberSequence sequence)
    {
        context.InvoiceSequences.Update(sequence);
        await Task.CompletedTask;
    }
}
