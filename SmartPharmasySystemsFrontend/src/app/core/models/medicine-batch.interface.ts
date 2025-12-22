import { Medicine } from './medicine.interface';
import { PurchaseInvoiceDetail } from './purchase-invoice-detail.interface';
import { SaleInvoiceDetail } from './sale-invoice-detail.interface';
import { InventoryMovement } from './inventory-movement.interface';
import { Alert } from './alert.interface';

export interface MedicineBatch {
    id: number;
    medicineId: number;
    medicineName: string;
    companyBatchNumber: string;
    expiryDate: string;
    quantity: number;
    remainingQuantity: number;
    soldQuantity: number;
    unitPurchasePrice: number;
    batchBarcode: string;
    status: string;
    storageLocation: string;
    entryDate: string;
    createdBy: number;
    createdByUserName: string | null;
    isDeleted: boolean;

    // UI Calculated Fields
    isExpired: boolean;
    isExpiringSoon: boolean;
    isSellable: boolean;
    daysUntilExpiry: number;

    medicine?: Medicine;
    purchaseInvoiceDetails?: PurchaseInvoiceDetail[];
    saleInvoiceDetails?: SaleInvoiceDetail[];
    inventoryMovements?: InventoryMovement[];
    alerts?: Alert[];
}
