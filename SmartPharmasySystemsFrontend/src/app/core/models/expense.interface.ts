import { PaymentType } from './enums';

// Main Expense DTO (for display)
export interface ExpenseDto {
    id: number;
    categoryId: number;
    categoryName: string;
    expenseType: string; // Alias for categoryName (backward compatibility)
    amount: number;
    expenseDate: string | Date;
    paymentMethod: PaymentType;
    isPaid: boolean;
    accountId: number;
    accountName: string;
    createdBy: number;
    createdAt: string | Date;
    notes: string;
}

// Create Expense DTO
export interface CreateExpenseDto {
    accountId?: number;
    categoryId: number;
    amount: number;
    expenseDate: string | Date;
    paymentMethod: PaymentType;
    notes: string;
    createdBy?: number;
}

// Update Expense DTO
export interface UpdateExpenseDto {
    id: number;
    categoryId: number;
    amount: number;
    expenseDate: string | Date;
    paymentMethod: PaymentType;
    notes: string;
    accountId?: number;
}

// Expense Query DTO
export interface ExpenseQueryDto {
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: 'asc' | 'desc';
    fromDate?: string;
    toDate?: string;
    categoryId?: number;
    expenseType?: string; // Compatibility
    paymentMethod?: PaymentType;
    isPaid?: boolean;
}

// Backward compatibility alias
export interface Expense extends ExpenseDto { }
