import { DocumentStatus } from './stock-movement.enums';
import { PurchaseInvoice } from './purchase-invoice.interface';
import { PurchaseReturn } from './purchase-return.interface';

export interface Supplier {
    id: number;
    name: string;
    contactPerson?: string;
    phoneNumber?: string;
    address?: string;
    email?: string;
    Balance: number;
    notes?: string;
    isActive?: boolean;
    isDeleted?: boolean;
    createdAt: string;
    updatedAt?: string;

    // UI Display Fields (From Backend DTO)
    statusName?: string;
    statusColor?: string;
    statusIcon?: string;
    actionByName?: string;
    actionDate?: string;

    purchaseInvoices?: PurchaseInvoice[];
    purchaseReturns?: PurchaseReturn[];
}

export interface SupplierPayment {
    id: number;
    supplierId: number;
    supplierName?: string;
    amount: number;
    paymentDate: string;
    referenceNo?: string;
    purchaseInvoiceId?: number;
    notes?: string;
    createdBy: number;
    isDeleted: boolean;
}

export interface CreateSupplierPaymentDto {
    supplierId: number;
    amount: number;
    paymentDate: Date | string;
    referenceNo?: string;
    purchaseInvoiceId?: number;
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
    sortBy?: string;
    sortDir?: string;
}
