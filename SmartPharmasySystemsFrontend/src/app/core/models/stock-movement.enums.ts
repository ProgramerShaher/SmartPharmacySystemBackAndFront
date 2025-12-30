export enum StockMovementType {
    Purchase = 1,
    Sale = 2,
    PurchaseReturn = 3,
    SalesReturn = 4,
    Adjustment = 5,
    Damage = 6,
    Expiry = 7
}

export enum ReferenceType {
    PurchaseInvoice = 1,
    SaleInvoice = 2,
    PurchaseReturn = 3,
    SalesReturn = 4,
    Manual = 5,
    Expense = 6,
    OpeningBalance = 7,
    ManualAdjustment = 8,
    SupplierPayment = 9,
    CustomerReceipt = 10
}

export enum DocumentStatus {
    Draft = 1,
    Approved = 2,
    Cancelled = 3,
    Returned = 4
}
