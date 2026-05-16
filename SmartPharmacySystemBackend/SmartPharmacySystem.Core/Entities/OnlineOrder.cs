using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents an online order placed by a customer from the mobile application.
/// </summary>
public class OnlineOrder : BaseEntity
{
    
    public string OrderNumber { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;
    
    // Customer information
    public int CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual Customer Customer { get; set; } = null!;

    public string DeliveryAddress { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    
    // Status
    public OnlineOrderStatus Status { get; set; } = OnlineOrderStatus.Pending;

    // Action tracking
    public int? HandledBy { get; set; } // Pharmacist who approved/rejected
    [ForeignKey("HandledBy")]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Handler { get; set; }

    public DateTime? HandledAt { get; set; }

    // Navigation property
    public virtual ICollection<OnlineOrderItem> OrderItems { get; set; } = new List<OnlineOrderItem>();
}
