using SmartPharmacySystem.Application.DTOs.Pricelists;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using AutoMapper;

namespace SmartPharmacySystem.Application.Services;

public class PricelistService : IPricelistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PricelistService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PricelistDto>> GetAllAsync(bool? isActive = null)
    {
        var pricelists = await _unitOfWork.Pricelists.GetAllAsync(isActive);
        var result = new List<PricelistDto>();

        foreach (var p in pricelists)
        {
            var dto = MapToDto(p);
            dto.CustomersCount = await _unitOfWork.Pricelists.GetCustomersCountAsync(p.Id);
            result.Add(dto);
        }

        return result;
    }

    public async Task<IEnumerable<PricelistSelectDto>> GetSelectListAsync()
    {
        var pricelists = await _unitOfWork.Pricelists.GetAllAsync(true);
        return pricelists.Select(p => new PricelistSelectDto
        {
            Id = p.Id,
            Name = p.Name,
            GlobalDiscountPercentage = p.GlobalDiscountPercentage
        });
    }

    public async Task<PricelistDto?> GetByIdAsync(int id)
    {
        var pricelist = await _unitOfWork.Pricelists.GetByIdWithItemsAsync(id);
        if (pricelist == null) return null;

        var dto = MapToDto(pricelist);
        dto.CustomersCount = await _unitOfWork.Pricelists.GetCustomersCountAsync(id);
        return dto;
    }

    public async Task<PricelistDto> CreateAsync(CreatePricelistDto dto, int userId)
    {
        var pricelist = new Pricelist
        {
            Name = dto.Name,
            Description = dto.Description,
            GlobalDiscountPercentage = dto.GlobalDiscountPercentage,
            IsActive = dto.IsActive,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            pricelist.Items.Add(new PricelistItem
            {
                MedicineId = itemDto.MedicineId,
                FixedPrice = itemDto.FixedPrice,
                DiscountPercentage = itemDto.DiscountPercentage,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Pricelists.AddAsync(pricelist);
        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(pricelist.Id))!;
    }

    public async Task<PricelistDto> UpdateAsync(int id, UpdatePricelistDto dto, int userId)
    {
        var pricelist = await _unitOfWork.Pricelists.GetByIdWithItemsAsync(id)
            ?? throw new KeyNotFoundException($"قائمة الأسعار رقم {id} غير موجودة");

        pricelist.Name = dto.Name;
        pricelist.Description = dto.Description;
        pricelist.GlobalDiscountPercentage = dto.GlobalDiscountPercentage;
        pricelist.IsActive = dto.IsActive;
        pricelist.UpdatedBy = userId;
        pricelist.UpdatedAt = DateTime.UtcNow;

        var existingItemsDict = pricelist.Items.ToDictionary(i => i.MedicineId);
        var newMedicineIds = dto.Items.Select(i => i.MedicineId).ToHashSet();

        // Remove items that are no longer in the dto
        var itemsToRemove = pricelist.Items.Where(i => !newMedicineIds.Contains(i.MedicineId)).ToList();
        foreach (var item in itemsToRemove)
        {
            pricelist.Items.Remove(item);
        }

        // Add or Update items
        foreach (var itemDto in dto.Items)
        {
            if (existingItemsDict.TryGetValue(itemDto.MedicineId, out var existingItem))
            {
                // Update existing item
                existingItem.FixedPrice = itemDto.FixedPrice;
                existingItem.DiscountPercentage = itemDto.DiscountPercentage;
            }
            else
            {
                // Add new item
                pricelist.Items.Add(new PricelistItem
                {
                    PricelistId = id,
                    MedicineId = itemDto.MedicineId,
                    FixedPrice = itemDto.FixedPrice,
                    DiscountPercentage = itemDto.DiscountPercentage,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
                
                // Add to dictionary to prevent duplicates if dto contains the same medicine twice
                existingItemsDict[itemDto.MedicineId] = null!; 
            }
        }

        await _unitOfWork.Pricelists.UpdateAsync(pricelist);
        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Pricelists.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal?> GetEffectivePriceAsync(int pricelistId, int medicineId, decimal originalPrice)
    {
        var pricelist = await _unitOfWork.Pricelists.GetByIdWithItemsAsync(pricelistId);
        if (pricelist == null) return null;

        // Check for specific medicine override
        var item = pricelist.Items.FirstOrDefault(i => i.MedicineId == medicineId);

        if (item?.FixedPrice.HasValue == true)
            return item.FixedPrice.Value; // Fixed price override

        var discountPct = item?.DiscountPercentage ?? pricelist.GlobalDiscountPercentage;
        if (discountPct > 0)
            return Math.Round(originalPrice * (1 - discountPct / 100), 2);

        return null; // No change
    }

    public async Task<decimal> GetDiscountPercentageAsync(int pricelistId, int medicineId)
    {
        var pricelist = await _unitOfWork.Pricelists.GetByIdWithItemsAsync(pricelistId);
        if (pricelist == null) return 0;

        var item = pricelist.Items.FirstOrDefault(i => i.MedicineId == medicineId);
        return item?.DiscountPercentage ?? pricelist.GlobalDiscountPercentage;
    }

    private static PricelistDto MapToDto(Pricelist p) => new PricelistDto
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        GlobalDiscountPercentage = p.GlobalDiscountPercentage,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        Items = p.Items.Select(i => new PricelistItemDto
        {
            Id = i.Id,
            PricelistId = i.PricelistId,
            MedicineId = i.MedicineId,
            MedicineName = i.Medicine?.Name ?? "",
            FixedPrice = i.FixedPrice,
            DiscountPercentage = i.DiscountPercentage
        }).ToList()
    };
}
