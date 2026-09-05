using System;

namespace SmartPharmacySystem.Application.DTOs.Shifts;

public class ShiftDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int BranchId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsCashTransferredToMainSafe { get; set; }
    public DateTime? CashTransferredAt { get; set; }
    public decimal? TransferredCashAmount { get; set; }
    public string? TransferReferenceNumber { get; set; }
}
