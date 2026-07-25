export interface StockTransferItem {
    id: number;
    stockTransferId: number;
    medicineId: number;
    medicineName: string;
    medicineBarcode: string;
    batchNumber: string;
    expiryDate: string;
    quantityRequested: number;
    quantityDispatched: number;
    quantityReceived?: number;
    variance: number;
}

export interface StockTransfer {
    id: number;
    transferCode: string;
    sourceWarehouseId: number;
    sourceWarehouseName: string;
    sourceBranchName: string;
    destinationWarehouseId: number;
    destinationWarehouseName: string;
    destinationBranchName: string;
    status: number;
    statusName: string;
    statusColor: string;
    transferType: number;
    transferTypeName: string;
    requestedByUserId: number;
    requestedByName: string;
    requestedAt: string;
    approvedByUserId?: number;
    approvedByName?: string;
    approvedAt?: string;
    dispatchedByUserId?: number;
    dispatchedByName?: string;
    dispatchedAt?: string;
    receivedByUserId?: number;
    receivedByName?: string;
    receivedAt?: string;
    notes?: string;
    itemsCount: number;
    totalQuantityRequested: number;
    totalQuantityReceived: number;
    items: StockTransferItem[];
    createdAt: string;
}

export interface CreateStockTransferItem {
    medicineId: number;
    batchNumber: string;
    expiryDate: string;
    quantityRequested: number;
}

export interface CreateStockTransfer {
    sourceWarehouseId: number;
    destinationWarehouseId: number;
    transferType: number;
    notes?: string;
    items: CreateStockTransferItem[];
}

export interface ReceiveStockTransfer {
    items: { stockTransferItemId: number; quantityReceived: number }[];
}
