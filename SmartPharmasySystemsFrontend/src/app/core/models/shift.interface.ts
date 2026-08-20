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
    transferToMainSafe?: boolean;
}

export interface ShiftSummaryDto {
    shiftId: number;
    userName: string;
    startTime: string;
    endTime?: string;
    status: string;
    notes: string;
    openingCash: number;
    totalSalesCash: number;
    totalSalesNetwork: number;
    totalSalesCredit: number;
    invoicesCount: number;
    totalReturnsCash: number;
    totalCustomerReceiptsCash: number;
    totalSupplierPaymentsCash: number;
    totalExpensesCash: number;
    expectedClosingCash: number;
    actualClosingCash?: number;
    difference: number;
}

export interface ShiftInvoiceDto {
    id: number;
    invoiceNumber: string;
    customerName: string;
    paymentMethod: string;
    totalAmount: number;
    createdAt: string;
}

export interface ShiftExpenseDto {
    id: number;
    referenceNumber: string;
    categoryName: string;
    amount: number;
    notes: string;
    createdAt: string;
}

export interface ShiftReturnDto {
    id: number;
    customerName: string;
    totalAmount: number;
    reason: string;
    createdAt: string;
}

export interface ShiftDetailsDto {
    summary: ShiftSummaryDto;
    invoices: ShiftInvoiceDto[];
    expenses: ShiftExpenseDto[];
    returns: ShiftReturnDto[];
}
