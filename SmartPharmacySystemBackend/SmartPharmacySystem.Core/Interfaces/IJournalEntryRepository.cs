using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع القيود المحاسبية والسندات
/// </summary>
public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(int id);
    Task<JournalEntry?> GetByVoucherNumberAsync(string voucherNumber);
    Task AddAsync(JournalEntry entity);
    Task UpdateAsync(JournalEntry entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    
    Task<(IEnumerable<JournalEntry> Items, int TotalCount)> GetPagedAsync(
        string? search, 
        DateTime? fromDate, 
        DateTime? toDate, 
        VoucherType? type,
        int page, 
        int pageSize);

    /// <summary>
    /// ترحيل القيد للأستاذ العام
    /// </summary>
    Task PostEntryAsync(int entryId);

    /// <summary>
    /// جلب حركات حساب معين خلال فترة زمنية (للأستاذ العام)
    /// </summary>
    Task<IEnumerable<JournalEntryLine>> GetLinesByAccountIdAsync(int accountId, DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// جلب جميع حركات النظام حتى تاريخ معين (لميزان المراجعة)
    /// </summary>
    Task<IEnumerable<JournalEntryLine>> GetAllLinesAsync(DateTime? upToDate);
}
