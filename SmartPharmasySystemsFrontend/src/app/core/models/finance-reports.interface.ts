export interface RevenueReport {
    period: string; // e.g., '2023-10' or date range
    totalRevenue: number;
    revenueByPaymentMethod: Record<string, number>;
    dailyRevenue: { date: string; amount: number }[];
}

export interface ExpenseReport {
    period: string;
    totalExpenses: number;
    expensesByCategory: Record<string, number>;
    dailyExpenses: { date: string; amount: number }[];
}

export interface ProfitLossReport {
    period: string;
    totalRevenue: number;
    totalExpenses: number;
    grossProfit: number;
    netProfit: number;
}

export interface CashFlowReport {
    period: string;
    startingBalance: number;
    cashIn: number;
    cashOut: number;
    endingBalance: number;
}
