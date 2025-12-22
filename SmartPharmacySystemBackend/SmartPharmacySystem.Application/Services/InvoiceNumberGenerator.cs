using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Application.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public InvoiceNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateSaleInvoiceNumberAsync()
    {
        return await GenerateNumberAsync("SI");
    }

    public async Task<string> GeneratePurchaseInvoiceNumberAsync()
    {
        return await GenerateNumberAsync("PI");
    }

    private async Task<string> GenerateNumberAsync(string prefix)
    {
        var year = DateTime.UtcNow.Year;
        int nextNumber;

        // «Õ’· ⁄·Ï «·‹ sequence »œÊ‰ › Õ Transaction ÃœÌœ
        var sequence = await _context.InvoiceSequences
            .FirstOrDefaultAsync(s => s.Prefix == prefix && s.Year == year);

        if (sequence == null)
        {
            sequence = new InvoiceNumberSequence
            {
                Prefix = prefix,
                Year = year,
                LastNumber = 1
            };
            _context.InvoiceSequences.Add(sequence);
            nextNumber = 1;
        }
        else
        {
            sequence.LastNumber++;
            _context.InvoiceSequences.Update(sequence);
            nextNumber = sequence.LastNumber;
        }

        // «Õ›Ÿ «· €ÌÌ—«  œ«Œ· Transaction «·Õ«·Ì ›Ì CreateAsync
        await _context.SaveChangesAsync();

        return $"{prefix}-{year}-{nextNumber:D6}";
    }

}
