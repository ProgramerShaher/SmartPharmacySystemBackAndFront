using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Interfaces.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Category> Categories { get; }
    DbSet<Medicine> Medicines { get; }
    DbSet<MedicineBatch> MedicineBatches { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseInvoice> PurchaseInvoices { get; }
    DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; }
    DbSet<SaleInvoice> SaleInvoices { get; }
    DbSet<SaleInvoiceDetail> SaleInvoiceDetails { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<SalesReturn> SalesReturns { get; }
    DbSet<SalesReturnDetail> SalesReturnDetails { get; }
    DbSet<PurchaseReturn> PurchaseReturns { get; }
    DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; }
    DbSet<InvoiceNumberSequence> InvoiceSequences { get; }
    DbSet<PriceOverride> PriceOverrides { get; }
    DbSet<PharmacyAccount> PharmacyAccounts { get; }
    DbSet<FinancialTransaction> FinancialTransactions { get; }
    DbSet<CustomerReceipt> CustomerReceipts { get; }
    DbSet<Customer> Customers { get; }
    DbSet<SupplierPayment> SupplierPayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
