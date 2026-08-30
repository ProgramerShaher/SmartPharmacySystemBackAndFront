using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SmartPharmacySystem.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        private IMedicineRepository? _medicines;
        public IMedicineRepository Medicines => _medicines ??= new MedicineRepository(_context);

        private IUserRepository? _users;
        public IUserRepository Users => _users ??= new UserRepository(_context);

        private IRoleRepository? _roles;
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);

        private ISupplierRepository? _suppliers;
        public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);

        private IPurchaseInvoiceRepository? _purchaseInvoices;
        public IPurchaseInvoiceRepository PurchaseInvoices => _purchaseInvoices ??= new PurchaseInvoiceRepository(_context);

        private ISaleInvoiceRepository? _saleInvoices;
        public ISaleInvoiceRepository SaleInvoices => _saleInvoices ??= new SaleInvoiceRepository(_context);

        private ISalesReturnRepository? _salesReturns;
        public ISalesReturnRepository SalesReturns => _salesReturns ??= new SalesReturnRepository(_context);

        private IInventoryMovementRepository? _inventoryMovements;
        public IInventoryMovementRepository InventoryMovements => _inventoryMovements ??= new InventoryMovementRepository(_context);

        private ICategoryRepository? _categories;
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        private IExpenseRepository? _expenses;
        public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_context);

        private IExpenseCategoryRepository? _expenseCategories;
        public IExpenseCategoryRepository ExpenseCategories => _expenseCategories ??= new ExpenseCategoryRepository(_context);

        private IAlertRepository? _alerts;
        public IAlertRepository Alerts => _alerts ??= new AlertRepository(_context);

        private IMedicineBatchRepository? _medicineBatches;
        public IMedicineBatchRepository MedicineBatches => _medicineBatches ??= new MedicineBatchRepository(_context);

        private IMedicineUnitRepository? _medicineUnits;
        public IMedicineUnitRepository MedicineUnits => _medicineUnits ??= new MedicineUnitRepository(_context);

        private IPurchaseInvoiceDetailRepository? _purchaseInvoiceDetails;
        public IPurchaseInvoiceDetailRepository PurchaseInvoiceDetails => _purchaseInvoiceDetails ??= new PurchaseInvoiceDetailRepository(_context);

        private IPurchaseReturnDetailRepository? _purchaseReturnDetails;
        public IPurchaseReturnDetailRepository PurchaseReturnDetails => _purchaseReturnDetails ??= new PurchaseReturnDetailRepository(_context);

        private ISaleInvoiceDetailRepository? _saleInvoiceDetails;
        public ISaleInvoiceDetailRepository SaleInvoiceDetails => _saleInvoiceDetails ??= new SaleInvoiceDetailRepository(_context);

        private ISalesReturnDetailRepository? _salesReturnDetails;
        public ISalesReturnDetailRepository SalesReturnDetails => _salesReturnDetails ??= new SalesReturnDetailRepository(_context);

        private IPurchaseReturnRepository? _purchaseReturns;
        public IPurchaseReturnRepository PurchaseReturns => _purchaseReturns ??= new PurchaseReturnRepository(_context);

        private IFinancialRepository? _financials;
        public IFinancialRepository Financials => _financials ??= new FinancialRepository(_context);

        private IInvoiceSequenceRepository? _invoiceSequences;
        public IInvoiceSequenceRepository InvoiceSequences => _invoiceSequences ??= new InvoiceSequenceRepository(_context);

        private ISupplierPaymentRepository? _supplierPayments;
        private ICustomerRepository? _customers;
        private ICustomerReceiptRepository? _customerReceipts;
        public ISupplierPaymentRepository SupplierPayments => _supplierPayments ??= new SupplierPaymentRepository(_context);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public ICustomerReceiptRepository CustomerReceipts => _customerReceipts ??= new CustomerReceiptRepository(_context);

        private IPriceOverrideRepository? _priceOverrides;
        public IPriceOverrideRepository PriceOverrides => _priceOverrides ??= new PriceOverrideRepository(_context);

        private IAccountRepository? _accounts;
        public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);

        private IJournalEntryRepository? _journalEntries;
        public IJournalEntryRepository JournalEntries => _journalEntries ??= new JournalEntryRepository(_context);

        private IChequeRepository? _cheques;
        public IChequeRepository Cheques => _cheques ??= new ChequeRepository(_context);

        private IPharmacySettingsRepository? _pharmacySettings;
        public IPharmacySettingsRepository PharmacySettings => _pharmacySettings ??= new PharmacySettingsRepository(_context);

        private IEmployeeRepository? _employees;
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);

        private IDepartmentRepository? _departments;
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);

        private IAttendanceRepository? _attendances;
        public IAttendanceRepository Attendances => _attendances ??= new AttendanceRepository(_context);

        private IMonthlySalaryRepository? _monthlySalaries;
        public IMonthlySalaryRepository MonthlySalaries => _monthlySalaries ??= new MonthlySalaryRepository(_context);

        private IEmployeeLoanRepository? _employeeLoans;
        public IEmployeeLoanRepository EmployeeLoans => _employeeLoans ??= new EmployeeLoanRepository(_context);

        private ICustomerLedgerRepository? _customerLedgers;
        public ICustomerLedgerRepository CustomerLedgers => _customerLedgers ??= new CustomerLedgerRepository(_context);

        private IInterBranchSettlementRepository? _interBranchSettlements;
        public IInterBranchSettlementRepository InterBranchSettlements => _interBranchSettlements ??= new InterBranchSettlementRepository(_context);

        private IDailyClosingRepository? _dailyClosings;
        public IDailyClosingRepository DailyClosings => _dailyClosings ??= new DailyClosingRepository(_context);

        private IInventoryStockRepository? _inventoryStocks;
        public IInventoryStockRepository InventoryStocks => _inventoryStocks ??= new InventoryStockRepository(_context);

        private IStockCountRepository? _stockCounts;
        public IStockCountRepository StockCounts => _stockCounts ??= new StockCountRepository(_context);

        private IDamagedGoodsRepository? _damagedGoods;
        public IDamagedGoodsRepository DamagedGoods => _damagedGoods ??= new DamagedGoodsRepository(_context);

        private IStockTransferRepository? _stockTransfers;
        public IStockTransferRepository StockTransfers => _stockTransfers ??= new StockTransferRepository(_context);

        private IStockTransferItemRepository? _stockTransferItems;
        public IStockTransferItemRepository StockTransferItems => _stockTransferItems ??= new StockTransferItemRepository(_context);

        private IMedicineWarehouseConfigRepository? _medicineWarehouseConfigs;
        public IMedicineWarehouseConfigRepository MedicineWarehouseConfigs => _medicineWarehouseConfigs ??= new MedicineWarehouseConfigRepository(_context);

        private IWarehouseRepository? _warehouses;
        public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_context);

        private IBranchRepository? _branches;
        public IBranchRepository Branches => _branches ??= new BranchRepository(_context);

        private IEmployeeBranchAssignmentRepository? _employeeBranchAssignments;
        public IEmployeeBranchAssignmentRepository EmployeeBranchAssignments => _employeeBranchAssignments ??= new EmployeeBranchAssignmentRepository(_context);

        private IPricelistRepository? _pricelists;
        public IPricelistRepository Pricelists => _pricelists ??= new PricelistRepository(_context);

        // ===== RBAC =====
        private IPermissionRepository? _permissions;
        public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);

        private IUserPermissionOverrideRepository? _userPermissionOverrides;
        public IUserPermissionOverrideRepository UserPermissionOverrides => _userPermissionOverrides ??= new UserPermissionOverrideRepository(_context);

        private IAuditLogRepository? _auditLogs;
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

        private IUserShiftRepository? _userShifts;
        public IUserShiftRepository UserShifts => _userShifts ??= new UserShiftRepository(_context);

        private IFinancialPeriodRepository? _financialPeriods;
        public IFinancialPeriodRepository FinancialPeriods => _financialPeriods ??= new FinancialPeriodRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task ExecuteTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await action();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
