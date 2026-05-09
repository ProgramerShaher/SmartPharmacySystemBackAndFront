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
    batchNumber?: string;
    movementType: StockMovementType;
    movementTypeLabel: string;
    referenceType: ReferenceType;
    referenceTypeLabel: string;
    quantity: number;
    date: string;
    referenceId: number;
    referenceNumber: string;
    createdBy: number;
    createdByName?: string;
    notes: string;
    financialDescription?: string;
}

export interface StockMovementSummary {
    totalStockValue: number;
    nearExpiryCount: number;
    lowStockCount: number;
    todayMovements: number;
    last30DaysTrend: StockMovementTrend[];
    categoryDistribution: StockCategoryDistribution[];
}

export interface StockMovementTrend {
    date: string;
    additions: number;
    deductions: number;
}

export interface StockCategoryDistribution {
    categoryName: string;
    quantity: number;
    value: number;
}

/**
 * Stock Card DTO - for displaying stock ledger (Matches backend StockCardDto)
 */
export interface StockCardDto {
    date: string;
    movementType: string;
    referenceNumber: string;
    quantityChange: number;
    runningBalance: number;
    notes: string;
}

/**
 * Create Manual Movement DTO
 */
export interface CreateManualMovementDto {
    medicineId: number;
    batchId?: number | null;
    movementType: StockMovementType;
    quantity: number;
    notes: string;
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
    createdBy?: number;
    page?: number;
    pageSize?: number;
}
