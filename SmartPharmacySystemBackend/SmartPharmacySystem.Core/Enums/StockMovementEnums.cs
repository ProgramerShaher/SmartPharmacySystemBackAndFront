namespace SmartPharmacySystem.Core.Enums;

public enum StockMovementType
{
    Purchase = 1,          // Addition from Purchase Invoice
    Sale = 2,              // Deduction from Sale Invoice
    PurchaseReturn = 3,    // Deduction (Returning to Supplier)
    SalesReturn = 4,       // Addition (Customer returning item)
    Adjustment = 5,        // Manual Adjustment (Stocktaking)
    Damage = 6,            // Manual Deduction (Damaged/Lost)
    Expiry = 7,            // Manual Deduction (Expired)
    TransferOut = 8,       // Deduction (Transferring to another branch)
    TransferIn = 9         // Addition (Receiving from another branch)
}

public enum ReferenceType
{
    PurchaseInvoice = 1,
    SaleInvoice = 2,
    PurchaseReturn = 3,
    SalesReturn = 4,
    Manual = 5,
    Expense = 6,
    OpeningBalance = 7,
    ManualAdjustment = 8,
    SupplierPayment = 9,
    CustomerReceipt = 10,
    BranchTransfer = 11
}
