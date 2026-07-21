export interface BranchDto {
    id: number;
    branchCode: string;
    name: string;
    location?: string;
    branchType: string | number;
    branchTypeName: string;
    isActive: boolean;
    warehouseCount: number;
    employeeCount: number;
    createdAt: string;
}

export interface CreateBranchDto {
    branchCode: string;
    name: string;
    location?: string;
    branchType: string | number;
    isActive: boolean;
}

export interface UpdateBranchDto {
    id: number;
    branchCode: string;
    name: string;
    branchType: string | number;
    location?: string;
    isActive: boolean;
}

export interface BranchQueryDto {
    search?: string;
    isActive?: boolean;
    branchType?: string | number;
}
