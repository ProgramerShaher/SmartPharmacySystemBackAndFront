export interface Expense {
    id: number;
    expenseType: string;
    description: string;
    categoryName: string;
    amount: number;
    date: Date;
    expenseDate: string; // Keeping for compatibility as 'unrelated' property if used by UI currently
    paymentMethod: string;
    createdBy: number;
    notes: string;
    createdAt: string; // Keeping unrelated
    isDeleted: boolean;
}

export interface ExpenseQueryDto {
    search?: string;
    expenseType?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
}
