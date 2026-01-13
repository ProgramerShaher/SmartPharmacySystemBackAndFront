namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير ديون العملاء
/// Customer Debts Report DTO
/// </summary>
public class CustomerDebtsReportDto
{
    /// <summary>إجمالي ديون العملاء (ما لك عليهم)</summary>
    public decimal TotalReceivable { get; set; }

    /// <summary>إجمالي رصيد لصالح العملاء (ما لهم عليك)</summary>
    public decimal TotalPayable { get; set; }

    /// <summary>الصافي</summary>
    public decimal NetBalance => TotalReceivable - TotalPayable;

    /// <summary>عدد العملاء المدينين</summary>
    public int DebtorCount { get; set; }

    /// <summary>عدد العملاء الدائنين</summary>
    public int CreditorCount { get; set; }

    /// <summary>تفاصيل ديون كل عميل</summary>
    public List<CustomerDebtDto> Customers { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>تفاصيل دين العميل</summary>
public class CustomerDebtDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    /// <summary>الرصيد الحالي (موجب = مدين لك، سالب = دائن)</summary>
    public decimal Balance { get; set; }

    /// <summary>نوع الرصيد</summary>
    public string BalanceType => Balance > 0 ? "مدين" : Balance < 0 ? "دائن" : "صفر";

    /// <summary>لون الحالة</summary>
    public string StatusColor => Balance > 0 ? "danger" : Balance < 0 ? "success" : "info";

    /// <summary>إجمالي المشتريات</summary>
    public decimal TotalPurchases { get; set; }

    /// <summary>إجمالي المدفوعات</summary>
    public decimal TotalPayments { get; set; }

    /// <summary>عدد الفواتير المستحقة</summary>
    public int OutstandingInvoices { get; set; }

    /// <summary>تاريخ آخر فاتورة</summary>
    public DateTime? LastInvoiceDate { get; set; }

    /// <summary>تاريخ آخر دفعة</summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>عدد أيام التأخير</summary>
    public int DaysOverdue { get; set; }
}
