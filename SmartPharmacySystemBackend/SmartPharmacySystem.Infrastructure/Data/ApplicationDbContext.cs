using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Infrastructure.Data;

/// <summary>
/// Represents the database context for the application.
/// This class manages the database connections and entity configurations.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<MedicineBatch> MedicineBatches { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
    public DbSet<SaleInvoice> SaleInvoices { get; set; }
    public DbSet<SaleInvoiceDetail> SaleInvoiceDetails { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<SalesReturn> SalesReturns { get; set; }
    public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
    public DbSet<InvoiceNumberSequence> InvoiceSequences { get; set; }

    // Financial System DbSets
    public DbSet<PharmacyAccount> PharmacyAccounts { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<FinancialInvoice> FinancialInvoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Financial System Configurations
        modelBuilder.Entity<PharmacyAccount>(entity =>
        {
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            // Seed initial account
            entity.HasData(new PharmacyAccount { Id = 1, Balance = 0, LastUpdated = new DateTime(2025, 1, 1) });
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();
        });

        modelBuilder.Entity<FinancialInvoice>(entity =>
        {
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();
        });

        // Configure relationships

        // Medicine - Category (optional)
        modelBuilder.Entity<Medicine>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Medicines)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // MedicineBatch - Medicine
        modelBuilder.Entity<MedicineBatch>()
            .HasOne(mb => mb.Medicine)
            .WithMany(m => m.MedicineBatches)
            .HasForeignKey(mb => mb.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on CompanyBatchNumber
        modelBuilder.Entity<MedicineBatch>()
            .HasIndex(mb => mb.CompanyBatchNumber)
            .IsUnique();

        // PurchaseInvoice - Supplier
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Supplier)
            .WithMany(s => s.PurchaseInvoices)
            .HasForeignKey(pi => pi.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasIndex(pi => pi.PurchaseInvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<PurchaseInvoice>()
            .Property(pi => pi.PurchaseInvoiceNumber)
            .HasMaxLength(20);

        // PurchaseInvoiceDetail - PurchaseInvoice
        modelBuilder.Entity<PurchaseInvoiceDetail>()
            .HasOne(pid => pid.PurchaseInvoice)
            .WithMany(pi => pi.PurchaseInvoiceDetails)
            .HasForeignKey(pid => pid.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseInvoiceDetail - Medicine
        modelBuilder.Entity<PurchaseInvoiceDetail>()
            .HasOne(pid => pid.Medicine)
            .WithMany()
            .HasForeignKey(pid => pid.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseInvoiceDetail - MedicineBatch
        modelBuilder.Entity<PurchaseInvoiceDetail>()
            .HasOne(pid => pid.Batch)
            .WithMany(mb => mb.PurchaseInvoiceDetails)
            .HasForeignKey(pid => pid.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SaleInvoice>()
            .HasIndex(si => si.SaleInvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<SaleInvoice>()
            .Property(si => si.SaleInvoiceNumber)
            .HasMaxLength(20);

        // SaleInvoiceDetail - SaleInvoice
        modelBuilder.Entity<SaleInvoiceDetail>()
            .HasOne(sid => sid.SaleInvoice)
            .WithMany(si => si.SaleInvoiceDetails)
            .HasForeignKey(sid => sid.SaleInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // SaleInvoiceDetail - Medicine
        modelBuilder.Entity<SaleInvoiceDetail>()
            .HasOne(sid => sid.Medicine)
            .WithMany()
            .HasForeignKey(sid => sid.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        // SaleInvoiceDetail - MedicineBatch
        modelBuilder.Entity<SaleInvoiceDetail>()
            .HasOne(sid => sid.Batch)
            .WithMany(mb => mb.SaleInvoiceDetails)
            .HasForeignKey(sid => sid.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // InventoryMovement - Medicine
        modelBuilder.Entity<InventoryMovement>()
            .HasOne(im => im.Medicine)
            .WithMany(m => m.InventoryMovements)
            .HasForeignKey(im => im.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        // InventoryMovement - MedicineBatch (optional)
        modelBuilder.Entity<InventoryMovement>()
            .HasOne(im => im.Batch)
            .WithMany(mb => mb.InventoryMovements)
            .HasForeignKey(im => im.BatchId)
            .OnDelete(DeleteBehavior.NoAction);

        // Alert - MedicineBatch
        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Batch)
            .WithMany(mb => mb.Alerts)
            .HasForeignKey(a => a.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Alert - Enum Configuration (store as integers)
        modelBuilder.Entity<Alert>()
            .Property(a => a.AlertType)
            .HasConversion<int>()
            .IsRequired();

        modelBuilder.Entity<Alert>()
            .Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();

        // SalesReturn - SaleInvoice
        modelBuilder.Entity<SalesReturn>()
            .HasOne(sr => sr.SaleInvoice)
            .WithMany(si => si.SalesReturns)
            .HasForeignKey(sr => sr.SaleInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // SalesReturnDetail - SalesReturn
        modelBuilder.Entity<SalesReturnDetail>()
            .HasOne(srd => srd.SalesReturn)
            .WithMany(sr => sr.SalesReturnDetails)
            .HasForeignKey(srd => srd.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        // SalesReturnDetail - Medicine
        modelBuilder.Entity<SalesReturnDetail>()
            .HasOne(srd => srd.Medicine)
            .WithMany()
            .HasForeignKey(srd => srd.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        // SalesReturnDetail - MedicineBatch
        modelBuilder.Entity<SalesReturnDetail>()
            .HasOne(srd => srd.Batch)
            .WithMany(mb => mb.SalesReturnDetails)
            .HasForeignKey(srd => srd.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseReturn - PurchaseInvoice
        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.PurchaseInvoice)
            .WithMany()
            .HasForeignKey(pr => pr.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseReturn - Supplier
        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.Supplier)
            .WithMany(s => s.PurchaseReturns)
            .HasForeignKey(pr => pr.SupplierId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseReturnDetail - PurchaseReturn
        modelBuilder.Entity<PurchaseReturnDetail>()
            .HasOne(prd => prd.PurchaseReturn)
            .WithMany(pr => pr.PurchaseReturnDetails)
            .HasForeignKey(prd => prd.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseReturnDetail - Medicine
        modelBuilder.Entity<PurchaseReturnDetail>()
            .HasOne(prd => prd.Medicine)
            .WithMany()
            .HasForeignKey(prd => prd.MedicineId)
            .OnDelete(DeleteBehavior.NoAction);

        // PurchaseReturnDetail - MedicineBatch
        modelBuilder.Entity<PurchaseReturnDetail>()
            .HasOne(prd => prd.Batch)
            .WithMany(mb => mb.PurchaseReturnDetails)
            .HasForeignKey(prd => prd.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure InvoiceNumberSequence
        modelBuilder.Entity<InvoiceNumberSequence>()
            .HasKey(isq => isq.Id);

        modelBuilder.Entity<InvoiceNumberSequence>()
            .HasIndex(isq => new { isq.Prefix, isq.Year })
            .IsUnique();
    }

    public override int SaveChanges()
    {
        HandleInvoiceNumberImmutability();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleInvoiceNumberImmutability();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleInvoiceNumberImmutability()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is SaleInvoice)
            {
                var property = entry.Property("SaleInvoiceNumber");
                if (property.IsModified)
                {
                    property.IsModified = false;
                }
            }
            else if (entry.Entity is PurchaseInvoice)
            {
                var property = entry.Property("PurchaseInvoiceNumber");
                if (property.IsModified)
                {
                    property.IsModified = false;
                }
            }
        }
    }
}