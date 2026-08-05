using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
namespace SmartPharmacySystem.Infrastructure.Data;

/// <summary>
/// Represents the database context for the application.
/// This class manages the database connections and entity configurations.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
        // Increase timeout for heavy queries
        this.Database.SetCommandTimeout(60);
    }

    // Dynamic Runtime Property evaluated per DB request
    public int? CurrentBranchId => _currentUserService.GetCurrentBranchId();

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<PharmacySettings> PharmacySettings { get; set; } = null!;
    public DbSet<Medicine> Medicines { get; set; } = null!;
    public DbSet<MedicineBatch> MedicineBatches { get; set; } = null!;
    public DbSet<MedicineUnit> MedicineUnits { get; set; } = null!;
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
    public DbSet<OnlineOrder> OnlineOrders { get; set; } = null!;
    public DbSet<OnlineOrderItem> OnlineOrderItems { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; } = null!;
    public DbSet<Cheque> Cheques { get; set; } = null!;

    // ===== Multi-Branch & Inventory =====
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<EmployeeBranchAssignment> EmployeeBranchAssignments { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
    public DbSet<InventoryStock> InventoryStocks { get; set; } = null!;
    public DbSet<MedicineWarehouseConfig> MedicineWarehouseConfigs { get; set; } = null!;
    public DbSet<StockTransfer> StockTransfers { get; set; } = null!;
    public DbSet<StockTransferItem> StockTransferItems { get; set; } = null!;
    public DbSet<DamagedGoodsRecord> DamagedGoodsRecords { get; set; } = null!;
    public DbSet<StockCountHeader> StockCountHeaders { get; set; } = null!;
    public DbSet<StockCountItem> StockCountItems { get; set; } = null!;

    // ===== HR & Payroll =====
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<MonthlySalary> MonthlySalaries { get; set; } = null!;
    public DbSet<SalaryDeductionItem> SalaryDeductionItems { get; set; } = null!;
    public DbSet<EmployeeLoan> EmployeeLoans { get; set; } = null!;

    // ===== Customer & Finance =====
    public DbSet<CustomerLedger> CustomerLedgers { get; set; } = null!;
    public DbSet<InterBranchSettlement> InterBranchSettlements { get; set; } = null!;
    public DbSet<DailyClosing> DailyClosings { get; set; } = null!;

    // ===== Notifications =====
    public DbSet<Notification> Notifications { get; set; } = null!;



    // ===== Shifts =====
    public DbSet<UserShift> UserShifts { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress circular dependency and phantom model change warnings that block Update-Database in EF 9
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

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
                BranchId = 1,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1)
            });

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);
            
            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });



        modelBuilder.Entity<SalesReturn>(entity =>
        {
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalProfit).HasPrecision(18, 2);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
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

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Supplier Configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.Balance).HasPrecision(18, 2);
        });

        // Medicine Configuration
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.Property(e => e.MovingAverageCost).HasPrecision(18, 2);
            entity.Property(e => e.DefaultPurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.DefaultSalePrice).HasPrecision(18, 2);
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

        // MedicineUnit - Medicine
        modelBuilder.Entity<MedicineUnit>()
            .HasOne(mu => mu.Medicine)
            .WithMany(m => m.MedicineUnits)
            .HasForeignKey(mu => mu.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedicineUnit>(entity =>
        {
            entity.Property(mu => mu.DefaultPurchasePrice).HasPrecision(18, 2);
            entity.Property(mu => mu.DefaultSalePrice).HasPrecision(18, 2);
            entity.HasIndex(mu => mu.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        });

        // MedicineBatch Configuration
        modelBuilder.Entity<MedicineBatch>(entity =>
        {
            entity.HasIndex(mb => mb.CompanyBatchNumber).IsUnique();
            entity.HasIndex(mb => mb.BatchBarcode).IsUnique();

            // Explicitly map CreatedBy relationship (avoids shadow property CreatedByUserId)
            entity.HasOne(mb => mb.CreatedByUser)
                  .WithMany(u => u.CreatedBatches)
                  .HasForeignKey(mb => mb.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseInvoice - Supplier
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Supplier)
            .WithMany(s => s.PurchaseInvoices)
            .HasForeignKey(pi => pi.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Warehouse)
            .WithMany()
            .HasForeignKey(pi => pi.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseInvoice>()
            .HasIndex(pi => pi.PurchaseInvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<PurchaseInvoice>()
            .Property(pi => pi.PurchaseInvoiceNumber)
            .HasMaxLength(20);

        modelBuilder.Entity<PurchaseInvoice>()
            .Property(pi => pi.PaymentMethod)
            .HasConversion<int>();

        modelBuilder.Entity<PurchaseInvoice>()
            .Property(pi => pi.TotalAmount)
            .HasPrecision(18, 2);

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

        // Dynamic Multi-Branch Tenant Filter
        modelBuilder.Entity<PurchaseInvoice>()
            .HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

        // Configure Foreign Key Constraints
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Branch)
            .WithMany()
            .HasForeignKey(pi => pi.BranchId)
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
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.SalePrice).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
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

        modelBuilder.Entity<SaleInvoice>(entity =>
        {
            entity.Property(si => si.TotalAmount).HasPrecision(18, 2);
            entity.Property(si => si.TotalCost).HasPrecision(18, 2);
            entity.Property(si => si.TotalProfit).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SaleInvoice>()
            .HasOne(si => si.Customer)
            .WithMany(c => c.SaleInvoices)
            .HasForeignKey(si => si.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dynamic Multi-Branch Tenant Filter
        modelBuilder.Entity<SaleInvoice>()
            .HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

        // Configure Foreign Key Constraints
        modelBuilder.Entity<SaleInvoice>()
            .HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
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
        modelBuilder.Entity<SaleInvoiceDetail>(entity =>
        {
            entity.HasOne(sid => sid.Batch)
                .WithMany(mb => mb.SaleInvoiceDetails)
                .HasForeignKey(sid => sid.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(sid => sid.SalePrice).HasPrecision(18, 2);
            entity.Property(sid => sid.UnitCost).HasPrecision(18, 2);
            entity.Property(sid => sid.TotalLineAmount).HasPrecision(18, 2);
            entity.Property(sid => sid.TotalCost).HasPrecision(18, 2);
            entity.Property(sid => sid.Profit).HasPrecision(18, 2);
        });

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

        // Dynamic Multi-Branch Tenant Filter
        modelBuilder.Entity<InventoryMovement>()
            .HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

        // Configure Foreign Key Constraints
        modelBuilder.Entity<InventoryMovement>()
            .HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Alert - MedicineBatch
        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Batch)
            .WithMany(mb => mb.Alerts)
            .HasForeignKey(a => a.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Smart Filter: Main Branch (1) sees all, others see their own or global (null)
        modelBuilder.Entity<Alert>()
            .HasQueryFilter(a => !CurrentBranchId.HasValue || CurrentBranchId.Value == 1 || a.BranchId == null || a.BranchId == CurrentBranchId.Value);

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
        modelBuilder.Entity<SalesReturnDetail>(entity =>
        {
            entity.HasOne(srd => srd.Batch)
                .WithMany(mb => mb.SalesReturnDetails)
                .HasForeignKey(srd => srd.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(srd => srd.SalePrice).HasPrecision(18, 2);
            entity.Property(srd => srd.UnitCost).HasPrecision(18, 2);
            entity.Property(srd => srd.TotalLineAmount).HasPrecision(18, 2);
            entity.Property(srd => srd.TotalCost).HasPrecision(18, 2);
            entity.Property(srd => srd.Profit).HasPrecision(18, 2);
            entity.Property(srd => srd.TotalReturn).HasPrecision(18, 2);
        });

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
        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasOne(pr => pr.Supplier)
                .WithMany(s => s.PurchaseReturns)
                .HasForeignKey(pr => pr.SupplierId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(pr => pr.TotalAmount).HasPrecision(18, 2);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

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
        modelBuilder.Entity<PurchaseReturnDetail>(entity =>
        {
            entity.HasOne(prd => prd.Batch)
                .WithMany(mb => mb.PurchaseReturnDetails)
                .HasForeignKey(prd => prd.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(prd => prd.PurchasePrice).HasPrecision(18, 2);
            entity.Property(prd => prd.TotalReturn).HasPrecision(18, 2);
        });

        // Configure InvoiceNumberSequence
        modelBuilder.Entity<InvoiceNumberSequence>()
            .HasKey(isq => isq.Id);

        modelBuilder.Entity<InvoiceNumberSequence>()
            .HasIndex(isq => new { isq.Prefix, isq.Year })
            .IsUnique();
        // ==================== StockTransfer Configuration ====================

        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TransferCode).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Status).HasConversion<int>();
            entity.Property(t => t.TransferType).HasConversion<int>();
            entity.Property(t => t.Notes).HasMaxLength(500);

            // Multi-Branch tenant filter
            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            // Source Warehouse FK
            entity.HasOne(t => t.SourceWarehouse)
                  .WithMany()
                  .HasForeignKey(t => t.SourceWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Destination Warehouse FK
            entity.HasOne(t => t.DestinationWarehouse)
                  .WithMany()
                  .HasForeignKey(t => t.DestinationWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            // RequestedBy User FK
            entity.HasOne(t => t.RequestedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.RequestedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ApprovedBy User FK (nullable)
            entity.HasOne(t => t.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // DispatchedBy User FK (nullable)
            entity.HasOne(t => t.DispatchedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.DispatchedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ReceivedBy User FK (nullable)
            entity.HasOne(t => t.ReceivedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.ReceivedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Items collection
            entity.HasMany(t => t.Items)
                  .WithOne(i => i.StockTransfer)
                  .HasForeignKey(i => i.StockTransferId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.BatchNumber).HasMaxLength(100);

            entity.HasOne(i => i.Medicine)
                  .WithMany()
                  .HasForeignKey(i => i.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

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
        // ==================== Seed Data ====================

        // 1. Seed Roles (يتم تعريفها مرة واحدة فقط)
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

        // 2. تلويد الهاش لكلمة المرور "1234"
        // ملاحظة: تأكد من وجود مكتبة BCrypt.Net-Next في المشروع
        // في أعلى الملف تأكد من وجود:
        // using BCrypt.Net;

        // داخل ميثود OnModelCreating في جزء Seed Users:
        var hashed1234 = BCrypt.Net.BCrypt.HashPassword("1234");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "مدير النظام",
                Username = "admin",
                PasswordHash = hashed1234, // نضع المتغير الذي ولدناه الآن
                RoleId = 1,
                Status = UserStatus.Active,
                Email = "admin@pharmacy.com",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = null,
                IsDeleted = false
            },

        new User
            {
                Id = 2,
                FullName = "صيدلي النظام",
                Username = "pharmacist",
                PasswordHash = hashed1234,
                RoleId = 2,
                Status = UserStatus.Active,
                Email = "pharmacist@pharmacy.com",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = 1,
                IsDeleted = false
            }
        );
        // ==================== Multi-Branch & Inventory Configuration ====================

        // Branch Configuration
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.Property(e => e.BranchCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Location).HasMaxLength(250);
            entity.Property(e => e.BranchType).HasConversion<int>();
            entity.HasIndex(e => e.BranchCode).IsUnique();

            // Seed Branches 1 and 2
            entity.HasData(
                new Branch
                {
                    Id = 1,
                    BranchCode = "BR-001",
                    Name = "الفرع الرئيسي",
                    Location = "المركز الرئيسي",
                    BranchType = BranchType.Main,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Branch
                {
                    Id = 2,
                    BranchCode = "BR-002",
                    Name = "فرع السليمانية",
                    Location = "حي السليمانية",
                    BranchType = BranchType.Sub,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });

        // EmployeeBranchAssignment Configuration
        modelBuilder.Entity<EmployeeBranchAssignment>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.BranchId });
            entity.HasIndex(e => e.IsActive);

            // Seed initial branch assignments for existing seed users
            entity.HasData(
                new EmployeeBranchAssignment
                {
                    Id = 1,
                    UserId = 1, // admin
                    BranchId = 1, // Main branch
                    AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new EmployeeBranchAssignment
                {
                    Id = 2,
                    UserId = 2, // pharmacist
                    BranchId = 2, // Sub branch
                    AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });

        // Warehouse Configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Type).HasConversion<int>();

            entity.HasOne(e => e.Branch)
                  .WithMany(b => b.Warehouses)
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BranchId);
        });

        // InventoryStock Configuration
        modelBuilder.Entity<InventoryStock>(entity =>
        {
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Warehouse)
                  .WithMany()
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Medicine)
                  .WithMany()
                  .HasForeignKey(e => e.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.WarehouseId, e.MedicineId, e.BatchNumber }).IsUnique();
            entity.HasIndex(e => e.ExpiryDate);
        });

        // MedicineWarehouseConfig Configuration
        modelBuilder.Entity<MedicineWarehouseConfig>(entity =>
        {
            entity.HasKey(e => new { e.WarehouseId, e.MedicineId });

            entity.HasOne(e => e.Warehouse)
                  .WithMany()
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Medicine)
                  .WithMany()
                  .HasForeignKey(e => e.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // StockTransfer Configuration
        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.Property(e => e.TransferCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.TransferType).HasConversion<int>();

            entity.HasIndex(e => e.TransferCode).IsUnique();

            entity.HasOne(e => e.SourceWarehouse)
                  .WithMany()
                  .HasForeignKey(e => e.SourceWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationWarehouse)
                  .WithMany()
                  .HasForeignKey(e => e.DestinationWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RequestedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.RequestedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DispatchedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.DispatchedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReceivedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ReceivedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Dynamic Multi-Branch Tenant Filter
            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId);

            // Configure Foreign Key Constraints
            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // StockTransferItem Configuration
        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.StockTransfer)
                  .WithMany(s => s.Items)
                  .HasForeignKey(e => e.StockTransferId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Medicine)
                  .WithMany()
                  .HasForeignKey(e => e.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DamagedGoodsRecord Configuration
        modelBuilder.Entity<DamagedGoodsRecord>(entity =>
        {
            entity.Property(e => e.DamageCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DamageValue).HasPrecision(18, 2);
            entity.Property(e => e.DamageType).HasConversion<int>();
            entity.Property(e => e.DisposalMethod).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasIndex(e => e.DamageCode).IsUnique();
            entity.HasIndex(e => new { e.SourceWarehouseId, e.Status });

            entity.HasOne(e => e.SourceWarehouse)
                  .WithMany()
                  .HasForeignKey(e => e.SourceWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Medicine)
                  .WithMany()
                  .HasForeignKey(e => e.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RecordedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.RecordedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // StockCountHeader Configuration
        modelBuilder.Entity<StockCountHeader>(entity =>
        {
            entity.Property(e => e.CountCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CountType).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasIndex(e => e.CountCode).IsUnique();

            entity.HasOne(e => e.Warehouse)
                  .WithMany()
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // StockCountItem Configuration
        modelBuilder.Entity<StockCountItem>(entity =>
        {
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.VarianceReason).HasMaxLength(250);

            entity.HasOne(e => e.StockCountHeader)
                  .WithMany(s => s.Items)
                  .HasForeignKey(e => e.StockCountHeaderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Medicine)
                  .WithMany()
                  .HasForeignKey(e => e.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== HR & Payroll Configuration ====================

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.NationalId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);

            entity.HasIndex(e => e.EmployeeCode).IsUnique();

            entity.HasOne(e => e.Branch)
                  .WithMany(b => b.Employees)
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                  .WithMany()
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Attendance Configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.Property(e => e.WorkedHours).HasPrecision(5, 2);
            entity.Property(e => e.Shift).HasConversion<int>();
            entity.Property(e => e.AttendanceStatus).HasConversion<int>();

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Attendances)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkingBranch)
                  .WithMany()
                  .HasForeignKey(e => e.WorkingBranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmployeeId, e.CheckIn });
        });

        // MonthlySalary Configuration
        modelBuilder.Entity<MonthlySalary>(entity =>
        {
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.TotalAllowances).HasPrecision(18, 2);
            entity.Property(e => e.TotalBonuses).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            //entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.PaymentStatus).HasConversion<int>();

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Salaries)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month }).IsUnique();
        });

        // SalaryDeductionItem Configuration
        modelBuilder.Entity<SalaryDeductionItem>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.DeductionType).HasConversion<int>();
            entity.Property(e => e.Description).HasMaxLength(250);

            //entity.HasOne(e => e.MonthlySalary)
            //      .WithMany(s => s.Deductions)
            //      .HasForeignKey(e => e.MonthlySalaryId)
            //      .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployeeLoan Configuration
        modelBuilder.Entity<EmployeeLoan>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyInstalment).HasPrecision(18, 2);
            entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Loans)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmployeeId, e.IsFullyPaid });
        });

        // ==================== Customer & Finance Configuration ====================

        // CustomerLedger Configuration
        modelBuilder.Entity<CustomerLedger>(entity =>
        {
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);
            entity.Property(e => e.TransactionType).HasConversion<int>();

            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => new { e.CustomerId, e.TransactionDate });
            entity.HasIndex(e => e.BranchId);
        });

        // InterBranchSettlement Configuration
        modelBuilder.Entity<InterBranchSettlement>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(e => e.FromBranch)
                  .WithMany()
                  .HasForeignKey(e => e.FromBranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToBranch)
                  .WithMany()
                  .HasForeignKey(e => e.ToBranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SettledByUser)
                  .WithMany()
                  .HasForeignKey(e => e.SettledByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.FromBranchId, e.Status });
        });

        // DailyClosing Configuration
        modelBuilder.Entity<DailyClosing>(entity =>
        {
            entity.Property(e => e.OpeningCash).HasPrecision(18, 2);
            entity.Property(e => e.TotalCashSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCreditSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCardSales).HasPrecision(18, 2);
            entity.Property(e => e.TotalCollections).HasPrecision(18, 2);
            entity.Property(e => e.TotalCashReturns).HasPrecision(18, 2);
            entity.Property(e => e.TotalExpenses).HasPrecision(18, 2);
            entity.Property(e => e.ActualCash).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();

            entity.Property(e => e.BranchId).IsRequired();

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SubmittedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.SubmittedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.BranchId, e.ClosingDate }).IsUnique();

            // Dynamic Multi-Branch Tenant Filter
            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId);
        });

        // ==================== Notifications Configuration ====================
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Body).HasMaxLength(1000);
            entity.Property(e => e.ReferenceType).HasMaxLength(100);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Smart Filter: Main Branch (1) sees all, others see their own or global (null)
            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || CurrentBranchId.Value == 1 || e.BranchId == null || e.BranchId == CurrentBranchId.Value);

            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
        });

        // ==================== Performance Indexes ====================
        // Added for query optimization and faster search operations

        // Medicines Indexes
        modelBuilder.Entity<Medicine>()
            .HasIndex(m => m.Name)
            .HasDatabaseName("IX_Medicines_Name_Performance");

        modelBuilder.Entity<Medicine>()
            .HasIndex(m => m.DefaultBarcode)
            .HasDatabaseName("IX_Medicines_Barcode_Performance");

        modelBuilder.Entity<Medicine>()
            .HasIndex(m => m.ScientificName)
            .HasDatabaseName("IX_Medicines_ScientificName");

        modelBuilder.Entity<Medicine>()
            .HasIndex(m => new { m.CategoryId, m.Status })
            .HasDatabaseName("IX_Medicines_CategoryId_Status");

        // MedicineBatches Indexes
        modelBuilder.Entity<MedicineBatch>()
            .HasIndex(mb => new { mb.MedicineId, mb.ExpiryDate })
            .HasDatabaseName("IX_MedicineBatches_MedicineId_ExpiryDate");

        modelBuilder.Entity<MedicineBatch>()
            .HasIndex(mb => new { mb.Status, mb.ExpiryDate })
            .HasDatabaseName("IX_MedicineBatches_Status_ExpiryDate");

        // SaleInvoices Indexes
        modelBuilder.Entity<SaleInvoice>()
            .HasIndex(si => new { si.InvoiceDate, si.Status })
            .HasDatabaseName("IX_SaleInvoices_InvoiceDate_Status");

        modelBuilder.Entity<SaleInvoice>()
            .HasIndex(si => si.CustomerId)
            .HasDatabaseName("IX_SaleInvoices_CustomerId_Performance");

        modelBuilder.Entity<SaleInvoice>()
            .HasIndex(si => si.CreatedBy)
            .HasDatabaseName("IX_SaleInvoices_CreatedBy");

        // PurchaseInvoices Indexes
        modelBuilder.Entity<PurchaseInvoice>()
            .HasIndex(pi => new { pi.PurchaseDate, pi.Status })
            .HasDatabaseName("IX_PurchaseInvoices_PurchaseDate_Status");

        modelBuilder.Entity<PurchaseInvoice>()
            .HasIndex(pi => pi.SupplierId)
            .HasDatabaseName("IX_PurchaseInvoices_SupplierId_Performance");

        // Suppliers Indexes
        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.Name)
            .HasDatabaseName("IX_Suppliers_Name_Performance");

        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.PhoneNumber)
            .HasDatabaseName("IX_Suppliers_PhoneNumber");

        // Alerts Indexes
        modelBuilder.Entity<Alert>()
            .HasIndex(a => new { a.BatchId, a.AlertType })
            .HasDatabaseName("IX_Alerts_BatchId_AlertType");

        modelBuilder.Entity<Alert>()
            .HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_Alerts_CreatedAt");

        // ==================== OnlineOrder Configuration ====================
        modelBuilder.Entity<OnlineOrder>(entity =>
        {
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(o => o.CustomerNotes).HasMaxLength(1000);
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.PaymentMethod).HasConversion<int>();
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.OrderDate);

            entity.HasOne(o => o.Customer)
                  .WithMany()
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Handler)
                  .WithMany()
                  .HasForeignKey(o => o.HandledBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OnlineOrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(oi => oi.OnlineOrder)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OnlineOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Medicine)
                  .WithMany()
                  .HasForeignKey(oi => oi.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==================== Accounting System Configuration ====================

        // Account (Chart of Accounts)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();

            entity.HasIndex(e => e.Code).IsUnique();

            // Hierarchical relationship
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntry Configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.Property(e => e.VoucherNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();

            entity.HasIndex(e => e.VoucherNumber).IsUnique();
            entity.HasIndex(e => e.EntryDate);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntryLine Configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);

            entity.HasOne(e => e.JournalEntry)
                  .WithMany(e => e.Lines)
                  .HasForeignKey(e => e.JournalEntryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                  .WithMany(e => e.JournalEntryLines)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !CurrentBranchId.HasValue || e.BranchId == CurrentBranchId.Value);

            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Cheque Configuration
        modelBuilder.Entity<Cheque>(entity =>
        {
            entity.Property(e => e.ChequeNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BankName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Type).HasConversion<int>();

            entity.HasIndex(e => e.ChequeNumber);
            entity.HasIndex(e => e.DueDate);
        });



        // 4. Seed Medicine Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "مضادات حيوية", Description = "أدوية علاج الالتهابات البكتيرية", CreatedAt = new DateTime(2025, 1, 1) },
            new Category { Id = 2, Name = "مسكنات", Description = "أدوية تخفيف الألم وخافضات الحرارة", CreatedAt = new DateTime(2025, 1, 1) },
            new Category { Id = 3, Name = "فيتامينات", Description = "مكملات غذائية وفيتامينات", CreatedAt = new DateTime(2025, 1, 1) }
        );

        // 5. Seed Medicines
        modelBuilder.Entity<Medicine>().HasData(
            new Medicine 
            { 
                Id = 1, 
                Name = "Panadol Advance", 
                ScientificName = "Paracetamol", 
                CategoryId = 2, 
                DefaultBarcode = "5011309100010",
                DefaultSalePrice = 15.00m,
                DefaultPurchasePrice = 10.00m,
                Status = "Active",
                CreatedAt = new DateTime(2025, 1, 1)
            },
            new Medicine 
            { 
                Id = 2, 
                Name = "Augmentin 1g", 
                ScientificName = "Amoxicillin", 
                CategoryId = 1, 
                DefaultBarcode = "5011309100020",
                DefaultSalePrice = 85.00m,
                DefaultPurchasePrice = 60.00m,
                Status = "Active",
                CreatedAt = new DateTime(2025, 1, 1)
            }
        );

        // 6. Seed Suppliers & Customers
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "شركة الأدوية العالمية", PhoneNumber = "011223344", CreatedAt = new DateTime(2025, 1, 1) }
        );
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "عميل نقدي عام", PhoneNumber = "000000000", CreatedAt = new DateTime(2025, 1, 1) }
        );


        // ==================== Full Chart of Accounts Seeding ====================
        modelBuilder.Entity<Account>().HasData(
            // 1. الأصول (Assets)
            new Account { Id = 1, Code = "1", Name = "الأصول", Type = AccountType.Asset, IsMainAccount = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 11, Code = "11", Name = "الأصول المتداولة", Type = AccountType.Asset, IsMainAccount = true, ParentId = 1, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            
            // حسابات النقدية (IDs expected by Service)
            new Account { Id = 1101, Code = "1101", Name = "الصندوق الرئيسي", Type = AccountType.Asset, IsMainAccount = false, ParentId = 11, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 1102, Code = "1102", Name = "البنك - حساب جاري", Type = AccountType.Asset, IsMainAccount = false, ParentId = 11, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            
            // حسابات المخزون (ID 1301 expected by Service)
            new Account { Id = 1301, Code = "1301", Name = "مخزون الأدوية", Type = AccountType.Asset, IsMainAccount = false, ParentId = 11, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },

            // 2. الخصوم (Liabilities)
            new Account { Id = 2, Code = "2", Name = "الخصوم", Type = AccountType.Liability, IsMainAccount = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 21, Code = "21", Name = "الخصوم المتداولة", Type = AccountType.Liability, IsMainAccount = true, ParentId = 2, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 2101, Code = "2101", Name = "ذمم الموردين", Type = AccountType.Liability, IsMainAccount = false, ParentId = 21, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },

            // 3. حقوق الملكية (Equity)
            new Account { Id = 3, Code = "3", Name = "حقوق الملكية", Type = AccountType.Equity, IsMainAccount = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 31, Code = "31", Name = "رأس المال", Type = AccountType.Equity, IsMainAccount = true, ParentId = 3, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 3101, Code = "3101", Name = "رأس مال الشركاء", Type = AccountType.Equity, IsMainAccount = false, ParentId = 31, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },

            // 4. الإيرادات (Revenues)
            new Account { Id = 4, Code = "4", Name = "الإيرادات", Type = AccountType.Revenue, IsMainAccount = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 41, Code = "41", Name = "إيرادات المبيعات", Type = AccountType.Revenue, IsMainAccount = false, ParentId = 4, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },

            // 5. المصروفات (Expenses)
            new Account { Id = 5, Code = "5", Name = "المصروفات", Type = AccountType.Expense, IsMainAccount = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 51, Code = "51", Name = "تكلفة المشتريات", Type = AccountType.Expense, IsMainAccount = false, ParentId = 5, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 52, Code = "52", Name = "مصروفات تشغيلية", Type = AccountType.Expense, IsMainAccount = false, ParentId = 5, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
            new Account { Id = 5206, Code = "5206", Name = "خسائر الأدوية التالفة", Type = AccountType.Expense, IsMainAccount = false, ParentId = 52, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) }
        );


        // 7. Seed Opening Balance Journal Entry (50,000 Cash) - مرحل مسبقاً
        modelBuilder.Entity<JournalEntry>().HasData(
            new JournalEntry 
            { 
                Id = 1, 
                BranchId = 1,
                VoucherNumber = "OB-2025-001", 
                EntryDate = new DateTime(2025, 1, 1), 
                Description = "قيد افتتاحي - رصيد الصندوق", 
                Type = VoucherType.AdjustmentEntry,
                TotalDebit = 50000m,
                TotalCredit = 50000m,
                IsPosted = true, // مرحل - هذا قيد تاريخي
                CreatedBy = 1,
                CreatedAt = new DateTime(2025, 1, 1)
            }
        );

        modelBuilder.Entity<JournalEntryLine>().HasData(
            new JournalEntryLine 
            { 
                Id = 1, 
                BranchId = 1,
                JournalEntryId = 1, 
                AccountId = 1101, // الصندوق الرئيسي (تم تحديثه)
                Debit = 50000m, 
                Credit = 0, 
                Description = "إيداع رصيد افتتاحي" 
            },
            new JournalEntryLine 
            { 
                Id = 2, 
                BranchId = 1,
                JournalEntryId = 1, 
                AccountId = 3101, // رأس مال الشركاء (تم تحديثه)
                Debit = 0, 
                Credit = 50000m, 
                Description = "إثبات رأس المال" 
            }
        );
    }

    public override int SaveChanges()
    {
        ApplyBaseEntityAuditLogic();
        ApplyMultiBranchLogic();
        HandleInvoiceNumberImmutability();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBaseEntityAuditLogic();
        ApplyMultiBranchLogic();
        HandleInvoiceNumberImmutability();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyMultiBranchLogic()
    {
        foreach (var entry in ChangeTracker.Entries<BaseMultiBranchEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Auto-inject current user's BranchId to the new invoice
                if (CurrentBranchId.HasValue)
                {
                    entry.Entity.BranchId = CurrentBranchId.Value;
                }
            }
        }
    }

    private void ApplyBaseEntityAuditLogic()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsDeleted = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Deleted)
            {
                // Soft delete logic
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
            }
        }
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
