export enum QuotationStatus {
  Draft = 0,
  Sent = 1,
  Accepted = 2,
  Rejected = 3,
  Expired = 4,
  ConvertedToInvoice = 5
}

export interface QuotationDetail {
  id: number;
  quotationId: number;
  medicineId: number;
  medicineName: string;
  medicineCode?: string;
  barcode?: string;
  saleUnitId?: number;
  unitName?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  subtotal: number;
  total: number;
  notes?: string;
}

export interface Quotation {
  id: number;
  branchId?: number;
  branchName?: string;
  quotationNumber: string;
  quotationDate: string;
  expiryDate?: string;
  customerId?: number;
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  status: QuotationStatus;
  statusName: string;
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  isTaxInclusive: boolean;
  totalDiscount: number;
  totalAmount: number;
  notes?: string;
  termsAndConditions?: string;
  convertedSaleInvoiceId?: number;
  convertedSaleInvoiceNumber?: string;
  convertedAt?: string;
  convertedBy?: number;
  createdAt: string;
  details: QuotationDetail[];
}

export interface CreateQuotationDetailDto {
  medicineId: number;
  saleUnitId?: number;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  discountAmount?: number;
  taxRate: number;
  notes?: string;
}

export interface CreateQuotationDto {
  branchId?: number;
  quotationDate?: string;
  expiryDate?: string;
  customerId?: number;
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  taxRate: number;
  isTaxInclusive: boolean;
  totalDiscount: number;
  notes?: string;
  termsAndConditions?: string;
  details: CreateQuotationDetailDto[];
}

export interface ConvertQuotationToInvoiceDto {
  paymentMethod: number; // 0: Cash, 1: Card, 2: Credit
  paidAmount?: number;
  branchId?: number;
  notes?: string;
}

export interface QuotationPrintData {
  id: number;
  quotationNumber: string;
  quotationDate: string;
  expiryDate?: string;
  statusName: string;
  companyName: string;
  companyTaxNumber?: string;
  companyCommercialRegister?: string;
  companyAddress?: string;
  companyPhone?: string;
  companyEmail?: string;
  companyLogoUrl?: string;
  branchName: string;
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  customerAddress?: string;
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  isTaxInclusive: boolean;
  totalDiscount: number;
  totalAmount: number;
  notes?: string;
  termsAndConditions?: string;
  details: QuotationDetail[];
}
