import { DocumentStatus } from './stock-movement.enums';
import { SaleInvoiceDetail, CreateSaleInvoiceDetailDto } from './sale-invoice-detail.interface';
import { SalesReturn } from './sales-return.interface';

export interface SaleInvoice {
    id: number;
    saleInvoiceNumber: string;
    invoiceDate: string; // Changed from saleInvoiceDate to match DTO
    status: DocumentStatus;
    statusName?: string;
    statusColor?: string;
    statusIcon?: string;
    totalAmount: number;
    totalCost: number;
    totalProfit: number;
    paymentMethod: string; // Enum as string
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
    salesReturns?: SalesReturn[];
}

export interface CreateSaleInvoiceDto {
    invoiceDate: string | Date; // Will be serialized to ISO string
    paymentMethod: number; // Enum: 1=Cash, 2=Credit
    customerId?: number | null; // Null for walk-in
    customerName?: string; // Required for walk-in customers
    details: CreateSaleInvoiceDetailDto[];
    notes?: string;
}

export interface UpdateSaleInvoiceDto {
    id: number;
    invoiceDate: string | Date;
    paymentMethod: number;
    customerId?: number | null;
    customerName?: string; 
    details: CreateSaleInvoiceDetailDto[];
    notes?: string;
}
