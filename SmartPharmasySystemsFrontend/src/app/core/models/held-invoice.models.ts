export interface HeldInvoiceDto {
    id: number;
    holdReference: string;
    customerId?: number | null;
    customerName?: string | null;
    userShiftId?: number | null;
    totalAmount: number;
    totalDiscount: number;
    itemsCount: number;
    cartJson: string;
    notes?: string | null;
    createdAt: string;
    createdBy: number;
}

export interface CreateHeldInvoiceDto {
    holdReference?: string | null;
    customerId?: number | null;
    customerName?: string | null;
    userShiftId?: number | null;
    totalAmount: number;
    totalDiscount: number;
    itemsCount: number;
    cartJson: string;
    notes?: string | null;
}
