export interface Role {
    id: number;
    name: string;
    description?: string;
    permissions: string[];
}

export interface RoleCreateDto {
    name: string;
    description?: string;
    permissions: string[];
}

export interface RoleUpdateDto {
    name?: string;
    description?: string;
    permissions?: string[];
}
