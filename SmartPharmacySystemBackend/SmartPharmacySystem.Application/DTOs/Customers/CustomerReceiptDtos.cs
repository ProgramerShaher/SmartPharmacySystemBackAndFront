using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Customers
{
    public class CustomerReceiptDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? ReferenceNo { get; set; }
        public PaymentType PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public bool IsCancelled { get; set; }
    }

    public class CreateCustomerReceiptDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public string? ReferenceNo { get; set; }
        public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;
        public string? Notes { get; set; }
    }

    public class CustomerStatementDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public List<CustomerStatementItemDto> Items { get; set; } = new();
    }

    public class CustomerStatementItemDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "فاتورة مبيعات", "مرتجع مبيعات", "سند قبض"
        public string Reference { get; set; } = string.Empty;
        public decimal Debit { get; set; }  // مدين (زيادة الدين - فاتورة)
        public decimal Credit { get; set; } // دائن (نقص الدين - قبض/مرتجع)
        public decimal RunningBalance { get; set; }
        public string? Notes { get; set; }
    }
}
