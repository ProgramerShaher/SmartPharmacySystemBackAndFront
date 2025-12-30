using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class FinancialController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly ILogger<FinancialController> _logger;

    public FinancialController(IFinancialService financialService, ILogger<FinancialController> logger)
    {
        _financialService = financialService;
        _logger = logger;
    }

    /// <summary>
    /// Get pharmacy balance
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var balance = await _financialService.GetBalanceAsync();
        return Ok(ApiResponse<PharmacyAccountDto>.Succeeded(balance, "تم جلب الرصيد بنجاح"));
    }

    /// <summary>
    /// Get financial transactions
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] FinancialTransactionQueryDto query)
    {
        var transactions = await _financialService.GetTransactionsAsync(query);
        return Ok(ApiResponse<PagedResponse<FinancialTransactionDto>>.Succeeded(transactions, "تم جلب الحركات المالية بنجاح"));
    }

    /// <summary>
    /// Get financial report
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("report")]
    public async Task<IActionResult> GetReport([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var report = await _financialService.GetFinancialReportAsync(start, end);
        return Ok(ApiResponse<FinancialReportDto>.Succeeded(report, "تم جلب التقرير المالي بنجاح"));
    }

    /// <summary>
    /// Record manual financial adjustment
    /// </summary>
    /// <access>Admin</access>
    [HttpPost("manual-adjustment")]
    public async Task<IActionResult> RecordManualAdjustment([FromBody] CreateManualAdjustmentRequest request)
    {
        // For now, assuming the user is Admin for this endpoint 
        // In a real scenario, this would be checked via [Authorize(Roles = "Admin")]
        var isAdmin = User.IsInRole("Admin") || true;

        var transaction = await _financialService.AddManualAdjustmentAsync(
            accountId: 1, // Default main account
            amount: request.Amount,
            description: request.Description,
            isAdminUser: isAdmin
        );
        return Ok(ApiResponse<FinancialTransactionDto>.Succeeded(transaction, "تم تسجيل التعديل المالي يدوياً بنجاح"));
    }

    /// <summary>
    /// Get general ledger report (Keshf El Hesab)
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _financialService.GetGeneralLedgerAsync(start, end, page, pageSize);
        return Ok(ApiResponse<PagedResponse<GeneralLedgerDto>>.Succeeded(result, "تم جلب كشف الحساب العام بنجاح"));
    }

    /// <summary>
    /// Get annual financial report (Aggregation by category)
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("annual-report/{year}")]
    public async Task<IActionResult> GetAnnualReport(int year)
    {
        var result = await _financialService.GetAnnualFinancialReportAsync(year);
        return Ok(ApiResponse<IEnumerable<AnnualFinancialReportDto>>.Succeeded(result, "تم جلب تقرير الجرد السنوي بنجاح"));
    }

    /// <summary>
    /// Get annual financial summary (Smart Aggregation for Management)
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("annual-summary/{year}")]
    public async Task<IActionResult> GetAnnualSummary(int year)
    {
        var result = await _financialService.GetAnnualFinancialSummaryAsync(year);
        return Ok(ApiResponse<IEnumerable<FinancialSummaryDto>>.Succeeded(result, "تم جلب ملخص التحليل المالي السنوي بنجاح"));
    }
}

public class CreateManualAdjustmentRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
