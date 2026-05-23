using AutoMapper;
using SmartPharmacySystem.Application.DTOs.InterBranchSettlements;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class InterBranchSettlementService : IInterBranchSettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InterBranchSettlementService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InterBranchSettlementDto> GetByIdAsync(int id)
    {
        var settlement = await _unitOfWork.InterBranchSettlements.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("التسوية غير موجودة");
        return _mapper.Map<InterBranchSettlementDto>(settlement);
    }

    public async Task<IEnumerable<InterBranchSettlementDto>> GetAllAsync(int? fromBranchId = null, int? toBranchId = null, SettlementStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var settlements = await _unitOfWork.InterBranchSettlements.GetAllAsync(fromBranchId, toBranchId, status, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<InterBranchSettlementDto>>(settlements);
    }

    public async Task<IEnumerable<InterBranchSettlementDto>> GetPendingAsync()
    {
        var settlements = await _unitOfWork.InterBranchSettlements.GetPendingAsync();
        return _mapper.Map<IEnumerable<InterBranchSettlementDto>>(settlements);
    }

    public async Task<InterBranchSettlementDto> CreateAsync(CreateInterBranchSettlementDto dto)
    {
        var settlement = _mapper.Map<InterBranchSettlement>(dto);
        settlement.Status = SettlementStatus.Pending;
        await _unitOfWork.InterBranchSettlements.AddAsync(settlement);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<InterBranchSettlementDto>(settlement);
    }

    public async Task<InterBranchSettlementDto> ApproveAsync(int id, int approvedByUserId)
    {
        var settlement = await _unitOfWork.InterBranchSettlements.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("التسوية غير موجودة");

        if (settlement.Status != SettlementStatus.Pending)
            throw new InvalidOperationException("يمكن فقط تسوية الطلبات المعلقة");

        settlement.Status = SettlementStatus.Settled;
        settlement.SettledByUserId = approvedByUserId;
        settlement.SettlementDate = DateTime.UtcNow;

        await _unitOfWork.InterBranchSettlements.UpdateAsync(settlement);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<InterBranchSettlementDto>(settlement);
    }

    public async Task UpdateAsync(InterBranchSettlementDto dto)
    {
        var settlement = await _unitOfWork.InterBranchSettlements.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("التسوية غير موجودة");

        _mapper.Map(dto, settlement);
        await _unitOfWork.InterBranchSettlements.UpdateAsync(settlement);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var settlement = await _unitOfWork.InterBranchSettlements.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("التسوية غير موجودة");
        await _unitOfWork.InterBranchSettlements.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalPendingAmountAsync(int branchId)
    {
        return await _unitOfWork.InterBranchSettlements.GetTotalPendingAmountAsync(branchId);
    }
}
