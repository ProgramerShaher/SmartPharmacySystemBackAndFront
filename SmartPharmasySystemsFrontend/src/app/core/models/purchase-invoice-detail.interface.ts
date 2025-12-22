import { PurchaseInvoice } from './purchase-invoice.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface PurchaseInvoiceDetail {
    id: number;
    purchaseInvoiceId: number;
    purchaseInvoiceNumber: string;
    medicineId: number;
    medicineName: string;
    batchId: number;
    companyBatchNumber: string;
    expiryDate: string; // Required for purchase entry to define batch details
    quantity: number;
    purchasePrice: number;
    total: number;
    isDeleted: boolean;
    purchaseInvoice?: PurchaseInvoice;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

export interface PurchaseInvoiceDetailCreateDto {
    purchaseInvoiceId: number;
    medicineId: number;
    batchId?: number;
    quantity: number;
    purchasePrice: number;
}

export interface PurchaseInvoiceDetailUpdateDto {
    quantity?: number;
    purchasePrice?: number;
    total?: number;
}
