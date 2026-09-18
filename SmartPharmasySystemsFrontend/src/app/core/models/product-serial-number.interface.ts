export interface ProductSerialNumberDto {
    id: number;
    medicineId: number;
    medicineName?: string | null;
    batchId?: number | null;
    serialNumber: string;
    status: string;
    purchaseInvoiceDetailId?: number | null;
    saleInvoiceDetailId?: number | null;
    customerId?: number | null;
    customerName?: string | null;
    saleDate?: string | null;
    warrantyMonths: number;
    warrantyExpiryDate?: string | null;
    isWarrantyValid: boolean;
    notes?: string | null;
    createdAt: string;
}

export interface RegisterSerialNumbersDto {
    medicineId: number;
    batchId?: number | null;
    purchaseInvoiceDetailId?: number | null;
    serialNumbers: string[];
    warrantyMonths?: number;
    notes?: string | null;
}

export interface WarrantyCheckResultDto {
    serialNumber: string;
    medicineId: number;
    medicineName: string;
    status: string;
    saleDate?: string | null;
    customerId?: number | null;
    customerName?: string | null;
    warrantyMonths: number;
    warrantyExpiryDate?: string | null;
    isUnderWarranty: boolean;
    remainingDays: number;
    notes?: string | null;
}
