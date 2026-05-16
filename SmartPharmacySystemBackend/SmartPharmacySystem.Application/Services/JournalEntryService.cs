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
/// تنفيذ خدمة القيود المحاسبية
/// </summary>
public class JournalEntryService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<JournalEntryService> logger) : IJournalEntryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<JournalEntryService> _logger = logger;

    public async Task<JournalEntryDto> GetByIdAsync(int id)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"القيد المحاسبي برقام {id} غير موجود");
        return _mapper.Map<JournalEntryDto>(entry);
    }

    public async Task<PagedResponse<JournalEntryDto>> GetPagedAsync(int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status)
    {
        var (items, total) = await _unitOfWork.JournalEntries.GetPagedAsync(
            null, 
            startDate, 
            endDate, 
            null, 
            page, 
            pageSize);
        
        // Manual filter for status if needed, or update repository to support status filter
        var filteredItems = items;
        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Posted") filteredItems = items.Where(e => e.IsPosted);
            else if (status == "Draft") filteredItems = items.Where(e => !e.IsPosted);
        }

        return new PagedResponse<JournalEntryDto>(_mapper.Map<IEnumerable<JournalEntryDto>>(filteredItems), total, page, pageSize);
    }

    public async Task<JournalEntryDto> CreateAsync(JournalEntryDto dto, int? userId)
    {
        _logger.LogInformation("جاري إنشاء قيد محاسبي جديد بواسطة المستخدم {UserId}", userId);

        if (dto.Lines == null || !dto.Lines.Any())
            throw new InvalidOperationException("لا يمكن إنشاء قيد بدون أسطر");

        if (!IsBalanced(dto))
            throw new InvalidOperationException("القيد غير متوازن (المدين لا يساوي الدائن)");

        var entry = _mapper.Map<JournalEntry>(dto);
        entry.CreatedAt = DateTime.UtcNow;
        entry.CreatedBy = userId;
        entry.IsPosted = false; // افتراضياً مسودة

        // حساب إجماليات القيد من الأسطر
        entry.TotalDebit = entry.Lines?.Sum(l => l.Debit) ?? 0m;
        entry.TotalCredit = entry.Lines?.Sum(l => l.Credit) ?? 0m;

        await _unitOfWork.JournalEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<JournalEntryDto>(entry);
    }

    public async Task UpdateAsync(JournalEntryDto dto, int? userId)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"القيد برقم {dto.Id} غير موجود");

        if (entry.IsPosted)
            throw new InvalidOperationException("لا يمكن تعديل قيد تم ترحيله بالفعل");

        if (!IsBalanced(dto))
            throw new InvalidOperationException("القيد غير متوازن");

        _mapper.Map(dto, entry);
        entry.UpdatedAt = DateTime.UtcNow;
        entry.UpdatedBy = userId;

        await _unitOfWork.JournalEntries.UpdateAsync(entry);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ApproveAsync(int id, int? userId)
    {
        _logger.LogInformation("جاري ترحيل القيد {Id} بواسطة {UserId}", id, userId);

        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"القيد برقم {id} غير موجود");

        if (entry.IsPosted)
            throw new InvalidOperationException("القيد تم ترحيله مسبقاً");

        // ترحيل الأرصدة إلى الحسابات (بدون فتح transaction جديدة لأننا داخل transaction خارجية)
        foreach (var line in entry.Lines)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(line.AccountId)
                ?? throw new KeyNotFoundException($"الحساب {line.AccountId} غير موجود");

            account.CurrentBalance += (line.Debit - line.Credit);
            await _unitOfWork.Accounts.UpdateAsync(account);
        }

        // تحديث حالة القيد
        await _unitOfWork.JournalEntries.PostEntryAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }


    public async Task CancelAsync(int id, int? userId, string reason)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"القيد برقم {id} غير موجود");

        if (entry.IsPosted)
            throw new InvalidOperationException("لا يمكن حذف قيد تم ترحيله، يجب عمل قيد عكسي بدلاً من ذلك");

        await _unitOfWork.JournalEntries.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReverseAsync(int id, int? userId, string reason)
    {
        var originalEntry = await _unitOfWork.JournalEntries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"القيد الأصلي برقم {id} غير موجود");

        if (!originalEntry.IsPosted)
            throw new InvalidOperationException("يمكن عكس القيود المرحلة فقط");

        _logger.LogInformation("جاري إنشاء قيد عكسي للقيد {Id}", id);

        var reversedEntry = new JournalEntry
        {
            EntryDate = DateTime.UtcNow,
            Type = originalEntry.Type,
            Description = $"قيد عكسي للقيد رقم {id} - السبب: {reason}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsPosted = false,
            Lines = originalEntry.Lines.Select(l => new JournalEntryLine
            {
                AccountId = l.AccountId,
                Debit = l.Credit, // عكس القيم
                Credit = l.Debit,
                Description = $"عكس: {l.Description}"
            }).ToList()
        };

        await _unitOfWork.JournalEntries.AddAsync(reversedEntry);
        await _unitOfWork.SaveChangesAsync();
    }

    public bool IsBalanced(JournalEntryDto dto)
    {
        if (dto.Lines == null || !dto.Lines.Any()) return false;
        var totalDebit = dto.Lines.Sum(l => l.Debit);
        var totalCredit = dto.Lines.Sum(l => l.Credit);
        return Math.Abs(totalDebit - totalCredit) < 0.001m;
    }
}
