using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

using SmartPharmacySystem.Application.Interfaces.Data;

namespace SmartPharmacySystem.Infrastructure.Data;

/// <summary>
/// Represents the database context for the application.
/// This class manages the database connections and entity configurations.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Medicine> Medicines { get; set; } = null!;
    public DbSet<MedicineBatch> MedicineBatches { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; } = null!;
    public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; } = null!;
    public DbSet<SaleInvoice> SaleInvoices { get; set; } = null!;
    public DbSet<SaleInvoiceDetail> SaleInvoiceDetails { get; set; } = null!;
    public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;
    public DbSet<SalesReturn> SalesReturns { get; set; } = null!;
    public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; } = null!;
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; } = null!;
    public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = null!;
    public DbSet<InvoiceNumberSequence> InvoiceSequences { get; set; } = null!;
    public DbSet<PriceOverride> PriceOverrides { get; set; } = null!;
    public DbSet<PharmacyAccount> PharmacyAccounts { get; set; } = null!;
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; } = null!;
    public DbSet<CustomerReceipt> CustomerReceipts { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<SupplierPayment> SupplierPayments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Financial System Configurations
        modelBuilder.Entity<PharmacyAccount>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.HasIndex(e => e.Name).IsUnique();

            // Seed initial account
            entity.HasData(new PharmacyAccount
            {
                Id = 1,
                Name = "الخزينة الرئيسية",
                Balance = 0,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1),
                LastUpdated = new DateTime(2025, 1, 1)
            });
        });

        // SupplierPayment Configuration
        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PaymentDate);
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Balance).HasPrecision(18, 2).HasDefaultValue(0);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.HasIndex(e => e.Name); // Search optimization
        });

        // CustomerReceipt Configuration
        modelBuilder.Entity<CustomerReceipt>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Receipts)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.ReceiptDate);
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.ReferenceType).HasConversion<int>();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);

            // Relationship with PharmacyAccount
            entity.HasOne(ft => ft.Account)
                  .WithMany()
                  .HasForeignKey(ft => ft.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Composite index for efficient queries by reference
            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId });

            // Index for transaction date queries
            entity.HasIndex(e => e.TransactionDate);

            // Index for date range queries
            entity.HasIndex(e => e.CreatedAt);
        });

        // Expense - Category relationship
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<int>();

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Expenses)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Expense Categories
        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "رواتب", Description = "رواتب الموظفين والبدلات" },
            new ExpenseCategory { Id = 2, Name = "إيجار", Description = "إيجار مقر الصيدلية والمخازن" },
            new ExpenseCategory { Id = 3, Name = "كهرباء", Description = "فواتير الكهرباء" },
            new ExpenseCategory { Id = 4, Name = "مياه", Description = "فواتير المياه" },
            new ExpenseCategory { Id = 5, Name = "اتصالات وانترنت", Description = "فواتير الهاتف والاشتراكات" },
            new ExpenseCategory { Id = 6, Name = "قرطاسية وأدوات مكتبية", Description = "أدوات مكتبية ومطبوعات" },
            new ExpenseCategory { Id = 7, Name = "صيانة", Description = "صيانة المعدات والمباني" },
            new ExpenseCategory { Id = 8, Name = "نظافة", Description = "أدوات ومواد نظافة" },
            new ExpenseCategory { Id = 9, Name = "أخرى", Description = "مصاريف متنوعة" }
        );

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

        // Unique index on CompanyBatchNumber & BatchBarcode
        modelBuilder.Entity<MedicineBatch>(entity =>
        {
            entity.HasIndex(mb => mb.CompanyBatchNumber).IsUnique();
            entity.HasIndex(mb => mb.BatchBarcode).IsUnique();
        });

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

        modelBuilder.Entity<PurchaseInvoice>()
            .Property(pi => pi.PaymentMethod)
            .HasConversion<int>();

        // PurchaseInvoice - User Audit
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Creator)
            .WithMany()
            .HasForeignKey(pi => pi.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Approver)
            .WithMany()
            .HasForeignKey(pi => pi.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Canceller)
            .WithMany()
            .HasForeignKey(pi => pi.CancelledBy)
            .OnDelete(DeleteBehavior.Restrict);

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
        modelBuilder.Entity<PurchaseInvoiceDetail>(entity =>
        {
            entity.HasOne(pid => pid.Batch)
                .WithMany(mb => mb.PurchaseInvoiceDetails)
                .HasForeignKey(pid => pid.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TrueUnitCost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SaleInvoice>()
            .HasIndex(si => si.SaleInvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<SaleInvoice>()
            .Property(si => si.SaleInvoiceNumber)
            .HasMaxLength(20);

        modelBuilder.Entity<SaleInvoice>()
                .Property(si => si.PaymentMethod)
                .HasConversion<int>();

        modelBuilder.Entity<SaleInvoice>()
            .HasOne(si => si.Customer)
            .WithMany(c => c.SaleInvoices)
            .HasForeignKey(si => si.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SaleInvoice - User Audit
        modelBuilder.Entity<SaleInvoice>()
                .HasOne(si => si.Creator)
                .WithMany()
                .HasForeignKey(si => si.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SaleInvoice>()
                .HasOne(si => si.Approver)
                .WithMany()
                .HasForeignKey(si => si.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SaleInvoice>()
                .HasOne(si => si.Canceller)
                .WithMany()
                .HasForeignKey(si => si.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);

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
            .Property(a => a.Severity)
            .HasConversion<int>()
            .IsRequired();

        modelBuilder.Entity<SalesReturn>()
            .HasOne(sr => sr.SaleInvoice)
            .WithMany(si => si.SalesReturns)
            .HasForeignKey(sr => sr.SaleInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // SalesReturn - User Audit
        modelBuilder.Entity<SalesReturn>()
                .HasOne(sr => sr.Creator)
                .WithMany()
                .HasForeignKey(sr => sr.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalesReturn>()
                .HasOne(sr => sr.Approver)
                .WithMany()
                .HasForeignKey(sr => sr.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalesReturn>()
                .HasOne(sr => sr.Canceller)
                .WithMany()
                .HasForeignKey(sr => sr.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);

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

        // PriceOverride Configuration
        modelBuilder.Entity<PriceOverride>(entity =>
            {
                entity.HasOne(po => po.SaleInvoice)
                      .WithMany()
                      .HasForeignKey(po => po.SaleInvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(po => po.Medicine)
                          .WithMany()
                          .HasForeignKey(po => po.MedicineId)
                          .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.Batch)
                          .WithMany()
                          .HasForeignKey(po => po.BatchId)
                          .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(po => po.User)
                          .WithMany()
                          .HasForeignKey(po => po.UserId)
                          .OnDelete(DeleteBehavior.Restrict);

                entity.Property(po => po.SoldPrice).HasPrecision(18, 2);
                entity.Property(po => po.ActualCost).HasPrecision(18, 2);
            });

        // SalesReturn - Configure decimal precision
        modelBuilder.Entity<SalesReturn>(entity =>
        {
            entity.Property(sr => sr.TotalAmount).HasPrecision(18, 2);
            entity.Property(sr => sr.TotalCost).HasPrecision(18, 2);
            entity.Property(sr => sr.TotalProfit).HasPrecision(18, 2);
        });

        // PurchaseReturn - PurchaseInvoice
        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.PurchaseInvoice)
            .WithMany()
            .HasForeignKey(pr => pr.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseReturn - User Audit
        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.Creator)
            .WithMany()
            .HasForeignKey(pr => pr.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.Approver)
            .WithMany()
            .HasForeignKey(pr => pr.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseReturn>()
            .HasOne(pr => pr.Canceller)
            .WithMany()
            .HasForeignKey(pr => pr.CancelledBy)
            .OnDelete(DeleteBehavior.Restrict);

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

        // ==================== User & Role Configuration ====================

        // Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(200);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Status).HasConversion<int>();
            entity.Property(u => u.Email).HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Notes).HasMaxLength(500);
            entity.Property(u => u.ResetPasswordCode).HasMaxLength(100);

            // Unique username
            entity.HasIndex(u => u.Username).IsUnique();

            // User-Role relationship
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship for CreatedBy
            entity.HasOne(u => u.Creator)
                  .WithMany(u => u.CreatedUsers)
                  .HasForeignKey(u => u.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== Seed Data ====================

        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "مدير النظام - صلاحيات كاملة",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = 2,
                Name = "Pharmacist",
                Description = "صيدلي - صلاحيات البيع والشراء",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Default Users
        // Password for both: Admin@123 (hashed with BCrypt)
        // Note: This is a placeholder hash - will be replaced with actual BCrypt hash in migration
        var defaultPasswordHash = "$2a$11$XKV8qNJKqF3yqVqKqF3yqeqF3yqVqKqF3yqVqKqF3yqVqKqF3yqVq";

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "مدير النظام",
                Username = "admin",
                PasswordHash = defaultPasswordHash,
                RoleId = 1,
                Status = UserStatus.Active,
                Email = "admin@pharmacy.com",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new User
            {
                Id = 2,
                FullName = "صيدلي النظام",
                Username = "pharmacist",
                PasswordHash = defaultPasswordHash,
                RoleId = 2,
                Status = UserStatus.Active,
                Email = "pharmacist@pharmacy.com",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = 1,
                IsDeleted = false
            }
        );
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