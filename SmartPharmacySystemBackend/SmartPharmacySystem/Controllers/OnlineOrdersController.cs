using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.OnlineOrders;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;
using System.Security.Claims;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Online Orders Controller
/// - Mobile customers can place and track orders.
/// - ERP pharmacists can view, accept, reject, and update orders.
/// </summary>
[ApiController]
[Route("api/online-orders")]
[Authorize]
public class OnlineOrdersController : ControllerBase
{
    private readonly IOnlineOrderService _orderService;
    private readonly ILogger<OnlineOrdersController> _logger;

    public OnlineOrdersController(IOnlineOrderService orderService, ILogger<OnlineOrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    // ===================== MOBILE APP Endpoints =====================

    /// <summary>
    /// [Mobile] Place a new order. Requires Customer JWT token.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOnlineOrderDto dto)
    {
        try
        {
            var customerId = GetCustomerId();
            var result = await _orderService.PlaceOrderAsync(customerId, dto);
            return Ok(ApiResponse<OnlineOrderDto>.Succeeded(result, "تم إرسال طلبك بنجاح! سيتم مراجعته قريباً."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing online order");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء إرسال الطلب"));
        }
    }

    /// <summary>
    /// [Mobile] Get all orders for the logged-in customer.
    /// </summary>
    [HttpGet("my-orders")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var customerId = GetCustomerId();
            var result = await _orderService.GetCustomerOrdersAsync(customerId);
            return Ok(ApiResponse<IEnumerable<OnlineOrderDto>>.Succeeded(result, "تم جلب طلباتك بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customer orders");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب الطلبات"));
        }
    }

    /// <summary>
    /// [Mobile] Track a specific order by ID.
    /// </summary>
    [HttpGet("my-orders/{orderId:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> TrackOrder(int orderId)
    {
        try
        {
            var customerId = GetCustomerId();
            var result = await _orderService.GetOrderByIdAsync(orderId, customerId);
            return Ok(ApiResponse<OnlineOrderDto>.Succeeded(result, "تم جلب تفاصيل الطلب"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ"));
        }
    }

    // ===================== ERP Endpoints =====================

    /// <summary>
    /// [ERP] Get all online orders with optional status filter.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> GetAllOrders([FromQuery] OnlineOrderStatus? status = null)
    {
        try
        {
            var result = await _orderService.GetAllOrdersAsync(status);
            return Ok(ApiResponse<IEnumerable<OnlineOrderDto>>.Succeeded(result, "تم جلب الطلبات بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all online orders");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب الطلبات"));
        }
    }

    /// <summary>
    /// [ERP] Get a specific order by ID.
    /// </summary>
    [HttpGet("{orderId:int}")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);
            return Ok(ApiResponse<OnlineOrderDto>.Succeeded(result, "تم جلب الطلب"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ"));
        }
    }

    /// <summary>
    /// [ERP] Update order status (Accept / Reject / Preparing / Delivered).
    /// </summary>
    [HttpPatch("{orderId:int}/status")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOnlineOrderStatusDto dto)
    {
        try
        {
            var pharmacistId = GetPharmacistId();
            var result = await _orderService.UpdateOrderStatusAsync(orderId, dto.Status, pharmacistId);
            return Ok(ApiResponse<OnlineOrderDto>.Succeeded(result, "تم تحديث حالة الطلب بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تحديث الحالة"));
        }
    }

    // ===================== Helpers =====================

    private int GetCustomerId()
    {
        var claim = User.FindFirst("CustomerId")?.Value;
        if (claim == null || !int.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم");
        return id;
    }

    private int GetPharmacistId()
    {
        var claim = User.FindFirst("UserId")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !int.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("لم يتم التعرف على الصيدلي");
        return id;
    }
}
