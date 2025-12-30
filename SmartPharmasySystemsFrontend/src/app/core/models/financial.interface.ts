import { ReferenceType } from './stock-movement.enums';

export enum FinancialTransactionType {
    Income = 1,
    Expense = 2
}

export interface PharmacyAccount {
    id: number;
    balance: number;
    lastUpdated: string;
}

export interface FinancialTransaction {
    id: number;
    type: FinancialTransactionType;
    amount: number;
    description: string;
    transactionDate: string;
    relatedInvoiceId?: number;
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

export interface FinancialReport {
    totalIncome: number;
    totalExpense: number;
    netProfit: number;
    startDate: string;
    endDate: string;
    currentBalance: number;
}

export interface FinancialTransactionQuery {
    type?: FinancialTransactionType;
    startDate?: string;
    endDate?: string;
    page: number;
    pageSize: number;
}
