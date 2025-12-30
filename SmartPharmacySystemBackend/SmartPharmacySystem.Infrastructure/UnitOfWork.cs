using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Repositories;

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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
