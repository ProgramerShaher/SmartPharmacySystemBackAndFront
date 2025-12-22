import { SaleInvoice } from './sale-invoice.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface SaleInvoiceDetail {
    id: number;
    saleInvoiceId: number;
    saleInvoiceDate: string;
    medicineId: number;
    medicineName: string;
    batchId: number;
    companyBatchNumber: string;
    quantity: number;
    salePrice: number;
    unitCost: number;
    totalLineAmount: number;
    totalCost: number;
    profit: number;
    isDeleted: boolean;
    saleInvoice?: SaleInvoice;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

export interface SaleInvoiceDetailCreateDto {
    saleInvoiceId: number;
    medicineId: number;
    batchId?: number;
    quantity: number;
    salePrice: number;
    unitCost: number;
}

export interface SaleInvoiceDetailUpdateDto {
    quantity?: number;
    salePrice?: number;
}
