import { PurchaseInvoice } from './purchase-invoice.interface';
import { Supplier } from './supplier.interface';
import { PurchaseReturnDetail } from './purchase-return-detail.interface';

export interface PurchaseReturn {
    id: number;
    purchaseInvoiceId: number;
    purchaseInvoiceNumber: string;
    supplierId: number;
    supplierName: string;
    returnDate: Date;
    totalAmount: number;
    reason: string;
    createdBy: number;
    createdAt: Date;
    isDeleted: boolean;
    purchaseInvoice?: PurchaseInvoice;
    supplier?: Supplier;
    purchaseReturnDetails?: PurchaseReturnDetail[];
}
