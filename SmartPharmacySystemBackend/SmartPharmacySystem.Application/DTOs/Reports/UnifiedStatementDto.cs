namespace SmartPharmacySystem.Application.DTOs.Reports;

/// <summary>
/// كشف الحساب الموحد للعملاء والموردين
/// Unified Account Statement for Customers and Suppliers
/// </summary>
public class UnifiedStatementDto
{
    /// <summary>
    /// معرف الكيان (عميل أو مورد)
    /// Entity ID (Customer or Supplier)
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// نوع الكيان: "Customer" أو "Supplier"
    /// Entity Type: "Customer" or "Supplier"
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// اسم الكيان
    /// Entity Name
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف
    /// Phone Number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// العنوان
    /// Address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// الرصيد الافتتاحي
    /// Opening Balance
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// الرصيد الحالي
    /// Current Balance
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// إجمالي المدين
    /// Total Debit
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// إجمالي الدائن
    /// Total Credit
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// من تاريخ
    /// From Date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// إلى تاريخ
    /// To Date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// تاريخ إنشاء التقرير
    /// Report Generation Date
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// حالة الحساب: مدين أو دائن أو متوازن
    /// Account Status: Debtor, Creditor, or Balanced
    /// </summary>
    public string AccountStatus => CurrentBalance > 0 ? "مدين" : CurrentBalance < 0 ? "دائن" : "متوازن";

    /// <summary>
    /// لون الحالة للعرض
    /// Status Color for Display
    /// </summary>
    public string StatusColor => CurrentBalance > 0 ? "danger" : CurrentBalance < 0 ? "success" : "info";

    /// <summary>
    /// بنود كشف الحساب
    /// Statement Lines
    /// </summary>
    public List<StatementLineDto> Lines { get; set; } = new();
}

/// <summary>
/// بند واحد في كشف الحساب
/// Single Statement Line Entry
/// </summary>
public class StatementLineDto
{
    /// <summary>
    /// تاريخ الحركة
    /// Transaction Date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// نوع المرجع: فاتورة، سند قبض، مرتجع
    /// Reference Type: Invoice, Receipt, Return
    /// </summary>
    public string ReferenceType { get; set; } = string.Empty;

    /// <summary>
    /// رقم المرجع
    /// Reference Number
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف المرجع للربط
    /// Reference ID for Linking
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// وصف الحركة
    /// Transaction Description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// المبلغ المدين (له علينا)
    /// Debit Amount (We owe them / They owe us based on context)
    /// </summary>
    public decimal Debit { get; set; }

    /// <summary>
    /// المبلغ الدائن (لنا عليه)
    /// Credit Amount
    /// </summary>
    public decimal Credit { get; set; }

    /// <summary>
    /// الرصيد التراكمي
    /// Running Balance (Cumulative)
    /// </summary>
    public decimal RunningBalance { get; set; }

    /// <summary>
    /// اسم المستخدم الذي أنشأ الحركة
    /// Created By User Name
    /// </summary>
    public string? CreatedByUserName { get; set; }

    /// <summary>
    /// تاريخ إنشاء الحركة في النظام
    /// Created At Timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// لون الصف للعرض
    /// Row Color for Display
    /// </summary>
    public string RowColor => Debit > 0 ? "danger" : Credit > 0 ? "success" : "info";
}

/// <summary>
/// استعلام كشف الحساب
/// Statement Query Parameters
/// </summary>
public class StatementQueryDto
{
    /// <summary>
    /// نوع الكيان: Customer أو Supplier
    /// Entity Type
    /// </summary>
    public string EntityType { get; set; } = "Customer";

    /// <summary>
    /// معرف الكيان
    /// Entity ID
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// من تاريخ
    /// From Date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// إلى تاريخ
    /// To Date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// رقم الصفحة
    /// Page Number
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// حجم الصفحة
    /// Page Size
    /// </summary>
    public int PageSize { get; set; } = 50;
}
