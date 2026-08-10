using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع سجل العمليات
/// Audit log repository interface
/// </summary>
public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync(
        int? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50);

    Task<int> CountAsync(
        int? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<AuditLog?> GetByIdAsync(int id);
    Task AddAsync(AuditLog log);
}
