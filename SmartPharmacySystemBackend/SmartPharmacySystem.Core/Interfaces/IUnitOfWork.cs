namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for Unit of Work pattern.
/// This interface provides access to all repositories and manages transactions.
/// </summary>
public interface IUnitOfWork
{
    IMedicineRepository Medicines { get; }
    IUserRepository Users { get; }
    ISupplierRepository Suppliers { get; }
    IPurchaseInvoiceRepository PurchaseInvoices { get; }
    ISaleInvoiceRepository SaleInvoices { get; }
    ISalesReturnRepository SalesReturns { get; }
    IInventoryMovementRepository InventoryMovements { get; }
    ICategoryRepository Categories { get; }
    IExpenseRepository Expenses { get; }
    IAlertRepository Alerts { get; }
    IMedicineBatchRepository MedicineBatches { get; }
    IPurchaseInvoiceDetailRepository PurchaseInvoiceDetails { get; }
    IPurchaseReturnDetailRepository PurchaseReturnDetails { get; }
    ISaleInvoiceDetailRepository SaleInvoiceDetails { get; }
    ISalesReturnDetailRepository SalesReturnDetails { get; }
    IPurchaseReturnRepository PurchaseReturns { get; }
    IFinancialRepository Financials { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
