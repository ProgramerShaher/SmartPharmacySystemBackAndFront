using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class MedicineUnitRepository : IMedicineUnitRepository
{
    private readonly ApplicationDbContext _context;

    public MedicineUnitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MedicineUnit?> GetByIdAsync(int id)
    {
        return await _context.MedicineUnits
            .Where(u => !u.IsDeleted && u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MedicineUnit>> GetByMedicineIdAsync(int medicineId)
    {
        return await _context.MedicineUnits
            .Where(u => !u.IsDeleted && u.MedicineId == medicineId)
            .OrderByDescending(u => u.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicineUnit>> GetAllowedForSaleAsync(int medicineId)
    {
        return await _context.MedicineUnits
            .Where(u => !u.IsDeleted && u.MedicineId == medicineId && u.IsAllowedForSale)
            .OrderByDescending(u => u.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicineUnit>> GetAllowedForPurchaseAsync(int medicineId)
    {
        return await _context.MedicineUnits
            .Where(u => !u.IsDeleted && u.MedicineId == medicineId && u.IsAllowedForPurchase)
            .OrderByDescending(u => u.SortOrder)
            .ToListAsync();
    }

    public async Task AddAsync(MedicineUnit entity)
    {
        await _context.MedicineUnits.AddAsync(entity);
    }

    public Task UpdateAsync(MedicineUnit entity)
    {
        _context.MedicineUnits.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var unit = await GetByIdAsync(id);
        if (unit != null)
        {
            unit.IsDeleted = true;
        }
    }
}
