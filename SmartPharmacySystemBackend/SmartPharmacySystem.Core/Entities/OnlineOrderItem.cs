using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a specific item inside an online order.
/// </summary>
public class OnlineOrderItem : BaseEntity
{
    
    public int OnlineOrderId { get; set; }
    [ForeignKey("OnlineOrderId")]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual OnlineOrder OnlineOrder { get; set; } = null!;
    
    public int MedicineId { get; set; }
    [ForeignKey("MedicineId")]
    public virtual Medicine Medicine { get; set; } = null!;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice => Quantity * UnitPrice;
}
