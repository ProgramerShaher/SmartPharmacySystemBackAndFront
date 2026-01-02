// Expense Category interfaces matching backend DTOs

export interface ExpenseCategoryDto {
  id: number;
  name: string;
  description?: string;
  createdAt: string | Date;
}

export interface CreateExpenseCategoryDto {
  name: string;
  description?: string;
}

export interface UpdateExpenseCategoryDto {
  id: number;
  name: string;
  description?: string;
}

// Backward compatibility alias
export interface ExpenseCategory extends ExpenseCategoryDto {}
