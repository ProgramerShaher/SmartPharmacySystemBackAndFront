using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class MedicineWarehouseConfigRepository : IMedicineWarehouseConfigRepository
{
    private readonly ApplicationDbContext _context;

    public MedicineWarehouseConfigRepository(ApplicationDbContext context) => _context = context;

    public async Task<MedicineWarehouseConfig?> GetByIdAsync(int warehouseId, int medicineId)
    {
        return await _context.MedicineWarehouseConfigs
            .AsNoTracking()
            .Include(c => c.Warehouse)
            .Include(c => c.Medicine)
            .FirstOrDefaultAsync(c => c.WarehouseId == warehouseId && c.MedicineId == medicineId);
    }

    public async Task<IEnumerable<MedicineWarehouseConfig>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.MedicineWarehouseConfigs
            .AsNoTracking()
            .Include(c => c.Medicine)
            .Where(c => c.WarehouseId == warehouseId)
            .OrderBy(c => c.Medicine.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicineWarehouseConfig>> GetByMedicineIdAsync(int medicineId)
    {
        return await _context.MedicineWarehouseConfigs
            .AsNoTracking()
            .Include(c => c.Warehouse).ThenInclude(w => w.Branch)
            .Where(c => c.MedicineId == medicineId)
            .ToListAsync();
    }

    public async Task<MedicineWarehouseConfig?> GetByWarehouseMedicineAsync(int warehouseId, int medicineId)
    {
        return await _context.MedicineWarehouseConfigs
            .AsNoTracking()
            .Include(c => c.Warehouse)
            .Include(c => c.Medicine)
            .FirstOrDefaultAsync(c => c.WarehouseId == warehouseId && c.MedicineId == medicineId);
    }

    public async Task<MedicineWarehouseConfig> AddAsync(MedicineWarehouseConfig config)
    {
        await _context.MedicineWarehouseConfigs.AddAsync(config);
        return config;
    }

    public async Task UpdateAsync(MedicineWarehouseConfig config)
    {
        _context.MedicineWarehouseConfigs.Update(config);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int warehouseId, int medicineId)
    {
        var config = await _context.MedicineWarehouseConfigs
            .FirstOrDefaultAsync(c => c.WarehouseId == warehouseId && c.MedicineId == medicineId);
        if (config != null)
        {
            _context.MedicineWarehouseConfigs.Remove(config);
        }
    }

    public async Task<bool> ExistsAsync(int warehouseId, int medicineId)
    {
        return await _context.MedicineWarehouseConfigs
            .AsNoTracking()
            .AnyAsync(c => c.WarehouseId == warehouseId && c.MedicineId == medicineId);
    }
}
