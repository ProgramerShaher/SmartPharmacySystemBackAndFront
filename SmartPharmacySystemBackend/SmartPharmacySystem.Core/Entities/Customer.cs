using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities
{
    public class Customer : BaseEntity
    {

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
        
        /// <summary>
        /// معرف الحساب المرتبط في شجرة الحسابات
        /// </summary>
        public int? AccountId { get; set; }


        // Mobile App Auth
        [MaxLength(500)]
        public string? PasswordHash { get; set; }


        // Navigation
        [ForeignKey(nameof(AccountId))]
        public virtual Account? Account { get; set; }
        public virtual ICollection<SaleInvoice> SaleInvoices { get; set; } = new List<SaleInvoice>();
        public virtual ICollection<CustomerReceipt> Receipts { get; set; } = new List<CustomerReceipt>();
    }
}
