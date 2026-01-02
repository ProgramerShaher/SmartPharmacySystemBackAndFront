import { Medicine } from './medicine.interface';
import { PurchaseInvoiceDetail } from './purchase-invoice-detail.interface';
import { SaleInvoiceDetail } from './sale-invoice-detail.interface';
import { InventoryMovement } from './inventory-movement.interface';
import { Alert } from './alert.interface';

/**
 * Medicine Batch interface - matches backend MedicineBatch entity 100%
 */
export interface MedicineBatch {
    id: number;
    medicineId: number;
    medicineName?: string;
    companyBatchNumber: string;
    expiryDate: string;
    quantity: number;
    remainingQuantity: number;
    unitPurchasePrice: number;
    retailPrice: number;
    purchaseInvoiceId?: number;
    batchBarcode?: string;
    status: string; // "Active" | "Expired" | "Damaged" | "Quarantine"
    storageLocation?: string;
    entryDate: string;
    createdBy: number;
    createdByUserName?: string;
    isDeleted: boolean;
    soldQuantity: number;

    // Computed properties (calculated in frontend)
    isExpired?: boolean;
    isExpiringSoon?: boolean;
    isNearExpiry?: boolean;
    daysUntilExpiry?: number;
    isSellable?: boolean;

    // Navigation properties
    medicine?: Medicine;
    purchaseInvoiceDetails?: PurchaseInvoiceDetail[];
    saleInvoiceDetails?: SaleInvoiceDetail[];
    inventoryMovements?: InventoryMovement[];
    alerts?: Alert[];
}

/**
 * Medicine Batch Response DTO from backend
 */
export interface MedicineBatchResponseDto {
    id: number;
    medicineId: number;
    medicineName: string;
    companyBatchNumber: string;
    expiryDate: string;
    quantity: number;
    remainingQuantity: number;
    unitPurchasePrice: number;
    retailPrice: number;
    batchBarcode?: string;
    status: string;
    storageLocation?: string;
    entryDate: string;
    soldQuantity: number;
    isExpired: boolean;
    isExpiringSoon: boolean;
    isSellable: boolean;
    daysUntilExpiry: number;
}

/**
 * Create Medicine Batch DTO
 */
export interface CreateMedicineBatchDto {
    medicineId: number;
    companyBatchNumber: string;
    expiryDate: string;
    quantity: number;
    unitPurchasePrice: number;
    retailPrice: number;
    batchBarcode?: string;
    storageLocation?: string;
}

/**
 * Update Medicine Batch DTO
 */
export interface UpdateMedicineBatchDto {
    id: number;
    companyBatchNumber?: string;
    expiryDate?: string;
    unitPurchasePrice?: number;
    retailPrice?: number;
    batchBarcode?: string;
    status?: string;
    storageLocation?: string;
}
