using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SupplierPayments
{
    public class CreateSupplierPaymentDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [MaxLength(50)]
        public string? ReferenceNo { get; set; }

        public string? Notes { get; set; }
    }

    public class SupplierPaymentDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupplierStatementDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalBalance { get; set; }
        public string Status { get; set; } = string.Empty; // "Debt" vs "Clear"
        public string StatusColor { get; set; } = string.Empty;
        public List<StatementItemDto> Transactions { get; set; } = new();
    }

    public class StatementItemDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Invoice", "Return", "Payment"
        public string Reference { get; set; } = string.Empty; // Invoice # or Ref #
        public decimal Debit { get; set; } // Payments/Returns (Decrease Debt)
        public decimal Credit { get; set; } // Purchases (Increase Debt)
        public decimal RunningBalance { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int? DocumentId { get; set; } // For clickable links
    }
}
