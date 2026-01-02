
// Enums
export enum FinancialTransactionType {
    Income = 1,
    Expense = 2,
    Adjustment = 3
}

export enum ReferenceType {
    Manual = 0,
    SaleInvoice = 1,
    PurchaseInvoice = 2,
    Salary = 3,
    Expenses = 4,
    SaleReturn = 5,
    PurchaseReturn = 6
}

// DTOs
export interface PharmacyAccount {
    id: number;
    balance: number;
    lastUpdated: string; // ISO Date
}

export interface FinancialTransaction {
    id: number;
    type: FinancialTransactionType;
    amount: number;
    description: string;
    transactionDate: string; // ISO Date
    relatedInvoiceId?: number | null;
}

export interface FinancialTransactionQueryDto {
    type?: FinancialTransactionType;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
    search?: string;
}

export interface FinancialReport {
    totalIncome: number;
    totalExpense: number;
    netProfit: number;
    startDate: string;
    endDate: string;
    currentBalance: number;
}

export interface GeneralLedger {
    transactionDate: string;
    description: string;
    category: string;
    incoming: number;
    outgoing: number;
    runningBalance: number;
    referenceId: number;
    referenceType: ReferenceType;
    referenceNumber: string;
}

export interface GeneralLedgerQueryDto {
    start?: string;
    end?: string;
    page?: number;
    pageSize?: number;
}

export interface CreateManualAdjustmentRequest {
    amount: number;
    description: string;
}

export interface AnnualFinancialReport {
    categoryName: string;
    totalAmount: number;
    transactionCount: number;
}

export interface FinancialSummary {
    categoryName: string;
    totalAmount: number;
    transactionCount: number;
    percentage: number;
}
