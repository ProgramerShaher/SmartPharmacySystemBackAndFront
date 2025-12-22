export interface Payment {
    id: number;
    paymentDate: string;
    amount: number;
    paymentMethod: 'CASH' | 'CARD' | 'TRANSFER' | 'CHECK';
    referenceNumber?: string;
    invoiceId?: number;
    supplierId?: number; // For purchase payments
    notes?: string;
}

export interface PaymentCreateDto {
    paymentDate: string;
    amount: number;
    paymentMethod: 'CASH' | 'CARD' | 'TRANSFER' | 'CHECK';
    referenceNumber?: string;
    invoiceId?: number;
    supplierId?: number;
    notes?: string;
}

export interface PaymentUpdateDto {
    paymentDate?: string;
    amount?: number;
    paymentMethod?: 'CASH' | 'CARD' | 'TRANSFER' | 'CHECK';
    referenceNumber?: string;
    notes?: string;
}
