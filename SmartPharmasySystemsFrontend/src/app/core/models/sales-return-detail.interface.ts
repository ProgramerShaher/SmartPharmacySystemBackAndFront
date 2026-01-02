import { SalesReturn } from './sales-return.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface SalesReturnDetail {
    id: number;
    salesReturnId: number;
    medicineId: number;
    medicineName?: string;
    batchId: number;
    companyBatchNumber?: string;
    quantity: number;
    salePrice: number;
    unitCost: number;
    totalLineAmount: number;
    totalCost: number;
    profit: number;
    salesReturn?: SalesReturn;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

export interface CreateSalesReturnDetailDto {
    medicineId: number;
    batchId: number;
    quantity: number;
}
