using SmartPharmacySystem.Application.DTOs.OnlineOrders;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.IServices;

public interface IOnlineOrderService
{
    // Mobile App
    Task<OnlineOrderDto> PlaceOrderAsync(int customerId, PlaceOnlineOrderDto dto);
    Task<IEnumerable<OnlineOrderDto>> GetCustomerOrdersAsync(int customerId);
    Task<OnlineOrderDto> GetOrderByIdAsync(int orderId, int? customerId = null);

    // ERP Dashboard
    Task<IEnumerable<OnlineOrderDto>> GetAllOrdersAsync(OnlineOrderStatus? status = null);
    Task<OnlineOrderDto> UpdateOrderStatusAsync(int orderId, OnlineOrderStatus newStatus, int handledByUserId);
}
