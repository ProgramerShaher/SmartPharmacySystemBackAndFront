import { Category } from './category.interface';
import { MedicineBatch } from './medicine-batch.interface';
import { InventoryMovement } from './inventory-movement.interface';

export interface Medicine {
    id: number;
    internalCode: string;
    name: string;
    categoryId?: number;
    categoryName: string;
    manufacturer: string;
    defaultBarcode: string;
    defaultPurchasePrice: number;
    defaultSalePrice: number;
    minAlertQuantity: number;
    soldByUnit: boolean;
    status: string;
    notes: string;
    totalQuantity?: number;
    stock?: number;
    createdAt: string;
    updatedAt?: string;
    isDeleted: boolean;
    category?: Category;
    medicineBatches?: MedicineBatch[];
    inventoryMovements?: InventoryMovement[];
}

export interface MedicineQueryDto {
    search?: string;
    categoryId?: number;
    manufacturer?: string;
    page?: number;
    pageSize?: number;
}
