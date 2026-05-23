using AutoMapper;
using SmartPharmacySystem.Application.DTOs.DailyClosings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class DailyClosingService : IDailyClosingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public DailyClosingService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<DailyClosingDto> GetByIdAsync(int id)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");
        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task<DailyClosingDto?> GetByBranchDateAsync(int branchId, DateTime date)
    {
        var closing = await _unitOfWork.DailyClosings.GetByBranchDateAsync(branchId, date);
        return closing != null ? _mapper.Map<DailyClosingDto>(closing) : null;
    }

    public async Task<IEnumerable<DailyClosingDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetByBranchIdAsync(branchId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<IEnumerable<DailyClosingDto>> GetByDateAsync(DateTime date)
    {
        var closings = await _unitOfWork.DailyClosings.GetByDateAsync(date);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<IEnumerable<DailyClosingDto>> GetPendingApprovalAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetPendingApprovalAsync(dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<DailyClosingDto> CreateAsync(CreateDailyClosingDto dto)
    {
        if (await _unitOfWork.DailyClosings.ExistsAsync(dto.BranchId, dto.ClosingDate))
            throw new InvalidOperationException("تم إنشاء إغلاق يومي لهذا التاريخ بالفعل");

        var closing = _mapper.Map<DailyClosing>(dto);
        closing.Status = ClosingStatus.PendingApproval;
        closing.SubmittedByUserId = _currentUserService.UserId ?? 0;
        await _unitOfWork.DailyClosings.AddAsync(closing);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task UpdateAsync(DailyClosingDto dto)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");

        _mapper.Map(dto, closing);
        await _unitOfWork.DailyClosings.UpdateAsync(closing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DailyClosingDto> ApproveAsync(int id, int approvedByUserId)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");

        if (closing.Status != ClosingStatus.PendingApproval)
            throw new InvalidOperationException("يمكن فقط اعتماد الإغلاقات المعلقة");

        closing.Status = ClosingStatus.Approved;
        closing.ApprovedByUserId = approvedByUserId;
        closing.ApprovedAt = DateTime.UtcNow;

        await _unitOfWork.DailyClosings.UpdateAsync(closing);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task DeleteAsync(int id)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");
        await _unitOfWork.DailyClosings.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<DailyClosingDto>> GetApprovedAsync(DateTime dateFrom, DateTime dateTo, int? branchId = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetApprovedAsync(dateFrom, dateTo, branchId);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<bool> ExistsAsync(int branchId, DateTime date)
    {
        return await _unitOfWork.DailyClosings.ExistsAsync(branchId, date);
    }
}
