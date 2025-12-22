using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the internal pharmacy account balance.
/// </summary>
public class PharmacyAccount
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Current available balance in the pharmacy.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Last time the balance was updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
