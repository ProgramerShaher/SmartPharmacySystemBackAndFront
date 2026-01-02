/**
 * User Status Enum - matches backend
 */
export enum UserStatus {
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

/**
 * User interface - matches backend User entity
 */
export interface User {
    id: number;
    fullName: string;
    username: string;
    roleId: number;
    roleName: string;
    status: UserStatus;
    email?: string;
    phoneNumber?: string;
    notes?: string;
    lastLogin?: string;
    createdAt: string;
    createdBy?: number;
    isDeleted: boolean;
}

export interface UserCreateDto {
    username: string;
    password: string;
    confirmPassword: string;
    fullName: string;
    email?: string;
    roleId: number;
    phoneNumber?: string;
    notes?: string;
}

export interface UserUpdateDto {
    username?: string;
    fullName?: string;
    email?: string;
    roleId?: number;
    phoneNumber?: string;
    password?: string;
    confirmPassword?: string;
    notes?: string;
    status?: UserStatus;
}

export interface UserQueryDto {
    search?: string;
    roleId?: number;
    status?: UserStatus;
    page?: number;
    pageSize?: number;
}
