namespace SmartPharmacySystem.Application.DTOs.Customers
{
    public class ReceiptStatisticsDto
    {
        public int TotalReceipts { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TodayAmount { get; set; }
    }
}
