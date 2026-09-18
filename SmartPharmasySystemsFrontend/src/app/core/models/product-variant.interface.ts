export interface ProductVariantDto {
    id: number;
    medicineId: number;
    medicineName?: string | null;
    size?: string | null;
    color?: string | null;
    colorHex?: string | null;
    sku?: string | null;
    barcode?: string | null;
    additionalPrice: number;
    stockQuantity: number;
    isActive: boolean;
    createdAt: string;
}

export interface CreateProductVariantDto {
    medicineId: number;
    size?: string | null;
    color?: string | null;
    colorHex?: string | null;
    sku?: string | null;
    barcode?: string | null;
    additionalPrice?: number;
    stockQuantity?: number;
    isActive?: boolean;
}

export interface UpdateProductVariantDto {
    size?: string | null;
    color?: string | null;
    colorHex?: string | null;
    sku?: string | null;
    barcode?: string | null;
    additionalPrice?: number;
    stockQuantity?: number;
    isActive?: boolean;
}
