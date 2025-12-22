export interface User {
    id: number;
    username: string;
    password?: string;
    fullName: string;
    email?: string;
    role: string;
    phoneNumber?: string;
    notes?: string;
    isActive: boolean;
    isDeleted: boolean;
    createdAt: Date;
    updatedAt?: Date;
    lastLogin?: Date;
    createdBy?: number;
}

export interface UserCreateDto {
    username: string;
    password?: string;
    confirmPassword?: string;
    fullName: string;
    email?: string;
    role: string;
    phoneNumber?: string;
    notes?: string;
    isActive?: boolean;
}

export interface UserUpdateDto {
    username?: string;
    fullName?: string;
    email?: string;
    role?: string;
    phoneNumber?: string;
    password?: string;
    confirmPassword?: string;
    notes?: string;
    isActive?: boolean;
    isDeleted?: boolean;
}

export interface UserQueryDto {
    search?: string;
    role?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
}
