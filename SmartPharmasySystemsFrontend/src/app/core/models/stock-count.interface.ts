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

export enum StockCountFrequency {
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}

export interface StockCountScheduleDto {
    id: number;
    warehouseId: number;
    warehouseName: string;
    branchId: number;
    branchName: string;
    frequency: StockCountFrequency;
    frequencyLabel: string;
    nextRunDate: string;
    lastRunDate?: string | null;
    isActive: boolean;
    notes?: string;
}

export interface CreateStockCountScheduleDto {
    warehouseId: number;
    frequency: StockCountFrequency;
    nextRunDate: string;
    notes?: string;
}

export interface UpdateStockCountScheduleDto {
    frequency: StockCountFrequency;
    nextRunDate: string;
    isActive: boolean;
    notes?: string;
}
