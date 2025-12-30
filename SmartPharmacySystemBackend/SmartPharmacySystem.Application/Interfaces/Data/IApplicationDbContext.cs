using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Interfaces.Data;

public interface IApplicationDbContext
{
    DbSet<Alert> Alerts { get; }
    DbSet<MedicineBatch> MedicineBatches { get; }
    DbSet<FinancialTransaction> FinancialTransactions { get; }
    DbSet<PharmacyAccount> PharmacyAccounts { get; }
    DbSet<InvoiceNumberSequence> InvoiceSequences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
