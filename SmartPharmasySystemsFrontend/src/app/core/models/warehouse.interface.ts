import { WarehouseType } from './enums';

export interface WarehouseDto {
    id: number;
    branchId: number;
    branchName: string;
    type: WarehouseType;
    typeName: string;
    name: string;
    createdAt: string;
}

export interface CreateWarehouseDto {
    branchId: number;
    type: WarehouseType;
    name: string;
}

export interface UpdateWarehouseDto {
    branchId?: number;
    type?: WarehouseType;
    name?: string;
}

export interface WarehouseQueryDto {
    branchId?: number;
    type?: WarehouseType;
    search?: string;
}

export interface InventoryStockDto {
    id: number;
    warehouseId: number;
    warehouseName: string;
    medicineId: number;
    medicineName: string;
    medicineBarcode: string;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
    expiryStatus: string;
    expiryStatusColor: string;
    daysUntilExpiry: number;
    storageLocation?: string;
}
