import { Supplier } from './supplier.interface';
import { PurchaseInvoiceDetail } from './purchase-invoice-detail.interface';
import { DocumentStatus } from './stock-movement.enums';

export interface PurchaseInvoice {
    id: number;
    purchaseInvoiceNumber: string;
    supplierId: number;
    supplierName: string;
    supplierInvoiceNumber: string;
    purchaseDate: string;
    totalAmount: number;
    paymentMethod: string;
    status: DocumentStatus;
    notes: string;
    createdBy: number;
    createdAt: string;
    isDeleted: boolean;
    supplier?: Supplier;
    purchaseInvoiceDetails?: PurchaseInvoiceDetail[];
}

export interface PurchaseInvoiceCreateDto {
    supplierId: number;
    supplierInvoiceNumber?: string;
    purchaseDate: string;
    paymentMethod: string;
    notes?: string;
    createdBy: number;
}

export interface PurchaseInvoiceUpdateDto {
    supplierId?: number;
    supplierInvoiceNumber?: string;
    purchaseDate?: string;
    totalAmount?: number;
    paymentMethod?: string;
    notes?: string;
}
