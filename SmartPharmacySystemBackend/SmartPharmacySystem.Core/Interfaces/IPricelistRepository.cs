using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPricelistRepository
{
    Task<IEnumerable<Pricelist>> GetAllAsync(bool? isActive = null);
    Task<Pricelist?> GetByIdAsync(int id);
    Task<Pricelist?> GetByIdWithItemsAsync(int id);
    Task<Pricelist> AddAsync(Pricelist pricelist);
    Task UpdateAsync(Pricelist pricelist);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetCustomersCountAsync(int pricelistId);
}
