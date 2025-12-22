// src/app/core/models/supplier.interface.ts
import { PurchaseInvoice } from './purchase-invoice.interface';
import { PurchaseReturn } from './purchase-return.interface';

export interface Supplier {
    id: number;
    name: string;
    contactPerson: string;
    phoneNumber: string;
    address: string;
    email: string;
    notes: string;
    balance: number;
    createdAt: string; // أو Date إذا كان تاريخاً
    updatedAt?: string; // جعله اختيارياً
    isDeleted: boolean;
    purchaseInvoices?: PurchaseInvoice[]; // جعله اختيارياً
    purchaseReturns?: PurchaseReturn[]; // جعله اختيارياً
}

export interface SupplierQueryDto {
    search?: string;
    page?: number;
    pageSize?: number;
}