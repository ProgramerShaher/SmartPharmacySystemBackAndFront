using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع عروض الأسعار
/// </summary>
public interface IQuotationRepository
{
    Task<Quotation?> GetByIdAsync(int id);
    Task<Quotation?> GetByIdWithDetailsAsync(int id);
    Task<Quotation?> GetByNumberAsync(string quotationNumber);
    Task<IEnumerable<Quotation>> GetAllAsync(int? branchId = null, int? customerId = null, QuotationStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<(IEnumerable<Quotation> Items, int TotalCount)> GetPagedAsync(string? search, int? branchId, int? customerId, QuotationStatus? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
    Task<string> GetNextQuotationNumberAsync(int branchId);
    Task AddAsync(Quotation quotation);
    Task UpdateAsync(Quotation quotation);
    Task DeleteAsync(int id);
}
