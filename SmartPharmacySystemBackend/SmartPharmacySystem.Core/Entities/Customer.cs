using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditLimit { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Mobile App Auth
        [MaxLength(500)]
        public string? PasswordHash { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation
        public virtual ICollection<SaleInvoice> SaleInvoices { get; set; } = new List<SaleInvoice>();
        public virtual ICollection<CustomerReceipt> Receipts { get; set; } = new List<CustomerReceipt>();
    }
}
