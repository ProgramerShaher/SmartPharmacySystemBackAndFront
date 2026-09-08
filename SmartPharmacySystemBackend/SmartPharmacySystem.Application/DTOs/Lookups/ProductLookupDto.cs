namespace SmartPharmacySystem.Application.DTOs.Lookups
{
    public class ProductLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ScientificName { get; set; }
        public string? DefaultBarcode { get; set; }
        public string? InternalCode { get; set; }
        public decimal DefaultSalePrice { get; set; }
        public decimal DefaultPurchasePrice { get; set; }
        public string? BaseUnitName { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
        public List<MedicineUnitLookupDto> Units { get; set; } = new();
    }

    public class MedicineUnitLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ConversionFactor { get; set; }
        public decimal DefaultSalePrice { get; set; }
        public decimal DefaultPurchasePrice { get; set; }
        public string? Barcode { get; set; }
    }
}
