import { SaleInvoice } from './sale-invoice.interface';
import { SalesReturnDetail } from './sales-return-detail.interface';

export interface SalesReturn {
    id: number;
    saleInvoiceId: number;
    saleInvoiceNumber: string;
    returnDate: Date;
    totalAmount: number;
    reason: string;
    customerName: string;
    createdBy: number;
    createdAt: Date;
    isDeleted: boolean;
    saleInvoice?: SaleInvoice;
    salesReturnDetails?: SalesReturnDetail[];
}
