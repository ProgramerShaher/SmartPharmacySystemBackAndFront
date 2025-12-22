import { Medicine } from './medicine.interface';

export interface Category {
    id: number;
    name: string;
    description: string;
    color?: string;
    icon?: string;
    createdAt: Date;
    updatedAt?: Date;
    createdBy?: string;
    isDeleted: boolean;
    medicines?: Medicine[];
}

export interface CategoryQueryDto {
    search?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
}
