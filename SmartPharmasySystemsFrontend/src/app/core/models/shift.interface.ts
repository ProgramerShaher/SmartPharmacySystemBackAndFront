export interface ShiftDto {
    id: number;
    userId: number;
    userName?: string;
    branchId: number;
    startTime: string; // ISO date string
    endTime?: string;
    openingCash: number;
    actualClosingCash?: number;
    expectedClosingCash: number;
    difference: number;
    status: string;
    notes?: string;
}

export interface OpenShiftDto {
    openingCash: number;
    notes?: string;
}

export interface CloseShiftDto {
    actualClosingCash: number;
    notes?: string;
}
