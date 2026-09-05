using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the internal pharmacy account balance.
/// </summary>
public class PharmacyAccount : BaseMultiBranchEntity
{

    /// <summary>
    /// اسم الحساب (مثل: الخزينة الرئيسية)
    /// Account name (e.g., Main Cash Register)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current available balance in the pharmacy.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// هل الحساب نشط
    /// Is the account active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// هل هذا حساب درج الكاشير (Cashier Drawer) أم الخزينة الرئيسية (Main Safe)
    /// True = Cashier Drawer, False = Main Safe
    /// </summary>
    public bool IsDrawerAccount { get; set; } = false;

}
