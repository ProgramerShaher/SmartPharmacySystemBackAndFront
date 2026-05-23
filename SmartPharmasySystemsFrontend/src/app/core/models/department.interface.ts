export interface DepartmentDto {
    id: number;
    name: string;
    employeeCount: number;
}

export interface CreateDepartmentDto {
    name: string;
}

export interface UpdateDepartmentDto {
    id: number;
    name: string;
}
