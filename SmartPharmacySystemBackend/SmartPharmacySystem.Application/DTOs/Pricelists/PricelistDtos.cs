namespace SmartPharmacySystem.Application.DTOs.Pricelists;

/// <summary>
/// DTO for reading a Pricelist.
/// </summary>
public class PricelistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal GlobalDiscountPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PricelistItemDto> Items { get; set; } = new();
    public int CustomersCount { get; set; }
}

/// <summary>
/// DTO for a single price override item within a Pricelist.
/// </summary>
public class PricelistItemDto
{
    public int Id { get; set; }
    public int PricelistId { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public decimal? FixedPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

/// <summary>
/// DTO for creating a new Pricelist.
/// </summary>
public class CreatePricelistDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal GlobalDiscountPercentage { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public List<CreatePricelistItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for a single item when creating/updating a Pricelist.
/// </summary>
public class CreatePricelistItemDto
{
    public int MedicineId { get; set; }
    public decimal? FixedPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

/// <summary>
/// DTO for updating an existing Pricelist.
/// </summary>
public class UpdatePricelistDto : CreatePricelistDto
{
    public int Id { get; set; }
}

/// <summary>
/// Lightweight DTO for dropdown lists.
/// </summary>
public class PricelistSelectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal GlobalDiscountPercentage { get; set; }
}
