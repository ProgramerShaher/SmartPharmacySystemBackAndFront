using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.ProductSerialNumbers
{
    public class ProductSerialNumberDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public int? BatchId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? PurchaseInvoiceDetailId { get; set; }
        public int? SaleInvoiceDetailId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? SaleDate { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public bool IsWarrantyValid { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterSerialNumbersDto
    {
        public int MedicineId { get; set; }
        public int? BatchId { get; set; }
        public int? PurchaseInvoiceDetailId { get; set; }
        public List<string> SerialNumbers { get; set; } = new List<string>();
        public int WarrantyMonths { get; set; } = 12;
        public string? Notes { get; set; }
    }

    public class WarrantyCheckResultDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? SaleDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public bool IsUnderWarranty { get; set; }
        public int RemainingDays { get; set; }
        public string? Notes { get; set; }
    }
}
