import { DocumentStatus } from './stock-movement.enums';
import { SaleInvoiceDetail, CreateSaleInvoiceDetailDto } from './sale-invoice-detail.interface';
import { SalesReturn } from './sales-return.interface';

export interface SaleInvoicePaymentDto {
    id: number;
    saleInvoiceId: number;
    paymentMethod: string | number;
    amount: number;
    referenceNumber?: string;
    accountId?: number;
    accountName?: string;
    notes?: string;
    createdAt?: string;
}

export interface CreateSaleInvoicePaymentDto {
    paymentMethod: number; // 1=Cash, 2=Credit, 3=Card/Network, 4=BankTransfer
    amount: number;
    referenceNumber?: string;
    accountId?: number;
    notes?: string;
}

export interface SaleInvoice {
    id: number;
    saleInvoiceNumber: string;
    invoiceDate: string; // Changed from saleInvoiceDate to match DTO
    status: DocumentStatus;
    statusName?: string;
    statusColor?: string;
    statusIcon?: string;
    totalAmount: number;
    subtotal?: number;
    taxRate?: number;
    taxAmount?: number;
    isTaxInclusive?: boolean;
    zatcaQrCode?: string;
    totalDiscount?: number;
    paidAmount?: number;
    remainingAmount?: number;
    totalCost: number;
    totalProfit: number;
    paymentMethod: string | number; // Enum as string or number
    customerId?: number;
    customerName: string;
    isPaid: boolean;
    createdBy: number;
    createdAt: string;
    createdByName: string;
    approvedBy?: number;
    approvedAt?: string;
    approvedByName?: string;
    cancelledBy?: number;
    cancelledAt?: string;
    cancelledByName?: string;
    // Action Tracking (Last Action)
    actionByName?: string;
    actionDate?: string;
    items: SaleInvoiceDetail[]; 
    payments?: SaleInvoicePaymentDto[];
    salesReturns?: SalesReturn[];
    notes?: string;
}

export interface CreateSaleInvoiceDto {
    invoiceDate: string | Date; // Will be serialized to ISO string
    paymentMethod: number; // Enum: 1=Cash, 2=Credit, 3=Card, etc.
    customerId?: number | null; // Null for walk-in
    customerName?: string; // Required for walk-in customers
    paidAmount?: number;
    taxRate?: number;
    isTaxInclusive?: boolean;
    details: CreateSaleInvoiceDetailDto[];
    payments?: CreateSaleInvoicePaymentDto[];
    notes?: string;
}

export interface UpdateSaleInvoiceDto {
    id: number;
    invoiceDate: string | Date;
    paymentMethod: number;
    customerId?: number | null;
    customerName?: string; 
    taxRate?: number;
    isTaxInclusive?: boolean;
    details: CreateSaleInvoiceDetailDto[];
    notes?: string;
}
