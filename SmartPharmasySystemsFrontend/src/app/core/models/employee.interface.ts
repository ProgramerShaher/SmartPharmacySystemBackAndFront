export interface EmployeeDto {
    id: number;
    employeeCode: string;
    fullName: string;
    nationalId: string;
    branchId: number;
    branchName: string;
    departmentId: number;
    departmentName: string;
    jobTitle: string;
    hireDate: string;
    terminationDate?: string;
    basicSalary: number;
    isActive: boolean;
    totalLoans: number;
    remainingLoans: number;
    createdAt: string;
}

export interface CreateEmployeeDto {
    employeeCode: string;
    fullName: string;
    nationalId: string;
    branchId: number;
    departmentId: number;
    jobTitle: string;
    hireDate: string;
    basicSalary: number;
    isActive: boolean;
}

export interface UpdateEmployeeDto {
    id: number;
    employeeCode: string;
    fullName: string;
    nationalId: string;
    branchId: number;
    departmentId: number;
    jobTitle: string;
    hireDate: string;
    basicSalary: number;
    isActive: boolean;
}

export interface EmployeeQueryDto {
    branchId?: number;
    departmentId?: number;
    isActive?: boolean;
    search?: string;
    page?: number;
    pageSize?: number;
}
