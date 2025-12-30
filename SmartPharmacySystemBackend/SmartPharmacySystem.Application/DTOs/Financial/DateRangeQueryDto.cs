namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// DTO لاستعلام نطاق التواريخ
/// Date range query DTO
/// </summary>
public class DateRangeQueryDto
{
    /// <summary>
    /// تاريخ البداية
    /// Start date
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// تاريخ النهاية
    /// End date
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// معرف الحساب (اختياري)
    /// Account ID (optional)
    /// </summary>
    public int? AccountId { get; set; }
}
