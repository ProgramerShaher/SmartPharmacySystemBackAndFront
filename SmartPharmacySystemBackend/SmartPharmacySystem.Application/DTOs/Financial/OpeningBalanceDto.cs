namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// DTO لإضافة الرصيد الافتتاحي
/// Opening balance DTO
/// </summary>
public class OpeningBalanceDto
{
    /// <summary>
    /// معرف الحساب
    /// Account ID
    /// </summary>
    public int AccountId { get; set; } = 1;

    /// <summary>
    /// المبلغ الافتتاحي
    /// Opening amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// الوصف
    /// Description
    /// </summary>
    public string Description { get; set; } = "الرصيد الافتتاحي";

    /// <summary>
    /// تاريخ الرصيد الافتتاحي
    /// Opening balance date
    /// </summary>
    public DateTime? TransactionDate { get; set; }
}
