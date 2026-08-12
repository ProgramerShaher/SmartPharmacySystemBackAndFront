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
    DbSet<OnlineOrder> OnlineOrders { get; }
    DbSet<OnlineOrderItem> OnlineOrderItems { get; }
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }
    DbSet<Cheque> Cheques { get; }

    // ===== Multi-Branch & Inventory =====
    DbSet<Branch> Branches { get; }
    DbSet<EmployeeBranchAssignment> EmployeeBranchAssignments { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryStock> InventoryStocks { get; }
    DbSet<MedicineWarehouseConfig> MedicineWarehouseConfigs { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    DbSet<StockTransferItem> StockTransferItems { get; }
    DbSet<DamagedGoodsRecord> DamagedGoodsRecords { get; }
    DbSet<StockCountHeader> StockCountHeaders { get; }
    DbSet<StockCountItem> StockCountItems { get; }
    DbSet<StockCountSchedule> StockCountSchedules { get; }
    DbSet<AutomatedAuditHeader> AutomatedAuditHeaders { get; }
    DbSet<AutomatedAuditItem> AutomatedAuditItems { get; }

    // ===== HR & Payroll =====
    DbSet<Department> Departments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<MonthlySalary> MonthlySalaries { get; }
    DbSet<SalaryDeductionItem> SalaryDeductionItems { get; }
    DbSet<EmployeeLoan> EmployeeLoans { get; }

    // ===== Customer & Finance =====
    DbSet<CustomerLedger> CustomerLedgers { get; }
    DbSet<InterBranchSettlement> InterBranchSettlements { get; }
    DbSet<DailyClosing> DailyClosings { get; }

    // ===== Notifications =====
    DbSet<Notification> Notifications { get; }
    DbSet<UserShift> UserShifts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
