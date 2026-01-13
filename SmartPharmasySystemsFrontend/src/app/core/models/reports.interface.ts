/**
 * واجهات التقارير المركزية
 * Central Reports Interfaces
 */

// ===================== كشف الحساب الموحد - Unified Statement =====================

/**
 * كشف الحساب الموحد للعملاء والموردين
 */
export interface UnifiedStatement {
  entityId: number;
  entityType: string; // "Customer" | "Supplier"
  entityName: string;
  phoneNumber?: string;
  address?: string;
  openingBalance: number;
  currentBalance: number;
  totalDebit: number;
  totalCredit: number;
  fromDate?: string;
  toDate?: string;
  generatedAt: string;
  accountStatus: string;
  statusColor: string;
  lines: StatementLine[];
}

/**
 * بند واحد في كشف الحساب
 */
export interface StatementLine {
  transactionDate: string;
  referenceType: string;
  referenceNumber: string;
  referenceId?: number;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
  createdByUserName?: string;
  createdAt: string;
  rowColor: string;
}

/**
 * استعلام كشف الحساب
 */
export interface StatementQuery {
  entityType: string;
  entityId: number;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

// ===================== تقرير صافي الأرباح - Net Profit Report =====================

/**
 * تقرير صافي الأرباح الدوري
 */
export interface NetProfitReport {
  fromDate: string;
  toDate: string;
  generatedAt: string;

  // الإيرادات
  grossSales: number;
  salesInvoiceCount: number;
  salesReturns: number;
  salesDiscounts: number;
  netSales: number;

  // تكلفة البضاعة المباعة
  costOfGoodsSold: number;
  grossProfit: number;
  grossProfitMargin: number;

  // المصروفات التشغيلية
  totalExpenses: number;
  expensesByCategory: ExpenseBreakdown[];

  // صافي الربح
  netProfit: number;
  netProfitMargin: number;
  profitStatus: string;
  statusColor: string;

  // ملخص إضافي
  totalPurchases: number;
  purchaseReturns: number;
  netPurchases: number;
}

/**
 * تفصيل المصروفات حسب الفئة
 */
export interface ExpenseBreakdown {
  categoryId: number;
  categoryName: string;
  amount: number;
  percentage: number;
  color: string;
}

/**
 * استعلام تقرير صافي الأرباح
 */
export interface NetProfitQuery {
  fromDate: string;
  toDate: string;
  includeExpenseDetails?: boolean;
}

// ===================== تقييم المخزون - Inventory Valuation =====================

/**
 * تقرير تقييم المخزون الذري
 */
export interface InventoryValuation {
  reportDate: string;
  totalCapital: number;
  totalRetailValue: number;
  potentialProfit: number;
  potentialProfitMargin: number;
  totalBatches: number;
  activeBatches: number;
  expiredBatches: number;
  expiringSoonBatches: number;
  expiredStockValue: number;
  totalQuantity: number;
  batches: BatchValuation[];
}

/**
 * تقييم دفعة واحدة
 */
export interface BatchValuation {
  batchId: number;
  medicineId: number;
  medicineName: string;
  scientificName?: string;
  companyBatchNumber: string;
  batchBarcode?: string;
  expiryDate: string;
  remainingQuantity: number;
  unitPurchasePrice: number;
  retailPrice: number;
  batchCapital: number;
  batchRetailValue: number;
  batchProfit: number;
  daysUntilExpiry: number;
  expiryStatus: string;
  expiryStatusColor: string;
  purchaseInvoiceId?: number;
  entryDate: string;
  storageLocation?: string;
}

/**
 * استعلام تقييم المخزون
 */
export interface InventoryValuationQuery {
  expiryFilter?: string; // "all" | "active" | "expired" | "expiring"
  medicineId?: number;
  categoryId?: number;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

// ===================== ملخص التقارير - Reports Summary =====================

/**
 * ملخص سريع للتقارير
 */
export interface ReportsSummary {
  totalCustomerDebts: number;
  totalSupplierDebts: number;
  inventoryCapital: number;
  currentMonthNetProfit: number;
  expiringSoonBatches: number;
}

// ===================== تقرير المبيعات اليومية - Daily Sales Report =====================

/**
 * تقرير المبيعات اليومية
 */
export interface DailySalesReport {
  date: string;
  totalSales: number;
  totalCost: number;
  grossProfit: number;
  profitMargin: number;
  invoiceCount: number;
  itemsSold: number;
  averageInvoiceValue: number;
  cashSales: number;
  creditSales: number;
  salesByHour: HourlySales[];
  topSellingItems: TopSellingItem[];
  generatedAt: string;
}

export interface HourlySales {
  hour: number;
  amount: number;
  invoiceCount: number;
}

export interface TopSellingItem {
  medicineId: number;
  medicineName: string;
  quantitySold: number;
  revenue: number;
}

// ===================== تقرير الأدوية الأكثر مبيعاً - Best Selling =====================

/**
 * تقرير الأدوية الأكثر مبيعاً
 */
export interface BestSellingMedicinesReport {
  fromDate: string;
  toDate: string;
  totalMedicinesSold: number;
  totalRevenue: number;
  medicines: BestSellingMedicine[];
  generatedAt: string;
}

export interface BestSellingMedicine {
  rank: number;
  medicineId: number;
  medicineName: string;
  scientificName?: string;
  categoryName?: string;
  quantitySold: number;
  totalRevenue: number;
  totalProfit: number;
  profitMargin: number;
  averageSellingPrice: number;
  invoiceCount: number;
  lastSaleDate?: string;
}

// ===================== تقرير ديون العملاء - Customer Debts Report =====================

/**
 * تقرير ديون العملاء
 */
export interface CustomerDebtsReport {
  totalReceivable: number;
  totalPayable: number;
  netBalance: number;
  debtorCount: number;
  creditorCount: number;
  customers: CustomerDebt[];
  generatedAt: string;
}

export interface CustomerDebt {
  customerId: number;
  customerName: string;
  phoneNumber?: string;
  address?: string;
  balance: number;
  balanceType: string;
  statusColor: string;
  totalPurchases: number;
  totalPayments: number;
  outstandingInvoices: number;
  lastInvoiceDate?: string;
  lastPaymentDate?: string;
  daysOverdue: number;
}

// ===================== تقرير ديون الموردين - Supplier Debts Report =====================

/**
 * تقرير ديون الموردين
 */
export interface SupplierDebtsReport {
  totalPayable: number;
  totalReceivable: number;
  netBalance: number;
  creditorCount: number;
  debtorCount: number;
  suppliers: SupplierDebt[];
  generatedAt: string;
}

export interface SupplierDebt {
  supplierId: number;
  supplierName: string;
  phoneNumber?: string;
  address?: string;
  contactPerson?: string;
  balance: number;
  balanceType: string;
  statusColor: string;
  totalPurchases: number;
  totalPayments: number;
  outstandingInvoices: number;
  lastPurchaseDate?: string;
  lastPaymentDate?: string;
  daysOverdue: number;
}
