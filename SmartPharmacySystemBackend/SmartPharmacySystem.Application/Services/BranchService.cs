using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Branches;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BranchDto> GetByIdAsync(int id)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الفرع غير موجود");
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> GetByCodeAsync(string branchCode)
    {
        var branch = await _unitOfWork.Branches.GetByCodeAsync(branchCode)
            ?? throw new KeyNotFoundException("الفرع غير موجود");
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync(string? search = null, bool? isActive = null, BranchType? type = null)
    {
        var branches = await _unitOfWork.Branches.GetAllAsync(search, isActive, type);
        return _mapper.Map<IEnumerable<BranchDto>>(branches);
    }

    public async Task<IEnumerable<BranchDto>> GetActiveBranchesAsync()
    {
        var branches = await _unitOfWork.Branches.GetActiveBranchesAsync();
        return _mapper.Map<IEnumerable<BranchDto>>(branches);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
    {
        if (await _unitOfWork.Branches.CodeExistsAsync(dto.BranchCode))
            throw new InvalidOperationException("كود الفرع موجود بالفعل");

        var branch = _mapper.Map<Branch>(dto);
        branch.IsActive = true;
        await _unitOfWork.Branches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BranchDto>(branch);
    }

    public async Task UpdateAsync(UpdateBranchDto dto)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("الفرع غير موجود");

        if (await _unitOfWork.Branches.CodeExistsAsync(dto.BranchCode, dto.Id))
            throw new InvalidOperationException("كود الفرع موجود بالفعل");

        _mapper.Map(dto, branch);
        await _unitOfWork.Branches.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الفرع غير موجود");
        await _unitOfWork.Branches.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<BranchDto>> GetByTypeAsync(BranchType type)
    {
        var branches = await _unitOfWork.Branches.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<BranchDto>>(branches);
    }

    public async Task<int> GetBranchCountAsync()
    {
        return await _unitOfWork.Branches.GetBranchCountAsync();
    }

    public async Task<bool> CodeExistsAsync(string branchCode, int? excludeId = null)
    {
        return await _unitOfWork.Branches.CodeExistsAsync(branchCode, excludeId);
    }
}
