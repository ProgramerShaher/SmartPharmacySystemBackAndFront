using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.CustomerLedgers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerLedgersController : ControllerBase
{
    private readonly ICustomerLedgerService _ledgerService;
    public CustomerLedgersController(ICustomerLedgerService ledgerService) => _ledgerService = ledgerService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ledgerService.GetByIdAsync(id);
        return Ok(ApiResponse<CustomerLedgerDto>.Succeeded(result, "تم جلب القيد بنجاح"));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _ledgerService.GetByCustomerIdAsync(customerId, from, to);
        return Ok(ApiResponse<IEnumerable<CustomerLedgerDto>>.Succeeded(result, "تم جلب قيود العميل"));
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _ledgerService.GetByBranchIdAsync(branchId, from, to);
        return Ok(ApiResponse<IEnumerable<CustomerLedgerDto>>.Succeeded(result, "تم جلب قيود الفرع"));
    }

    [HttpGet("customer/{customerId}/balance")]
    public async Task<IActionResult> GetBalance(int customerId)
    {
        var balance = await _ledgerService.GetCustomerBalanceAsync(customerId);
        return Ok(ApiResponse<decimal>.Succeeded(balance, "تم جلب الرصيد"));
    }

    [HttpGet("customer/{customerId}/balance/{branchId}")]
    public async Task<IActionResult> GetBalanceAtBranch(int customerId, int branchId)
    {
        var balance = await _ledgerService.GetCustomerBalanceAtBranchAsync(customerId, branchId);
        return Ok(ApiResponse<decimal>.Succeeded(balance, "تم جلب الرصيد في الفرع"));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] int? customerId, [FromQuery] int? branchId, [FromQuery] CustomerTransactionType? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _ledgerService.SearchAsync(customerId, branchId, type, from, to);
        return Ok(ApiResponse<IEnumerable<CustomerLedgerDto>>.Succeeded(result, "نتائج البحث"));
    }

    [HttpPost]
    public async Task<IActionResult> AddEntry(CreateCustomerLedgerDto dto)
    {
        var result = await _ledgerService.AddEntryAsync(dto);
        return Ok(ApiResponse<CustomerLedgerDto>.Succeeded(result, "تم إضافة القيد بنجاح"));
    }
}
