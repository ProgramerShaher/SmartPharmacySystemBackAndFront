using System;

namespace SmartPharmacySystem.Application.DTOs.ProductVariants
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal AdditionalPrice { get; set; }
        public decimal StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductVariantDto
    {
        public int MedicineId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal AdditionalPrice { get; set; } = 0m;
        public decimal StockQuantity { get; set; } = 0m;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductVariantDto
    {
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal AdditionalPrice { get; set; }
        public decimal StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}
