using System.Threading.Tasks;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.DTOs.PurchaseOrders;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة إدارة أوامر الشراء وتحويلها لفواتير مشتريات رسمية
/// </summary>
public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<PurchaseOrderPrintDto?> GetPrintDataAsync(int id);
    Task<PagedResult<PurchaseOrderDto>> GetPagedAsync(PurchaseOrderFilterDto filter);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, int userId);
    Task<PurchaseOrderDto> UpdateAsync(int id, UpdatePurchaseOrderDto dto, int userId);
    Task<bool> DeleteAsync(int id);
    Task<PurchaseOrderDto> ChangeStatusAsync(int id, PurchaseOrderStatus newStatus, int userId);
    Task<PurchaseInvoiceDto> ConvertToPurchaseInvoiceAsync(int id, ConvertPOToInvoiceDto dto, int userId);
}
