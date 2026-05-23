using AutoMapper;
using SmartPharmacySystem.Application.DTOs.InventoryStocks;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class InventoryStockService : IInventoryStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryStockService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InventoryStockDto> GetByIdAsync(int id)
    {
        var stock = await _unitOfWork.InventoryStocks.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المخزون غير موجود");
        return _mapper.Map<InventoryStockDto>(stock);
    }

    public async Task<IEnumerable<InventoryStockDto>> GetByWarehouseIdAsync(int warehouseId)
    {
        var stocks = await _unitOfWork.InventoryStocks.GetByWarehouseIdAsync(warehouseId);
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }

    public async Task<IEnumerable<InventoryStockDto>> GetByMedicineIdAsync(int medicineId)
    {
        var stocks = await _unitOfWork.InventoryStocks.GetByMedicineIdAsync(medicineId);
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }

    public async Task<IEnumerable<InventoryStockDto>> GetExpiringSoonAsync(int daysThreshold)
    {
        var stocks = await _unitOfWork.InventoryStocks.GetExpiringSoonAsync(daysThreshold);
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }

    public async Task<IEnumerable<InventoryStockDto>> GetExpiredAsync()
    {
        var stocks = await _unitOfWork.InventoryStocks.GetExpiredAsync();
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }

    public async Task<IEnumerable<InventoryStockDto>> GetBelowReorderLevelAsync()
    {
        var stocks = await _unitOfWork.InventoryStocks.GetBelowReorderLevelAsync();
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }

    public async Task<int> GetTotalQuantityAsync(int warehouseId, int medicineId)
    {
        return await _unitOfWork.InventoryStocks.GetTotalQuantityAsync(warehouseId, medicineId);
    }

    public async Task<InventoryStockDto> CreateAsync(CreateInventoryStockDto dto)
    {
        var stock = _mapper.Map<InventoryStock>(dto);
        await _unitOfWork.InventoryStocks.AddAsync(stock);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<InventoryStockDto>(stock);
    }

    public async Task UpdateAsync(UpdateInventoryStockDto dto)
    {
        var stock = await _unitOfWork.InventoryStocks.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("المخزون غير موجود");

        _mapper.Map(dto, stock);
        await _unitOfWork.InventoryStocks.UpdateAsync(stock);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var stock = await _unitOfWork.InventoryStocks.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المخزون غير موجود");
        await _unitOfWork.InventoryStocks.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.InventoryStocks.ExistsAsync(id);
    }

    public async Task<IEnumerable<InventoryStockDto>> SearchAsync(string? search = null, int? warehouseId = null, int? medicineId = null)
    {
        var stocks = await _unitOfWork.InventoryStocks.SearchAsync(search, warehouseId, medicineId);
        return _mapper.Map<IEnumerable<InventoryStockDto>>(stocks);
    }
}
