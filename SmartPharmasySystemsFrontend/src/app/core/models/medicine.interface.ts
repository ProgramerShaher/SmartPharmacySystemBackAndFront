import { Category } from './category.interface';
import { MedicineBatch } from './medicine-batch.interface';
import { InventoryMovement } from './inventory-movement.interface';

/**
 * Medicine interface - matches backend Medicine entity 100%
 */
export interface Medicine {
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
    minAlertQuantity: number;
    reorderLevel: number;
    soldByUnit: boolean;
    status: string; // "Active" | "Inactive"
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
    notes?: string;
}

/**
 * Update Medicine DTO
 */
export interface UpdateMedicineDto {
    id: number;
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
    notes?: string;
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
    minAlertQuantity: number;
    reorderLevel: number;
    soldByUnit: boolean;
    status: string;
    notes?: string;
    totalQuantity?: number;
    stock?: number;
    createdAt?: string;
    updatedAt?: string;
    description?: string;
}
