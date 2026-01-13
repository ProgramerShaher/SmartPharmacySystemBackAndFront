/**
 * Master Dashboard Statistics Models
 * Matches backend DTOs for comprehensive dashboard data
 */

export interface MasterDashboardStats {
  financialIntelligence: FinancialIntelligence;
  inventoryIntelligence: InventoryIntelligence;
  operationalPulse: OperationalPulse;
}

// ============================================
// 1. Financial Intelligence Hub
// ============================================

export interface FinancialIntelligence {
  /** صافي الربح اليوم */
  todayNetProfit: number;
  /** إجمالي المبيعات المعتمدة اليوم */
  todayApprovedSales: number;
  /** المرتجعات اليوم */
  todayReturns: number;
  /** تكلفة البضاعة المباعة */
  todayCOGS: number;
  /** المصاريف اليوم */
  todayExpenses: number;
  /** رصيد الصندوق */
  cashBalance: number;
  /** رصيد البنك */
  bankBalance: number;
  /** السيولة الإجمالية */
  totalLiquidity: number;
  /** ما لنا عند العملاء */
  customerReceivables: number;
  /** ما علينا للموردين */
  supplierPayables: number;
  /** المديونية الصافية */
  netDebt: number;
  /** تدفق السيولة الداخلة */
  cashFlowInLast30Days: DailyCashFlow[];
  /** تدفق السيولة الخارجة */
  cashFlowOutLast30Days: DailyCashFlow[];
  /** إيرادات الفروع */
  branchRevenues: BranchRevenue[];
}

export interface DailyCashFlow {
  date: Date | string;
  amount: number;
}

export interface BranchRevenue {
  branchName: string;
  revenue: number;
}

// ============================================
// 2. Inventory Intelligence
// ============================================

export interface InventoryIntelligence {
  /** قيمة المخزون الإجمالية */
  totalInventoryValue: number;
  /** توزيع المخزون حسب الموردين */
  inventoryBySupplier: SupplierInventory[];
  /** رادار الصلاحية */
  expiryRadar: ExpiryRadar;
  /** الأصناف الحرجة */
  criticalStockItems: CriticalStockItem[];
}

export interface SupplierInventory {
  supplierId: number;
  supplierName: string;
  inventoryValue: number;
  itemCount: number;
}

export interface ExpiryRadar {
  /** نسبة أقل من 3 أشهر (أحمر) */
  percentageLessThan3Months: number;
  /** نسبة 3-6 أشهر (برتقالي) */
  percentage3To6Months: number;
  /** نسبة 6-12 شهر (أصفر) */
  percentage6To12Months: number;
  /** نسبة أكثر من سنة (أخضر) */
  percentageMoreThan12Months: number;
}

export interface CriticalStockItem {
  medicineId: number;
  medicineName: string;
  currentStock: number;
  reorderPoint: number;
  suggestedOrderQuantity: number;
  preferredSupplierId?: number;
  preferredSupplierName?: string;
}

// ============================================
// 3. Operational Pulse
// ============================================

export interface OperationalPulse {
  /** نبض النظام - آخر 10 عمليات */
  activityStream: ActivityStreamItem[];
  /** أداء الكاشيرات */
  cashierPerformance: CashierPerformance[];
  /** الخريطة الحرارية */
  hourlyHeatMap: HourlyHeatMap[];
}

export interface ActivityStreamItem {
  /** نوع العملية */
  operationType: 'SaleInvoice' | 'PurchaseInvoice' | 'SalesReturn' | 'Payment';
  /** رقم المستند */
  documentNumber: string;
  /** المبلغ */
  amount: number;
  /** اسم المستخدم */
  username: string;
  /** الوقت */
  timestamp: Date | string;
  /** معرف المرجع */
  referenceId: number;
  /** الوصف */
  description: string;
}

export interface CashierPerformance {
  userId: number;
  username: string;
  totalSales: number;
  invoiceCount: number;
  averageInvoiceValue: number;
}

export interface HourlyHeatMap {
  hour: number; // 0-23
  totalSales: number;
  transactionCount: number;
}
