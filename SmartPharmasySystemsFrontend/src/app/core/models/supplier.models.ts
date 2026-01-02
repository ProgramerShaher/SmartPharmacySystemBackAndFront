import { DocumentStatus } from './stock-movement.enums';

export interface Supplier {
    id: number;
    name: string;
    contactPerson?: string;
    phoneNumber?: string;
    address?: string;
    email?: string;
    balance: number;
    notes?: string;
    isActive?: boolean;
    isDeleted?: boolean;
    createdAt: string;
    updatedAt?: string;
    purchaseInvoices?: any[]; // Will be typed properly when PurchaseInvoice interface is available
    purchaseReturns?: any[]; // Will be typed properly when PurchaseReturn interface is available
}

export interface SupplierPayment {
    id: number;
    supplierId: number;
    supplierName?: string;
    amount: number;
    paymentDate: string;
    referenceNo?: string;
    notes?: string;
    createdBy: number;
    isDeleted: boolean;
}

export interface CreateSupplierPaymentDto {
    supplierId: number;
    amount: number;
    paymentDate: Date | string;
    referenceNo?: string;
    notes?: string;
}

export interface StatementItemDto {
    date: string;
    type: string;
    reference: string;
    credit: number; // Increase Debt
    debit: number;  // Decrease Debt (Payment/Return)
    runningBalance: number;
    documentId: number;
    notes: string;
}

export interface SupplierStatement {
    supplierId: number;
    supplierName: string;
    totalBalance: number;
    status: string;
    statusColor: string;
    transactions: StatementItemDto[];
}

export interface SupplierQueryDto {
    search?: string;
    page?: number;
    pageSize?: number;
}
