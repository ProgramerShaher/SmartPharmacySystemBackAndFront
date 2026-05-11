using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.OnlineOrders;

// DTO returned to the mobile app / ERP
public class OnlineOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int CustomerId { get; set; }
    public DateTime? HandledAt { get; set; }
    public List<OnlineOrderItemDto> OrderItems { get; set; } = new();
}

public class OnlineOrderItemDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? MedicineImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

// Sent from mobile app to place an order
public class PlaceOnlineOrderDto
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderItemRequestDto
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
}

// Sent from ERP to update order status
public class UpdateOnlineOrderStatusDto
{
    public OnlineOrderStatus Status { get; set; }
}
