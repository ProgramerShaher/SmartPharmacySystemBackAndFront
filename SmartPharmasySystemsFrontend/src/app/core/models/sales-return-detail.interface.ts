import { SalesReturn } from './sales-return.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface SalesReturnDetail {
    id: number;
    salesReturnId: number;
    returnDate: Date;
    medicineId: number;
    medicineName: string;
    batchId: number;
    companyBatchNumber: string;
    quantity: number;
    salePrice: number;
    unitCost: number;
    totalReturn: number;
    isDeleted: boolean;
    salesReturn?: SalesReturn;
    medicine?: Medicine;
    batch?: MedicineBatch;
}
