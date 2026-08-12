export interface AutomatedAuditHeaderDto {
  id: number;
  auditCode: string;
  warehouseId: number | null;
  warehouseName: string;
  branchId: number;
  countType: number;
  countTypeName: string;
  auditDate: string;
  createdByUserId: number;
  createdByUserName: string;
  notes: string;
  totalOpeningValue: number;
  totalPurchasesValue: number;
  totalSalesValue: number;
  totalDamagesValue: number;
  totalShortageValue: number;
  items: AutomatedAuditItemDto[];
}

export interface AutomatedAuditItemDto {
  id: number;
  automatedAuditHeaderId: number;
  warehouseId: number;
  warehouseName: string;
  medicineId: number;
  medicineName: string;
  batchId: number | null;
  batchNumber: string | null;
  barcode: string | null;
  expiryDate: string | null;
  openingBalance: number;
  totalPurchases: number;
  totalSales: number;
  totalTransfersIn: number;
  totalTransfersOut: number;
  totalDamages: number;
  totalAdjustments: number;
  totalSalesReturns: number;
  totalPurchaseReturns: number;
  expectedSystemBalance: number;
  actualSystemBalance: number;
  variance: number;
  unitCost: number;
  varianceValue: number;
}

export interface GenerateAuditRequestDto {
  warehouseId: number | null;
  countType: number;
  notes: string;
}

export interface AuditChartDataDto {
  labels: string[];
  salesValues: number[];
  purchasesValues: number[];
  damagesValues: number[];
  shortagesValues: number[];
}
