using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// مستودع الوصول إلى بيانات ملفات الأنشطة التجارية
/// </summary>
public class BusinessProfileRepository : IBusinessProfileRepository
{
    private readonly ApplicationDbContext _context;

    public BusinessProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessProfile?> GetActiveProfileAsync()
    {
        // البحث عن الملف النشط المعتمد أولاً
        var profile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(p => p.IsActive && !p.IsDeleted);

        // إذا لم يكن هناك ملف محدد كنشط، نأخذ أول ملف مسجل
        if (profile == null)
        {
            profile = await _context.BusinessProfiles
                .FirstOrDefaultAsync(p => !p.IsDeleted);
        }

        return profile;
    }

    public async Task<BusinessProfile?> GetByTypeAsync(BusinessType businessType)
    {
        return await _context.BusinessProfiles
            .FirstOrDefaultAsync(p => p.BusinessType == businessType && !p.IsDeleted);
    }

    public async Task<IEnumerable<BusinessProfile>> GetAllProfilesAsync()
    {
        return await _context.BusinessProfiles
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task AddAsync(BusinessProfile profile)
    {
        await _context.BusinessProfiles.AddAsync(profile);
    }

    public async Task UpdateAsync(BusinessProfile profile)
    {
        _context.Entry(profile).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}
