using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.DTOs.OnlineOrders;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services;

public class OnlineOrderService : IOnlineOrderService
{
    private readonly IApplicationDbContext _context;

    public OnlineOrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    // ===================== Mobile App Methods =====================

    public async Task<OnlineOrderDto> PlaceOrderAsync(int customerId, PlaceOnlineOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new ArgumentException("يجب إضافة منتج واحد على الأقل");

        // Build order items and calculate total
        var orderItems = new List<OnlineOrderItem>();
        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == item.MedicineId && !m.IsDeleted);
            if (medicine == null)
                throw new KeyNotFoundException($"الدواء بالمعرف {item.MedicineId} غير موجود");

            if (item.Quantity <= 0)
                throw new ArgumentException($"الكمية يجب أن تكون أكبر من صفر للدواء: {medicine.Name}");

            var price = medicine.DefaultSalePrice;
            orderItems.Add(new OnlineOrderItem
            {
                MedicineId = item.MedicineId,
                Quantity = item.Quantity,
                UnitPrice = price
            });
            total += price * item.Quantity;
        }

        // Generate unique order number
        var orderNumber = $"MOB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new OnlineOrder
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            CustomerId = customerId,
            DeliveryAddress = dto.DeliveryAddress,
            CustomerNotes = dto.CustomerNotes,
            PaymentMethod = dto.PaymentMethod,
            TotalAmount = total,
            Status = OnlineOrderStatus.Pending,
            OrderItems = orderItems
        };

        _context.OnlineOrders.Add(order);
        await _context.SaveChangesAsync();

        return await MapToDto(order.Id);
    }

    public async Task<IEnumerable<OnlineOrderDto>> GetCustomerOrdersAsync(int customerId)
    {
        var orders = await _context.OnlineOrders
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .Include(o => o.OrderItems).ThenInclude(i => i.Medicine)
            .Include(o => o.Customer)
            .ToListAsync();

        return orders.Select(MapOrderToDto);
    }

    public async Task<OnlineOrderDto> GetOrderByIdAsync(int orderId, int? customerId = null)
    {
        var query = _context.OnlineOrders
            .Include(o => o.OrderItems).ThenInclude(i => i.Medicine)
            .Include(o => o.Customer)
            .Where(o => o.Id == orderId && !o.IsDeleted);

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        var order = await query.FirstOrDefaultAsync();
        if (order == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        return MapOrderToDto(order);
    }

    // ===================== ERP Methods =====================

    public async Task<IEnumerable<OnlineOrderDto>> GetAllOrdersAsync(OnlineOrderStatus? status = null)
    {
        var query = _context.OnlineOrders
            .Include(o => o.OrderItems).ThenInclude(i => i.Medicine)
            .Include(o => o.Customer)
            .Where(o => !o.IsDeleted);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return orders.Select(MapOrderToDto);
    }

    public async Task<OnlineOrderDto> UpdateOrderStatusAsync(int orderId, OnlineOrderStatus newStatus, int handledByUserId)
    {
        var order = await _context.OnlineOrders
            .Include(o => o.OrderItems).ThenInclude(i => i.Medicine)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

        if (order == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        order.Status = newStatus;
        order.HandledBy = handledByUserId;
        order.HandledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapOrderToDto(order);
    }

    // ===================== Private Helpers =====================

    private async Task<OnlineOrderDto> MapToDto(int orderId)
    {
        var order = await _context.OnlineOrders
            .Include(o => o.OrderItems).ThenInclude(i => i.Medicine)
            .Include(o => o.Customer)
            .FirstAsync(o => o.Id == orderId);
        return MapOrderToDto(order);
    }

    private static OnlineOrderDto MapOrderToDto(OnlineOrder order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        PaymentMethod = order.PaymentMethod.ToString(),
        DeliveryAddress = order.DeliveryAddress,
        CustomerNotes = order.CustomerNotes,
        Status = GetStatusArabic(order.Status),
        StatusCode = (int)order.Status,
        CustomerId = order.CustomerId,
        CustomerName = order.Customer?.Name ?? "",
        CustomerPhone = order.Customer?.PhoneNumber,
        HandledAt = order.HandledAt,
        OrderItems = order.OrderItems.Select(i => new OnlineOrderItemDto
        {
            Id = i.Id,
            MedicineId = i.MedicineId,
            MedicineName = i.Medicine?.Name ?? "",
            MedicineImage = i.Medicine?.ImageUrl,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.Quantity * i.UnitPrice
        }).ToList()
    };

    private static string GetStatusArabic(OnlineOrderStatus status) => status switch
    {
        OnlineOrderStatus.Pending => "قيد الانتظار",
        OnlineOrderStatus.Preparing => "جاري التحضير",
        OnlineOrderStatus.OutForDelivery => "في الطريق إليك",
        OnlineOrderStatus.Delivered => "تم التسليم",
        OnlineOrderStatus.Cancelled => "ملغي",
        OnlineOrderStatus.Rejected => "مرفوض",
        _ => "غير معروف"
    };
}
