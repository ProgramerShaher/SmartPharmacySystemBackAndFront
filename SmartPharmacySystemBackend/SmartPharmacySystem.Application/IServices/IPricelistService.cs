using SmartPharmacySystem.Application.DTOs.Pricelists;

namespace SmartPharmacySystem.Application.IServices;

public interface IPricelistService
{
    Task<IEnumerable<PricelistDto>> GetAllAsync(bool? isActive = null);
    Task<IEnumerable<PricelistSelectDto>> GetSelectListAsync();
    Task<PricelistDto?> GetByIdAsync(int id);
    Task<PricelistDto> CreateAsync(CreatePricelistDto dto, int userId);
    Task<PricelistDto> UpdateAsync(int id, UpdatePricelistDto dto, int userId);
    Task DeleteAsync(int id);

    /// <summary>
    /// Get the effective sale price for a medicine given a pricelist.
    /// Returns null if no special price applies.
    /// </summary>
    Task<decimal?> GetEffectivePriceAsync(int pricelistId, int medicineId, decimal originalPrice);

    /// <summary>
    /// Get the discount percentage for a medicine given a pricelist.
    /// Returns the item-specific discount if available, else the global discount.
    /// </summary>
    Task<decimal> GetDiscountPercentageAsync(int pricelistId, int medicineId);
}
