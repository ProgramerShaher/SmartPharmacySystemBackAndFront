import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';
import { StockMovementType, ReferenceType } from './stock-movement.enums';

export interface InventoryMovement {
    id: number;
    medicineId: number;
    batchId?: number;
    movementType: StockMovementType;
    referenceType: ReferenceType;
    sourceDocumentId?: number;
    referenceNumber: string;
    quantity: number;
    balanceAfter: number;
    date: Date;
    createdBy: number;
    notes: string;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

export interface InventoryMovementQueryDto {
    search?: string;
    medicineId?: number;
    batchId?: number;
    movementType?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
}
