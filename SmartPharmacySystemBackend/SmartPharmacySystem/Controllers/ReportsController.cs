using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Reports;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// مركز التقارير الإمبراطوري
/// Imperial Reporting Center
/// Performance Protocol: Response time < 100ms
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    // ===================== كشف الحساب الموحد - Unified Statement =====================

    /// <summary>
    /// كشف الحساب الموحد للعملاء والموردين
    /// Unified Account Statement for Customers and Suppliers
    /// GET /api/Reports/statement/Customer/1?fromDate=2024-01-01&toDate=2024-12-31
    /// </summary>
    /// <param name="entityType">نوع الكيان: Customer أو Supplier</param>
    /// <param name="entityId">معرف الكيان</param>
    /// <param name="fromDate">من تاريخ (اختياري)</param>
    /// <param name="toDate">إلى تاريخ (اختياري)</param>
    /// <returns>كشف الحساب مع الرصيد التراكمي</returns>
    [HttpGet("statement/{entityType}/{entityId}")]
    public async Task<IActionResult> GetUnifiedStatement(
        string entityType,
        int entityId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _reportService.GetUnifiedStatementAsync(entityType, entityId, fromDate, toDate);
            return Ok(ApiResponse<UnifiedStatementDto>.Succeeded(result, "تم جلب كشف الحساب بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found: {EntityType} {EntityId}", entityType, entityId);
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid entity type: {EntityType}", entityType);
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
    }

    /// <summary>
    /// كشف الحساب مع التصفح
    /// Paginated Account Statement
    /// </summary>
    [HttpGet("statement/{entityType}/{entityId}/paged")]
    public async Task<IActionResult> GetUnifiedStatementPaged(
        string entityType,
        int entityId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _reportService.GetUnifiedStatementPagedAsync(
                entityType, entityId, fromDate, toDate, page, pageSize);
            return Ok(ApiResponse<PagedResponse<StatementLineDto>>.Succeeded(result, "تم جلب كشف الحساب بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
    }

    // ===================== تقرير صافي الأرباح - Net Profit Report =====================

    /// <summary>
    /// تقرير صافي الأرباح الدوري
    /// Periodic Net Profit Report
    /// معادلة: صافي الربح = (إجمالي المبيعات - المرتجعات - الخصومات) - تكلفة البضاعة المباعة - المصروفات
    /// </summary>
    /// <param name="fromDate">من تاريخ (مطلوب)</param>
    /// <param name="toDate">إلى تاريخ (مطلوب)</param>
    /// <param name="includeExpenseDetails">تضمين تفاصيل المصروفات حسب الفئة</param>
    [HttpGet("net-profit")]
    public async Task<IActionResult> GetNetProfitReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] bool includeExpenseDetails = true)
    {
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<object>.Failed("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
        }

        var result = await _reportService.GetNetProfitReportAsync(fromDate, toDate, includeExpenseDetails);
        return Ok(ApiResponse<NetProfitReportDto>.Succeeded(result, "تم جلب تقرير صافي الأرباح بنجاح"));
    }

    // ===================== تقييم المخزون - Inventory Valuation =====================

    /// <summary>
    /// تقرير تقييم المخزون الذري
    /// Atomic Inventory Valuation Report
    /// يربط كل حبة دواء بـ BatchId و ExpiryDate و PurchasePrice
    /// </summary>
    [HttpGet("inventory-valuation")]
    public async Task<IActionResult> GetInventoryValuation([FromQuery] InventoryValuationQueryDto query)
    {
        var result = await _reportService.GetInventoryValuationAsync(query);
        return Ok(ApiResponse<InventoryValuationDto>.Succeeded(result, "تم جلب تقرير تقييم المخزون بنجاح"));
    }

    /// <summary>
    /// تقييم المخزون مع التصفح
    /// Paginated Inventory Valuation
    /// </summary>
    [HttpGet("inventory-valuation/paged")]
    public async Task<IActionResult> GetInventoryValuationPaged([FromQuery] InventoryValuationQueryDto query)
    {
        var result = await _reportService.GetInventoryValuationPagedAsync(query);
        return Ok(ApiResponse<PagedResponse<BatchValuationDto>>.Succeeded(result, "تم جلب تقييم المخزون بنجاح"));
    }

    // ===================== ملخص التقارير - Reports Summary =====================

    /// <summary>
    /// ملخص سريع للتقارير (للوحة التحكم)
    /// Quick Reports Summary (for Dashboard)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetReportsSummary()
    {
        var result = await _reportService.GetReportsSummaryAsync();
        return Ok(ApiResponse<ReportsSummaryDto>.Succeeded(result, "تم جلب ملخص التقارير بنجاح"));
    }

    // ===================== تقرير المبيعات اليومية - Daily Sales Report =====================

    /// <summary>
    /// تقرير المبيعات اليومية
    /// Daily Sales Report with hourly breakdown
    /// GET /api/Reports/daily-sales?date=2024-01-15
    /// </summary>
    /// <param name="date">تاريخ التقرير (الافتراضي: اليوم)</param>
    [HttpGet("daily-sales")]
    public async Task<IActionResult> GetDailySalesReport([FromQuery] DateTime? date = null)
    {
        var reportDate = date ?? DateTime.Today;
        var result = await _reportService.GetDailySalesReportAsync(reportDate);
        return Ok(ApiResponse<DailySalesReportDto>.Succeeded(result, "تم جلب تقرير المبيعات اليومية بنجاح"));
    }

    // ===================== تقرير الأدوية الأكثر مبيعاً - Best Selling Report =====================

    /// <summary>
    /// تقرير الأدوية الأكثر مبيعاً
    /// Best Selling Medicines Report
    /// GET /api/Reports/best-selling?fromDate=2024-01-01&toDate=2024-12-31&top=10
    /// </summary>
    /// <param name="fromDate">من تاريخ</param>
    /// <param name="toDate">إلى تاريخ</param>
    /// <param name="top">عدد الأدوية (الافتراضي: 10)</param>
    [HttpGet("best-selling")]
    public async Task<IActionResult> GetBestSellingMedicines(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int top = 10)
    {
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<object>.Failed("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
        }

        if (top <= 0 || top > 100)
        {
            return BadRequest(ApiResponse<object>.Failed("عدد الأدوية يجب أن يكون بين 1 و 100"));
        }

        var result = await _reportService.GetBestSellingMedicinesAsync(fromDate, toDate, top);
        return Ok(ApiResponse<BestSellingMedicinesReportDto>.Succeeded(result, "تم جلب تقرير الأدوية الأكثر مبيعاً بنجاح"));
    }

    // ===================== تقرير ديون العملاء - Customer Debts Report =====================

    /// <summary>
    /// تقرير ديون العملاء
    /// Customer Debts Report - shows receivables and payables
    /// GET /api/Reports/customer-debts
    /// </summary>
    [HttpGet("customer-debts")]
    public async Task<IActionResult> GetCustomerDebtsReport()
    {
        var result = await _reportService.GetCustomerDebtsReportAsync();
        return Ok(ApiResponse<CustomerDebtsReportDto>.Succeeded(result, "تم جلب تقرير ديون العملاء بنجاح"));
    }

    [HttpGet("employee-performance")]
    public async Task<IActionResult> GetEmployeePerformanceReport([FromQuery] EmployeePerformanceReportQueryDto query)
    {
        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value.Date > query.ToDate.Value.Date)
        {
            return BadRequest(ApiResponse<object>.Failed("تاريخ البداية يجب أن يكون قبل تاريخ النهاية"));
        }

        try
        {
            var result = await _reportService.GetEmployeePerformanceReportAsync(query);
            return Ok(ApiResponse<EmployeePerformanceReportDto>.Succeeded(result, "تم جلب تقرير أداء الموظفين بنجاح"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
    }

    // ===================== تقرير ديون الموردين - Supplier Debts Report =====================

    /// <summary>
    /// تقرير ديون الموردين
    /// Supplier Debts Report - shows payables and receivables
    /// GET /api/Reports/supplier-debts
    /// </summary>
    [HttpGet("supplier-debts")]
    public async Task<IActionResult> GetSupplierDebtsReport()
    {
        var result = await _reportService.GetSupplierDebtsReportAsync();
        return Ok(ApiResponse<SupplierDebtsReportDto>.Succeeded(result, "تم جلب تقرير ديون الموردين بنجاح"));
    }

    // ===================== تصدير التقارير - Export Reports =====================

    /// <summary>
    /// تصدير كشف الحساب بصيغة CSV
    /// Export Account Statement as CSV
    /// </summary>
    [HttpGet("statement/{entityType}/{entityId}/export")]
    public async Task<IActionResult> ExportStatement(
        string entityType,
        int entityId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string format = "csv")
    {
        try
        {
            var statement = await _reportService.GetUnifiedStatementAsync(entityType, entityId, fromDate, toDate);

            if (format.ToLower() == "csv")
            {
                var csv = GenerateStatementCsv(statement);
                var fileName = $"statement_{entityType}_{entityId}_{DateTime.Now:yyyyMMdd}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }

            // For PDF, return JSON and let frontend handle PDF generation
            return Ok(ApiResponse<UnifiedStatementDto>.Succeeded(statement, "Export data ready"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
    }

    private string GenerateStatementCsv(UnifiedStatementDto statement)
    {
        var sb = new System.Text.StringBuilder();

        // Header
        sb.AppendLine($"كشف حساب {statement.EntityType}");
        sb.AppendLine($"الاسم: {statement.EntityName}");
        sb.AppendLine($"الفترة: {statement.FromDate?.ToString("yyyy-MM-dd") ?? "البداية"} - {statement.ToDate?.ToString("yyyy-MM-dd") ?? "الآن"}");
        sb.AppendLine($"الرصيد الافتتاحي: {statement.OpeningBalance:N2}");
        sb.AppendLine($"الرصيد الحالي: {statement.CurrentBalance:N2}");
        sb.AppendLine();

        // Column headers
        sb.AppendLine("التاريخ,نوع الحركة,رقم المرجع,البيان,مدين,دائن,الرصيد");

        // Data rows
        foreach (var line in statement.Lines)
        {
            sb.AppendLine($"{line.TransactionDate:yyyy-MM-dd},{line.ReferenceType},{line.ReferenceNumber},\"{line.Description}\",{line.Debit:N2},{line.Credit:N2},{line.RunningBalance:N2}");
        }

        // Footer
        sb.AppendLine();
        sb.AppendLine($"إجمالي المدين: {statement.TotalDebit:N2}");
        sb.AppendLine($"إجمالي الدائن: {statement.TotalCredit:N2}");

        return sb.ToString();
    }
}
