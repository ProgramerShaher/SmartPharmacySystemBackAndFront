// Enums matching backend enums

export enum PaymentType {
    Cash = 1,
    Credit = 2
}

export enum FinancialTransactionType {
    Income = 1,
    Expense = 2
}

export enum InvoiceStatus {
    Draft = 1,
    Approved = 2,
    Cancelled = 3
}

export enum AlertType {
    ExpiryWarning = 1,
    StockLow = 2,
    StockOut = 3,
    SystemNotification = 4
}

export enum AlertPriority {
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

// Re-export from stock-movement.enums for convenience
export { ReferenceType, DocumentStatus, StockMovementType } from './stock-movement.enums';
