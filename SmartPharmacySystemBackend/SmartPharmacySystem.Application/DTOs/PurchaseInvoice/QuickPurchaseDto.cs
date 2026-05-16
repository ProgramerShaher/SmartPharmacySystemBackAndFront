using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.PurchaseInvoice
{
    public class QuickPurchaseDto
    {
        [Required]
        public int MedicineId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int BonusQuantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SalePrice { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public string CompanyBatchNumber { get; set; } = string.Empty;

        public string? BatchBarcode { get; set; }

        public string? Notes { get; set; }

        public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

        [Required]
        public int SupplierId { get; set; }
    }
}
