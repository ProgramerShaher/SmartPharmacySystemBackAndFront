import { SaleInvoice } from './sale-invoice.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface SaleInvoiceDetail {
    id: number;
    saleInvoiceId: number;
    medicineId: number;
    medicineName?: string; // DTO populate
    batchId: number;
    companyBatchNumber?: string; // DTO populate
    quantity: number;
    salePrice: number;
    unitCost: number;
    totalLineAmount: number;
    totalCost: number;
    profit: number;
    remainingQtyToReturn: number; // Critical for Returns
    saleInvoice?: SaleInvoice;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

export interface CreateSaleInvoiceDetailDto {
    medicineId: number;
    batchId?: number; // Optional, 0/null means FEFO auto-pick
    quantity: number;
    salePrice: number; 
    // UnitCost not sent from frontend, determined by backend
}
