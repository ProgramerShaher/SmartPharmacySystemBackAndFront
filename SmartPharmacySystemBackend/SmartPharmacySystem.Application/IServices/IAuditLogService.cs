using SmartPharmacySystem.Application.DTOs.AuditLog;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync(int? userId = null, string? action = null, string? entityType = null);
    Task<AuditLogDto?> GetByIdAsync(int id);
    Task LogAsync(int? userId, string? userName, string action, string? entityType, int? entityId, string? oldValues, string? newValues, string? ipAddress, string? notes = null);
}
