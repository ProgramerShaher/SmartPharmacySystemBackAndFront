import { Supplier } from './supplier.models';
import { PurchaseInvoiceDetail, CreatePurchaseInvoiceDetailDto } from './purchase-invoice-detail.interface';
import { DocumentStatus } from './stock-movement.enums';
import { PaymentType } from './enums';

/**
 * Purchase Invoice - Main entity
 * Matches backend PurchaseInvoiceDto exactly
 */
export interface PurchaseInvoice {
    id: number;
    supplierId: number;
    supplierName: string;
    supplierInvoiceNumber: string;
    purchaseInvoiceNumber: string;
    purchaseDate: string; // ISO Date
    totalAmount: number;
    paymentMethod: PaymentType;
    notes: string;
    createdBy: number;
    createdAt: string; // ISO Date
    createdByName: string;
    approvedByName?: string;
    cancelledByName?: string;
    status: DocumentStatus;

    // Payment Status
    isPaid?: boolean;

    // Status Tracking & Dynamic Colors
    statusName: string;
    statusColor: string;
    statusIcon: string;

    // Action Tracking
    actionByName: string;
    actionDate: string;

    isDeleted: boolean;

    // Details
    items: PurchaseInvoiceDetail[];
}

/**
 * DTO for creating a new purchase invoice
 * Matches backend CreatePurchaseInvoiceDto
 */
export interface CreatePurchaseInvoiceDto {
    supplierId: number;
    supplierInvoiceNumber: string;
    purchaseDate: string; // ISO date string
    paymentMethod: PaymentType;
    notes?: string;
    items: CreatePurchaseInvoiceDetailDto[];
}

/**
 * DTO for updating an existing purchase invoice
 * Matches backend UpdatePurchaseInvoiceDto
 */
export interface UpdatePurchaseInvoiceDto {
    id: number;
    supplierId: number;
    supplierInvoiceNumber: string;
    purchaseDate: string;
    paymentMethod: PaymentType;
    notes?: string;
    items: CreatePurchaseInvoiceDetailDto[];
}

/**
 * Query parameters for filtering purchase invoices
 */
export interface PurchaseInvoiceQueryDto {
    search?: string;
    supplierId?: number;
    status?: DocumentStatus;
    paymentMethod?: PaymentType;
    dateFrom?: string;
    dateTo?: string;
    page?: number;
    pageSize?: number;
}
