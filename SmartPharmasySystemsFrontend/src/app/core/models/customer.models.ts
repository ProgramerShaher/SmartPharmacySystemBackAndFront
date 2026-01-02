export interface Customer {
    id: number;
    name: string;
    phone?: string;
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
    receiptNumber: string;
    customerId: number;
    customerName: string;
    amount: number;
    paymentDate: string;
    paymentMethod: 'Cash' | 'BankTransfer' | 'Check';
    referenceNumber?: string; // Check number or transfer reference
    notes?: string;
    createdBy: number;
    createdByName?: string;
    createdAt: string;
}

export interface CreateCustomerReceiptDto {
    customerId: number;
    amount: number;
    paymentDate: string;
    paymentMethod: 'Cash' | 'BankTransfer' | 'Check';
    referenceNumber?: string;
    notes?: string;
}

export interface CustomerTransaction {
    id: number;
    transactionDate: string;
    type: 'SaleInvoice' | 'Receipt' | 'Return';
    referenceNumber: string;
    description: string;
    debit: number;  // Increases customer debt
    credit: number; // Decreases customer debt (payments)
    runningBalance: number;
}

export interface CustomerStatement {
    customer: Customer;
    transactions: CustomerTransaction[];
    summary: {
        totalSales: number;
        totalReceipts: number;
        totalReturns: number;
        currentBalance: number;
    };
}

export interface CustomerQueryDto {
    search?: string;
    hasDebt?: boolean; // Filter customers with balance > 0
    page?: number;
    pageSize?: number;
}
