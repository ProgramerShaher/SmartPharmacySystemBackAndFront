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
    }

    public class UpdateCustomerDto : CreateCustomerDto
    {
        public int Id { get; set; }
    }
}
