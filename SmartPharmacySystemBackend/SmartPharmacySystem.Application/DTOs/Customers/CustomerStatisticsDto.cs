namespace SmartPharmacySystem.Application.DTOs.Customers
{
    public class CustomerStatisticsDto
    {
        public decimal TotalDebt { get; set; }
        public int ActiveCustomersCount { get; set; }
        public int InactiveCustomersCount { get; set; }
        public int HighDebtCustomersCount { get; set; } // > 5000
        
        // Debt Distribution
        public int LowDebtCount { get; set; }    // 0 - 1000
        public int MediumDebtCount { get; set; } // 1000 - 5000
        public int HighDebtDistributionCount { get; set; } // > 5000 (Same as HighDebt, but kept for clarity if logic changes)
    }
}
