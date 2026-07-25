export interface DamagedGoodsRecord {
    id: number;
    damageCode: string;
    sourceWarehouseId: number;
    warehouseName: string;
    branchName: string;
    medicineId: number;
    medicineName: string;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
    damageType: number;
    damageTypeName: string;
    damageValue: number;
    disposalMethod: number;
    disposalMethodName: string;
    status: number;
    statusName: string;
    statusColor: string;
    approvedByUserId?: number;
    approvedByName?: string;
    approvedAt?: string;
    recordedByUserId: number;
    recordedByName: string;
    recordedAt: string;
}

export interface CreateDamagedGoodsRecord {
    sourceWarehouseId: number;
    medicineId: number;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
    damageType: number;
    disposalMethod: number;
}
