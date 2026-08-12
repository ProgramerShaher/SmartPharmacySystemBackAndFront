using System;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.DTOs.StockCounts;

public class StockCountScheduleDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public StockCountFrequency Frequency { get; set; }
    public string FrequencyLabel { get; set; } = string.Empty;
    public DateTime NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockCountScheduleDto
{
    public int WarehouseId { get; set; }
    public StockCountFrequency Frequency { get; set; }
    public DateTime NextRunDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockCountScheduleDto
{
    public StockCountFrequency Frequency { get; set; }
    public DateTime NextRunDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
