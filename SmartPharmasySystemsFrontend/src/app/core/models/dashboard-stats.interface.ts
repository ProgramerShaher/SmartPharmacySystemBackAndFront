/**
 * Sales Dashboard Statistics DTO
 * Matches backend SalesDashboardStatsDto
 */
export interface SalesDashboardStats {
    todayTotalSales: number;
    todayNetProfit: number;
    customerDebts: number;
    todayReturnsAmount: number;
    returnRate: number;
    cashPercentage: number;
    last7DaysSales: number[];
}

/**
 * Purchases Dashboard Statistics DTO
 * Matches backend PurchasesDashboardStatsDto
 */
export interface PurchasesDashboardStats {
    monthlyTotalPurchases: number;
    supplierDebts: number;
    overdueCount: number;
    monthlyReturnsAmount: number;
    returnRate: number;
    supplierDistribution: SupplierDistributionItem[];
    last7DaysPurchases: number[];
}

export interface SupplierDistributionItem {
    supplierName: string;
    totalAmount: number;
}
