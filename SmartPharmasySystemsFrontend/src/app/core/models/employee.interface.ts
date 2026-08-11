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
    userId: number;
    shift: number;
    shiftName: string;
    shiftStartTime?: string;
    shiftEndTime?: string;
    workingHours: number;
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
    userId: number;
    shift: number;
    shiftStartTime?: string;
    shiftEndTime?: string;
    workingHours: number;

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
    userId: number;
    shift: number;
    shiftStartTime?: string;
    shiftEndTime?: string;
    workingHours: number;

}

export interface EmployeeQueryDto {
    branchId?: number;
    departmentId?: number;
    isActive?: boolean;
    search?: string;
    page?: number;
    pageSize?: number;
}

export interface AttendanceDto {
    id: number;
    employeeId: number;
    employeeName: string;
    workingBranchId: number;
    workingBranchName: string;
    checkIn: string;
    checkOut?: string;
    workedHours?: number;
    shift: number;
    shiftName: string;
    attendanceStatus: number;
    attendanceStatusName: string;
}

export interface CreateAttendanceDto {
    employeeId: number;
    workingBranchId: number;
    checkIn: string;
    checkOut?: string;
    shift: number;
    attendanceStatus: number;
}

export interface SalaryDeductionItemDto {
    id: number;
    deductionType: string;
    description: string;
    amount: number;
}

export interface MonthlySalaryDto {
    id: number;
    employeeId: number;
    employeeName: string;
    employeeCode: string;
    branchId: number;
    branchName: string;
    month: number;
    monthName: string;
    year: number;
    basicSalary: number;
    totalAllowances: number;
    totalBonuses: number;
    totalDeductions: number;
    netSalary: number;
    paymentStatus: number;
    paymentStatusName: string;
    paymentStatusColor: string;
    paidAt?: string;
    deductions: SalaryDeductionItemDto[];
    createdAt: string;
}

export interface CreateSalaryDeductionItemDto {
    deductionType: string;
    description: string;
    amount: number;
}

export interface CreateMonthlySalaryDto {
    employeeId: number;
    branchId: number;
    month: number;
    year: number;
    basicSalary: number;
    totalAllowances: number;
    totalBonuses: number;
    totalDeductions: number;
    deductions?: CreateSalaryDeductionItemDto[];
}

export interface PayrollSummaryDto {
    totalEmployees: number;
    paidCount: number;
    pendingCount: number;
    totalBasicSalary: number;
    totalAllowances: number;
    totalBonuses: number;
    totalDeductions: number;
    totalNetSalary: number;
}
