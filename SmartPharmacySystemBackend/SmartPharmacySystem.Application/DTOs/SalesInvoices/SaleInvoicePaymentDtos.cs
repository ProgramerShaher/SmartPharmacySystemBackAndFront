using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices
{
    public class SaleInvoicePaymentDto
    {
        public int Id { get; set; }
        public int SaleInvoiceId { get; set; }
        public PaymentType PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSaleInvoicePaymentDto
    {
        public PaymentType PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? AccountId { get; set; }
        public string? Notes { get; set; }
    }
}
