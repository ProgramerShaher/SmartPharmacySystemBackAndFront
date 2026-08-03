using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.StockTransfers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _transferService;
    public StockTransfersController(IStockTransferService transferService) => _transferService = transferService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _transferService.GetByIdAsync(id);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم جلب التحويل بنجاح"));
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _transferService.GetByCodeAsync(code);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم جلب التحويل بنجاح"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? sourceWarehouseId, [FromQuery] int? destinationWarehouseId, [FromQuery] TransferStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] TransferType? transferType)
    {
        var result = await _transferService.GetAllAsync(sourceWarehouseId, destinationWarehouseId, status, from, to, transferType);
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب التحويلات"));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(TransferStatus status)
    {
        var result = await _transferService.GetByStatusAsync(status);
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب التحويلات حسب الحالة"));
    }

    [HttpGet("pending-approval")]
    public async Task<IActionResult> GetPendingApproval()
    {
        var result = await _transferService.GetPendingForApprovalAsync();
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب تحويلات معلقة"));
    }

    [HttpGet("pending-dispatch")]
    public async Task<IActionResult> GetPendingDispatch()
    {
        var result = await _transferService.GetPendingForDispatchAsync();
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب تحويلات للشحن"));
    }

    [HttpGet("pending-receipt/{warehouseId}")]
    public async Task<IActionResult> GetPendingReceipt(int warehouseId)
    {
        var result = await _transferService.GetPendingForReceiptAsync(warehouseId);
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب تحويلات للاستلام"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStockTransferDto dto, [FromQuery] int requestedByUserId)
    {
        var result = await _transferService.CreateAsync(dto, requestedByUserId);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم إنشاء التحويل بنجاح"));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] int approvedByUserId)
    {
        var result = await _transferService.ApproveAsync(id, approvedByUserId);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم اعتماد التحويل"));
    }

    [HttpPut("{id}/dispatch")]
    public async Task<IActionResult> Dispatch(int id, [FromQuery] int dispatchedByUserId)
    {
        var result = await _transferService.DispatchAsync(id, dispatchedByUserId);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم شحن التحويل"));
    }

    [HttpPut("{id}/receive")]
    public async Task<IActionResult> Receive(int id, ReceiveStockTransferDto dto, [FromQuery] int receivedByUserId)
    {
        var result = await _transferService.ReceiveAsync(id, dto, receivedByUserId);
        return Ok(ApiResponse<StockTransferDto>.Succeeded(result, "تم استلام التحويل"));
    }

    [HttpGet("by-branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId, [FromQuery] TransferType? transferType)
    {
        var result = await _transferService.GetByBranchAsync(branchId, transferType);
        return Ok(ApiResponse<IEnumerable<StockTransferDto>>.Succeeded(result, "تم جلب تحويلات الفرع"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _transferService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف التحويل بنجاح", "تم الحذف"));
    }
}
