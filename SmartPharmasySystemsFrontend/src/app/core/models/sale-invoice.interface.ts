import { DocumentStatus } from './stock-movement.enums';
import { SaleInvoiceDetail } from './sale-invoice-detail.interface';
import { SalesReturn } from './sales-return.interface';

export interface SaleInvoice {
    id: number;
    saleInvoiceNumber: string;
    saleInvoiceDate: string;
    status: DocumentStatus;
    totalAmount: number;
    totalCost: number;
    totalProfit: number;
    paymentMethod: string;
    customerName: string;
    notes?: string;
    createdBy: number;
    createdAt: string;
    isDeleted: boolean;
    saleInvoiceDetails?: SaleInvoiceDetail[];
    salesReturns?: SalesReturn[];
}

export interface SaleInvoiceCreateDto {
    saleInvoiceDate: string;
    customerName: string;
    paymentMethod: string;
    notes?: string;
    createdBy: number;
}

export interface SaleInvoiceUpdateDto {
    saleInvoiceDate?: string;
    customerName?: string;
    paymentMethod?: string;
    notes?: string;
}
