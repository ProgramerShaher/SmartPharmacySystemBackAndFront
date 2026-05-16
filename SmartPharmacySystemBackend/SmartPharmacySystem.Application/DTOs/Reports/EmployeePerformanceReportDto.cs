namespace SmartPharmacySystem.Application.DTOs.Reports;

public class EmployeePerformanceReportQueryDto
{
    public int? EmployeeId { get; set; }
    public int? RoleId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? OperationType { get; set; }
}

public class EmployeePerformanceReportDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? EmployeeId { get; set; }
    public int? RoleId { get; set; }
    public string OperationType { get; set; } = "All";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalSales { get; set; }
    public int SalesInvoiceCount { get; set; }
    public decimal TotalReturns { get; set; }
    public int SalesReturnCount { get; set; }
    public decimal NetSales => TotalSales - TotalReturns;
    public int ItemsSold { get; set; }
    public int ItemsReturned { get; set; }
    public List<EmployeePerformanceSummaryDto> Employees { get; set; } = new();
    public List<EmployeeSalesInvoiceDto> SaleInvoices { get; set; } = new();
    public List<EmployeeSalesReturnDto> SalesReturns { get; set; } = new();
}

public class EmployeePerformanceSummaryDto
{
    public int? EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int SalesInvoiceCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal TotalReturns { get; set; }
    public int SalesReturnCount { get; set; }
    public int ItemsReturned { get; set; }
    public decimal NetSales => TotalSales - TotalReturns;
}

public class EmployeeSalesInvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemsCount { get; set; }
    public List<EmployeeOperationItemDto> Items { get; set; } = new();
}

public class EmployeeSalesReturnDto
{
    public int ReturnId { get; set; }
    public int SaleInvoiceId { get; set; }
    public string SaleInvoiceNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Reason { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemsCount { get; set; }
    public List<EmployeeOperationItemDto> Items { get; set; } = new();
}

public class EmployeeOperationItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}
