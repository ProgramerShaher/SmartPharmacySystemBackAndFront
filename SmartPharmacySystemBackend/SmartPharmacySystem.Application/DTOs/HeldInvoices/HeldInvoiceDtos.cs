using System;

namespace SmartPharmacySystem.Application.DTOs.HeldInvoices
{
    public class HeldInvoiceDto
    {
        public int Id { get; set; }
        public string HoldReference { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? UserShiftId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public int ItemsCount { get; set; }
        public string CartJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }

    public class CreateHeldInvoiceDto
    {
        public string? HoldReference { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? UserShiftId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public int ItemsCount { get; set; }
        public string CartJson { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
