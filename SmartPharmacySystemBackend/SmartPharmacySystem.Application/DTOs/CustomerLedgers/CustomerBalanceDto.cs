namespace SmartPharmacySystem.Application.DTOs.CustomerLedgers;

/// <summary>
/// كائن نقل البيانات لعرض رصيد العميل الموحد.
/// </summary>
public class CustomerBalanceDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalPayments { get; set; }
    public DateTime LastTransactionDate { get; set; }
}
