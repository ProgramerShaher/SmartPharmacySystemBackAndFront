import { PurchaseInvoice } from './purchase-invoice.interface';
import { Medicine } from './medicine.interface';
import { MedicineBatch } from './medicine-batch.interface';

/**
 * Purchase Invoice Detail - Line item entity
 * Matches backend PurchaseInvoiceDetailDto exactly
 */
export interface PurchaseInvoiceDetail {
    id: number;
    purchaseInvoiceId: number;
    purchaseInvoiceNumber: string;
    medicineId: number;
    medicineName: string;
    batchId: number;
    companyBatchNumber: string;
    quantity: number;
    bonusQuantity: number;
    purchasePrice: number;
    salePrice: number;
    total: number;
    trueUnitCost: number; // Calculated: (Qty * Price) / (Qty + Bonus)
    isDeleted: boolean;

    // Batch-related fields
    expiryDate?: string | null; // ISO date string
    daysUntilExpiry: number;
    canSell: boolean;
    expiryStatus: string; // 'Valid' | 'Expiring Soon' | 'Expired'

    // Navigation Properties
    purchaseInvoice?: PurchaseInvoice;
    medicine?: Medicine;
    batch?: MedicineBatch;
}

/**
 * DTO for creating a new purchase invoice detail
 * Matches backend CreatePurchaseInvoiceDetailDto
 */
export interface CreatePurchaseInvoiceDetailDto {
    medicineId: number;
    quantity: number;
    bonusQuantity: number;
    purchasePrice: number;
    salePrice: number;
    expiryDate: string; // ISO date string
    batchBarcode?: string | null;
    companyBatchNumber?: string | null;
}

/**
 * DTO for updating a purchase invoice detail
 */
export interface UpdatePurchaseInvoiceDetailDto {
    id: number;
    medicineId: number;
    quantity: number;
    bonusQuantity: number;
    purchasePrice: number;
    salePrice: number;
    expiryDate: string;
    batchBarcode?: string | null;
    companyBatchNumber?: string | null;
}
