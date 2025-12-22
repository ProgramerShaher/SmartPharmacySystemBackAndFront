import { PurchaseReturn } from './purchase-return.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

export interface PurchaseReturnDetail {
    id: number;
    purchaseReturnId: number;
    purchaseReturnNumber: string;
    medicineId: number;
    medicineName: string;
    batchId: number;
    companyBatchNumber: string;
    quantity: number;
    purchasePrice: number;
    totalReturn: number;
    isDeleted: boolean;
    purchaseReturn?: PurchaseReturn;
    medicine?: Medicine;
    batch?: MedicineBatch;
}
