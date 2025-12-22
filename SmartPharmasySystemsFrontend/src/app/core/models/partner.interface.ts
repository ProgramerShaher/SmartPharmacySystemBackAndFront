export interface Partner {
    id: number;
    name?: string;
    type?: 'SUPPLIER' | 'CUSTOMER';
    contactPerson?: string;
    email?: string;
    phone?: string;
    address?: string;
    notes?: string;
    createdAt: string;
    updatedAt?: string;
    isDeleted: boolean;
}

export interface PartnerCreateDto {
    name: string;
    type: 'SUPPLIER' | 'CUSTOMER';
    contactPerson?: string;
    email?: string;
    phone?: string;
    address?: string;
    notes?: string;
}

export interface PartnerUpdateDto {
    name?: string;
    type?: 'SUPPLIER' | 'CUSTOMER';
    contactPerson?: string;
    email?: string;
    phone?: string;
    address?: string;
    notes?: string;
}

export interface Contact {
    id: number;
    partnerId: number;
    name: string;
    phone?: string;
    email?: string;
    position?: string;
    isPrimary: boolean;
}
