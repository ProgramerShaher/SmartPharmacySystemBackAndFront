using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// تنفيذ خدمة إدارة الشيكات
/// </summary>
public class ChequeService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<ChequeService> logger) : IChequeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<ChequeService> _logger = logger;

    public async Task<ChequeDto> GetByIdAsync(int id)
    {
        var cheque = await _unitOfWork.Cheques.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الشيك برقم {id} غير موجود");
        return _mapper.Map<ChequeDto>(cheque);
    }

    public async Task<PagedResponse<ChequeDto>> GetPagedAsync(int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status, string? type)
    {
        Enum.TryParse<ChequeStatus>(status, out var chequeStatus);
        Enum.TryParse<ChequeType>(type, out var chequeType);

        var (items, total) = await _unitOfWork.Cheques.GetPagedAsync(
            null, 
            status != null ? chequeStatus : null, 
            type != null ? chequeType : null, 
            startDate, 
            endDate, 
            page, 
            pageSize);

        return new PagedResponse<ChequeDto>(_mapper.Map<IEnumerable<ChequeDto>>(items), total, page, pageSize);
    }

    public async Task<ChequeDto> CreateAsync(ChequeDto dto, int userId)
    {
        _logger.LogInformation("تسجيل شيك جديد: {Number} بمبلغ {Amount}", dto.ChequeNumber, dto.Amount);

        var cheque = _mapper.Map<Cheque>(dto);
        cheque.CreatedAt = DateTime.UtcNow;
        cheque.CreatedBy = userId;
        cheque.Status = ChequeStatus.Pending;

        await _unitOfWork.Cheques.AddAsync(cheque);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ChequeDto>(cheque);
    }

    public async Task UpdateStatusAsync(int id, string status, int userId, string? notes = null)
    {
        var cheque = await _unitOfWork.Cheques.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الشيك برقم {id} غير موجود");

        _logger.LogInformation("تحديث حالة الشيك {Number} إلى {Status}", cheque.ChequeNumber, status);

        if (!Enum.TryParse<ChequeStatus>(status, out var newStatus))
            throw new ArgumentException("حالة الشيك غير صالحة");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // منطق خاص عند التحصيل (Collected)
            if (newStatus == ChequeStatus.Collected && cheque.Status != ChequeStatus.Collected)
            {
                // هنا يمكننا إضافة منطق إنشاء قيد محاسبي تلقائي
                _logger.LogInformation("تم تحصيل الشيك {Number}، جاري تحديث الأرصدة", cheque.ChequeNumber);
            }

            await _unitOfWork.Cheques.UpdateStatusAsync(id, newStatus);
            
            if (!string.IsNullOrEmpty(notes))
            {
                cheque.Notes = notes;
                await _unitOfWork.Cheques.UpdateAsync(cheque);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "خطأ أثناء تحديث حالة الشيك {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ChequeDto>> GetDueChequesAsync(int daysAhead)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysAhead);
        var (items, _) = await _unitOfWork.Cheques.GetPagedAsync(null, ChequeStatus.Pending, null, null, targetDate, 1, 1000);
        
        return _mapper.Map<IEnumerable<ChequeDto>>(items);
    }

    public async Task CancelAsync(int id, int userId, string reason)
    {
        var cheque = await _unitOfWork.Cheques.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الشيك برقم {id} غير موجود");

        if (cheque.Status == ChequeStatus.Collected)
            throw new InvalidOperationException("لا يمكن إلغاء شيك تم تحصيله بالفعل");

        await _unitOfWork.Cheques.UpdateStatusAsync(id, ChequeStatus.Cancelled);
        cheque.Notes = $"إلغاء: {reason}";
        await _unitOfWork.Cheques.UpdateAsync(cheque);
        await _unitOfWork.SaveChangesAsync();
    }
}
