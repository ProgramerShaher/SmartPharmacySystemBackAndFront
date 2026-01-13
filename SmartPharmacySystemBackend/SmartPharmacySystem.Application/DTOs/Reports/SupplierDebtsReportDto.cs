namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// تقرير ديون الموردين
/// Supplier Debts Report DTO
/// </summary>
public class SupplierDebtsReportDto
{
    /// <summary>إجمالي ما عليك للموردين (ديونك)</summary>
    public decimal TotalPayable { get; set; }

    /// <summary>إجمالي ما لك على الموردين (رصيد زائد)</summary>
    public decimal TotalReceivable { get; set; }

    /// <summary>الصافي (موجب = عليك، سالب = لك)</summary>
    public decimal NetBalance => TotalPayable - TotalReceivable;

    /// <summary>عدد الموردين المستحق لهم</summary>
    public int CreditorCount { get; set; }

    /// <summary>عدد الموردين لديهم رصيد زائد</summary>
    public int DebtorCount { get; set; }

    /// <summary>تفاصيل ديون كل مورد</summary>
    public List<SupplierDebtDto> Suppliers { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>تفاصيل دين المورد</summary>
public class SupplierDebtDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }

    /// <summary>الرصيد الحالي (موجب = عليك له، سالب = لك عليه)</summary>
    public decimal Balance { get; set; }

    /// <summary>نوع الرصيد</summary>
    public string BalanceType => Balance > 0 ? "مستحق له" : Balance < 0 ? "رصيد زائد" : "صفر";

    /// <summary>لون الحالة</summary>
    public string StatusColor => Balance > 0 ? "danger" : Balance < 0 ? "success" : "info";

    /// <summary>إجمالي المشتريات</summary>
    public decimal TotalPurchases { get; set; }

    /// <summary>إجمالي المدفوعات</summary>
    public decimal TotalPayments { get; set; }

    /// <summary>عدد الفواتير المستحقة</summary>
    public int OutstandingInvoices { get; set; }

    /// <summary>تاريخ آخر فاتورة شراء</summary>
    public DateTime? LastPurchaseDate { get; set; }

    /// <summary>تاريخ آخر دفعة</summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>عدد أيام التأخير</summary>
    public int DaysOverdue { get; set; }
}
