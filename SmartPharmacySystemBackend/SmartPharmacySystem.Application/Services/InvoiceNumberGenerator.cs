using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Services;

public class InvoiceNumberGenerator(IApplicationDbContext context) : IInvoiceNumberGenerator
{
    public async Task<string> GenerateSaleInvoiceNumberAsync()
    {
        return await GenerateInvoiceNumberAsync("Sale", DateTime.UtcNow);
    }

    public async Task<string> GeneratePurchaseInvoiceNumberAsync()
    {
        return await GenerateInvoiceNumberAsync("Purchase", DateTime.UtcNow);
    }

    private async Task<string> GenerateInvoiceNumberAsync(string type, DateTime date)
    {
        string prefix = type switch
        {
            "Sale" => "INV",
            "Purchase" => "PUR",
            _ => "GEN"
        };

        var year = date.Year;
        var sequence = await context.InvoiceSequences
            .FirstOrDefaultAsync(s => s.Prefix == prefix && s.Year == year);

        if (sequence == null)
        {
            sequence = new InvoiceNumberSequence
            {
                Prefix = prefix,
                Year = year,
                LastNumber = 0
            };
            await context.InvoiceSequences.AddAsync(sequence);
        }

        sequence.LastNumber++;
        await context.SaveChangesAsync(CancellationToken.None);

        return $"{prefix}-{year}-{sequence.LastNumber:D6}";
    }
}
