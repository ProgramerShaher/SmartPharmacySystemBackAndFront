using System;

namespace SmartPharmacySystem.Application.DTOs.Shifts;

public class CloseShiftDto
{
    public decimal ActualClosingCash { get; set; }
    public string? Notes { get; set; }
    public bool TransferToMainSafe { get; set; } = false;
}
