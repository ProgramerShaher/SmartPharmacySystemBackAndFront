using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع الشيكات البنكية
/// </summary>
public interface IChequeRepository
{
    Task<Cheque?> GetByIdAsync(int id);
    Task<Cheque?> GetByNumberAsync(string chequeNumber);
    Task AddAsync(Cheque entity);
    Task UpdateAsync(Cheque entity);
    Task SoftDeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    Task<(IEnumerable<Cheque> Items, int TotalCount)> GetPagedAsync(
        string? search,
        ChequeStatus? status,
        ChequeType? type,
        DateTime? fromDueDate,
        DateTime? toDueDate,
        int page,
        int pageSize);

    /// <summary>
    /// تحديث حالة الشيك (مثلاً من معلق إلى محصل)
    /// </summary>
    Task UpdateStatusAsync(int chequeId, ChequeStatus status);
}
