/**
 * Category Interface and DTOs - Matching Backend
 */
export interface Category {
    id: number;
    name: string;
    description?: string;
    createdAt?: string;
    isDeleted?: boolean;
}

export interface CategoryDto {
    id: number;
    name: string;
    description?: string;
    createdAt: string;
    medicinesCount?: number;
}

export interface CreateCategoryDto {
    name: string;
    description?: string;
}

export interface UpdateCategoryDto {
    id: number;
    name: string;
    description?: string;
}

export interface CategoryQueryDto {
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
}
