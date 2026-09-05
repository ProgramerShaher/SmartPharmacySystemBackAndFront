import { Category } from './category.interface';
import { MedicineBatch } from './medicine-batch.interface';
import { InventoryMovement } from './inventory-movement.interface';

export interface MedicineUnit {
    id?: number;
    name: string;
    conversionFactor: number;
    defaultPurchasePrice: number;
    defaultSalePrice: number;
    barcode?: string;
}

/**
 * Medicine interface - matches backend Medicine entity 100%
 */
export interface Medicine {
    id: number;
    baseUnitName?: string;
    medicineUnits?: MedicineUnit[];
    internalCode?: string;
    name: string;
    scientificName?: string;
    activeIngredient?: string;
    categoryId?: number;
    categoryName?: string;
    manufacturer?: string;
    defaultBarcode?: string;
    barcode?: string;
    movingAverageCost: number;
    defaultPurchasePrice: number;
    defaultSalePrice: number;
    minAlertQuantity: number;
    reorderLevel: number;
    soldByUnit: boolean;
    status: string; // "Active" | "Inactive"
    imageUrl?: string;
    notes?: string;
    createdAt: string;
    updatedAt?: string;
    isDeleted: boolean;

    // Computed/Additional
    totalQuantity?: number;
    stock?: number;

    // Navigation properties
    category?: Category;
    medicineBatches?: MedicineBatch[];
    inventoryMovements?: InventoryMovement[];
}

/**
 * Create Medicine DTO
 */
export interface CreateMedicineDto {
    internalCode?: string;
    name: string;
    scientificName?: string;
    activeIngredient?: string;
    categoryId?: number;
    manufacturer?: string;
    defaultBarcode?: string;
    defaultPurchasePrice: number;
    defaultSalePrice: number;
    minAlertQuantity: number;
    reorderLevel: number;
    soldByUnit: boolean;
    imageUrl?: string;
    notes?: string;
    baseUnitName?: string;
    medicineUnits?: MedicineUnit[];
}

/**
 * Update Medicine DTO
 */
export interface UpdateMedicineDto {
    id: number;
    internalCode?: string;
    name?: string;
    scientificName?: string;
    activeIngredient?: string;
    categoryId?: number;
    manufacturer?: string;
    defaultBarcode?: string;
    defaultPurchasePrice?: number;
    defaultSalePrice?: number;
    minAlertQuantity?: number;
    reorderLevel?: number;
    soldByUnit?: boolean;
    status?: string;
    imageUrl?: string;
    notes?: string;
    baseUnitName?: string;
    medicineUnits?: MedicineUnit[];
}

/**
 * Medicine Query DTO
 */
export interface MedicineQueryDto {
    search?: string;
    categoryId?: number;
    manufacturer?: string;
    status?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
}

/**
 * Medicine DTO from backend
 */
export interface MedicineDto {
    id: number;
    internalCode?: string;
    name: string;
    scientificName?: string;
    activeIngredient?: string;
    categoryId?: number;
    categoryName?: string;
    manufacturer?: string;
    defaultBarcode?: string;
    movingAverageCost: number;
    defaultPurchasePrice: number;
    defaultSalePrice: number;
    purchasePrice?: number;
    salePrice?: number;
    totalStock?: number;
    minAlertQuantity: number;
    reorderLevel: number;
    soldByUnit: boolean;
    status: string;
    imageUrl?: string;
    notes?: string;
    totalQuantity?: number;
    stock?: number;
    createdAt?: string;
    updatedAt?: string;
    description?: string;
    baseUnitName?: string;
    medicineUnits?: MedicineUnit[];
}

export interface MedicineDetailsDto extends MedicineDto {
    batches: MedicineBatchDetailDto[];
}

export interface MedicineBatchDetailDto {
    id: number;
    batchNumber: string;
    expiryDate: string;
    remainingQuantity: number;
    alertStatus: string;
    statusColor: string;
}
