import { DocumentStatus } from './stock-movement.enums';
import { SaleInvoice } from './sale-invoice.interface';
import { SalesReturnDetail, CreateSalesReturnDetailDto } from './sales-return-detail.interface';

export interface SalesReturn {
    id: number;
    saleInvoiceId: number;
    saleInvoiceNumber: string;
    returnDate: string;
    totalAmount: number;
    totalCost: number;
    totalProfit: number;
    reason: string;
    customerName: string;
    status: DocumentStatus;
    statusName?: string;
    statusColor?: string;
    statusIcon?: string;
    createdBy: number;
    createdAt: string;
    createdByName: string;
    approvedByName?: string;
    cancelledByName?: string;
    // Action Tracking (Last Action)
    actionByName: string;
    actionDate: string;
    items: SalesReturnDetail[]; // DTO uses 'Items'
    saleInvoice?: SaleInvoice;
}

export interface CreateSalesReturnDto {
    saleInvoiceId: number;
    returnDate: string;
    reason: string;
    details: CreateSalesReturnDetailDto[];
    remainingQtyToReturn?: number; // Calculated or validated
}

export interface UpdateSalesReturnDto {
    id: number;
    returnDate: string;
    reason: string;
    details: CreateSalesReturnDetailDto[];
}
