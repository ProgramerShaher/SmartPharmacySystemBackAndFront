using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[Authorize]
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

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var balance = await _financialService.GetBalanceAsync();
        return Ok(ApiResponse<PharmacyAccountDto>.Succeeded(balance, "تم جلب الرصيد بنجاح"));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] FinancialTransactionQueryDto query)
    {
        var transactions = await _financialService.GetTransactionsAsync(query);
        return Ok(ApiResponse<PagedResponse<FinancialTransactionDto>>.Succeeded(transactions, "تم جلب الحركات المالية بنجاح"));
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetFinancialInvoices([FromQuery] FinancialInvoiceQueryDto query)
    {
        var invoices = await _financialService.GetFinancialInvoicesAsync(query);
        return Ok(ApiResponse<PagedResponse<FinancialInvoiceDto>>.Succeeded(invoices, "تم جلب فواتير النظام المالي بنجاح"));
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetReport([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var report = await _financialService.GetFinancialReportAsync(start, end);
        return Ok(ApiResponse<FinancialReportDto>.Succeeded(report, "تم جلب التقرير المالي بنجاح"));
    }

    [HttpPost("manual-transaction")]
    public async Task<IActionResult> RecordManualTransaction([FromBody] CreateManualTransactionRequest request)
    {
        var transaction = await _financialService.ProcessTransactionAsync(
            request.Amount,
            request.Type,
            request.Description
        );
        return Ok(ApiResponse<FinancialTransactionDto>.Succeeded(transaction, "تم تسجيل الحركة المالية يدوياً بنجاح"));
    }
}

public class CreateManualTransactionRequest
{
    public decimal Amount { get; set; }
    public FinancialTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}
