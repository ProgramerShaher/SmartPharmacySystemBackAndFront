using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;
using System.Security.Claims;

namespace SmartPharmacySystem.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class BarcodeController : ControllerBase
{
    private readonly ISaleInvoiceService _saleService;
    private readonly IPurchaseInvoiceService _purchaseService;
    private readonly ISalesReturnService _returnService;

    public BarcodeController(
        ISaleInvoiceService saleService,
        IPurchaseInvoiceService purchaseService,
        ISalesReturnService returnService)
    {
        _saleService = saleService;
        _purchaseService = purchaseService;
        _returnService = returnService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessBarcode([FromBody] GetProductForTransactionByBarcodeQuery query)
    {
        try
        {
            int userId = GetUserId();
            BarcodeResultDto result;

            switch (query.TransactionType)
            {
                case TransactionType.Sale:
                    result = await _saleService.ProcessBarcodeItemAsync(query.Barcode, userId);
                    break;
                case TransactionType.Purchase:
                    result = await _purchaseService.ProcessBarcodeItemAsync(query.Barcode, userId);
                    break;
                case TransactionType.Return:
                    result = await _returnService.ProcessBarcodeItemAsync(query.Barcode, userId);
                    break;
                default:
                    return BadRequest(ApiResponse<object>.Failed("نوع العملية غير مدعوم"));
            }

            return Ok(ApiResponse<BarcodeResultDto>.Succeeded(result, "تم جلب بيانات الصنف بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failed($"خطأ في النظام: {ex.Message}"));
        }
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
        {
            return parsedId;
        }
        return 1; // Default to Admin/System if claim not found (should not happen with [Authorize])
    }
}
