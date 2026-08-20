namespace SmartPharmacySystem.Application.DTOs.Customers
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal Balance { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? AccountId { get; set; }

        /// <summary>FK to pricelist linked to this customer.</summary>
        public int? PricelistId { get; set; }
        /// <summary>Name of the linked pricelist (for display).</summary>
        public string? PricelistName { get; set; }
        /// <summary>Global discount % of the linked pricelist (for quick use in POS).</summary>
        public decimal PricelistDiscountPercentage { get; set; }

        public string Status => IsActive ? "نشط" : "متوقف";
        public string StatusColor => IsActive ? "success" : "danger";
        public string DebtStatus => Balance > 0 ? "مديون" : "خالص";
        public string DebtStatusColor => Balance > 0 ? "warning" : "success";
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>Optional: link customer to a price list for automatic discounts in POS.</summary>
        public int? PricelistId { get; set; }
        public string? PricelistName { get; set; }
    }

    public class UpdateCustomerDto : CreateCustomerDto
    {
        public int Id { get; set; }
    }
}
