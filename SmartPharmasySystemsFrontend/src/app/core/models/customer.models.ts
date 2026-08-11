import { Data } from "@angular/router";

export interface Customer {
    id: number;
    name: string;
    phoneNumber?: string;
    email?: string;
    address?: string;
    balance: number; // Current debt (positive = customer owes us)
    creditLimit?: number;
    notes?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CustomerReceipt {
    id: number;
    customerId: number;
    customerName: string;
    amount: number;
    receiptDate: string;
    paymentMethod: string;
    referenceNo?: string;
    notes?: string;
    createdBy: number;
    createdByName?: string;
    createdAt: string;
}

export interface CreateCustomerReceiptDto {
    customerId: number;
    amount: number;
    receiptDate: string;
    paymentMethod: string;
    referenceNo?: string;
    saleInvoiceId?: number;
    notes?: string;
}

export interface CustomerTransaction {
    date: string;
    type: string;
    reference: string;
    debit: number;
    credit: number;
    transactionDate: string;
    runningBalance: number;
    notes?: string;
}

export interface CustomerStatement {
    customerId: number;
    customerName: string;
    currentBalance: number;
    items: CustomerTransaction[];
}

export interface CustomerQueryDto {
    search?: string;
    hasDebt?: boolean; // Filter customers with balance > 0
    page?: number;
    pageSize?: number;
}

export interface CustomerStatistics {
    totalDebt: number;
    activeCustomersCount: number;
    inactiveCustomersCount: number;
    highDebtCustomersCount: number; // > 5000
    lowDebtCount: number;    // 0 - 1000
    mediumDebtCount: number; // 1000 - 5000
    highDebtDistributionCount: number; // > 5000
}
