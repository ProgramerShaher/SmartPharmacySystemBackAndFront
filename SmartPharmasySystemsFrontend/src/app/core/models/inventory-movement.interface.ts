import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';
import { StockMovementType, ReferenceType } from './stock-movement.enums';

/**
 * Inventory Movement - matches backend InventoryMovement entity
 */
export interface InventoryMovement {
    id: number;
    medicineId: number;
    batchId?: number | null;
    movementType: StockMovementType;
    referenceType: ReferenceType;
    quantity: number;
    date: string; // ISO date string
    referenceId: number;
    referenceNumber: string;
    createdBy: number;
    notes: string;

    // Navigation properties
    medicine?: Medicine;
    batch?: MedicineBatch;
}

/**
 * Stock Movement DTO - matches backend StockMovementDto
 */
export interface StockMovementDto {
    id: number;
    medicineId: number;
    medicineName: string;
    batchId?: number | null;
    batchNumber?: string;  // Changed from companyBatchNumber
    movementType: StockMovementType;
    movementTypeLabel: string;
    referenceType: ReferenceType;
    referenceTypeLabel: string;
    quantity: number;
    date: string;
    referenceId: number;
    referenceNumber: string;
    createdBy: number;
    createdByName?: string;  // This already exists
    notes: string;
}

/**
 * Stock Card DTO - for displaying stock ledger
 */
export interface StockCardDto {
    id: number;
    date: string;
    movementType: StockMovementType;
    movementTypeLabel: string;
    referenceNumber: string;
    quantityIn: number;
    quantityOut: number;
    balance: number;
    notes: string;
}

/**
 * Create Manual Movement DTO
 */
export interface CreateManualMovementDto {
    medicineId: number;
    batchId?: number | null;
    movementType: StockMovementType; // Only Adjustment, Damage, or Expiry
    quantity: number;
    notes: string; // Required for manual movements
}

/**
 * Query DTO for filtering movements
 */
export interface StockMovementQueryDto {
    search?: string;
    medicineId?: number;
    batchId?: number;
    movementType?: StockMovementType;
    referenceType?: ReferenceType;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
}
