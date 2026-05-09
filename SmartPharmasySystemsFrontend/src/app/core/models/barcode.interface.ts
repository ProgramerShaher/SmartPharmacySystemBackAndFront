export enum TransactionType {
    Sale = 'Sale',
    Purchase = 'Purchase',
    Return = 'Return'
}

export interface BarcodeQuery {
    barcode: string;
    transactionType: TransactionType;
}

export interface BarcodeResult {
    medicineId: number;
    tradeName: string;
    scientificName?: string;
    activeIngredient?: string;
    storageLocation?: string;
    movingAverageCost: number;
    batchId: number;
    batchNumber: string;
    salePrice: number;
    availableQuantity: number;
    expiryDate: string;
    isNearExpiry: boolean;
    alternatives: MedicineAlternative[];
}

export interface MedicineAlternative {
    medicineId: number;
    name: string;
    salePrice: number;
    availableQuantity: number;
}
