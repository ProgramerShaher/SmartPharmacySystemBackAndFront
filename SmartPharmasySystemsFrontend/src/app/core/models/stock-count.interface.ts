export enum StockCountType {
    Daily = 1,
    Monthly = 2,
    BiAnnual = 3,
    Annual = 4
}

export enum StockCountStatus {
    Draft = 1,
    InProgress = 2,
    PendingApproval = 3,
    Approved = 4,
    Closed = 5
}

export interface StockCountHeaderDto {
    id: number;
    countCode: string;
    warehouseId: number;
    warehouseName: string;
    countType: StockCountType;
    status: StockCountStatus;
    startedAt: string;
    finishedAt?: string | null;
    approvedByUserId?: number | null;
    approvedByName?: string | null;
    notes: string;
}

export interface StockCountItemDto {
    stockCountHeaderId: number;
    medicineId: number;
    medicineName: string;
    batchNumber: string;
    expiryDate: string;
    systemQuantity: number;
    physicalQuantity?: number | null;
    variance: number;
    varianceValue: number;
    varianceReason?: string | null;
}

export interface CreateStockCountHeaderDto {
    warehouseId: number;
    countType: StockCountType;
    notes?: string;
}

export interface CreateStockCountItemDto {
    medicineId: number;
    batchNumber: string;
    physicalQuantity: number;
    varianceReason?: string;
}

export interface UpdateStockCountItemDto {
    medicineId: number;
    batchNumber: string;
    physicalQuantity: number;
    varianceReason?: string;
}
