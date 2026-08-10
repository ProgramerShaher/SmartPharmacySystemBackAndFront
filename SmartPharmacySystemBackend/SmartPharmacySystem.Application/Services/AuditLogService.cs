using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.AuditLog;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AuditLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAllAsync(int? userId = null, string? action = null, string? entityType = null)
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync();

        if (userId.HasValue)
            logs = logs.Where(l => l.UserId == userId.Value);

        if (!string.IsNullOrEmpty(action))
            logs = logs.Where(l => l.Action.Contains(action, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(entityType))
            logs = logs.Where(l => l.EntityType == entityType);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs.OrderByDescending(l => l.CreatedAt));
    }

    public async Task<AuditLogDto?> GetByIdAsync(int id)
    {
        var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
        if (log == null) return null;

        return _mapper.Map<AuditLogDto>(log);
    }

    public async Task LogAsync(int? userId, string? userName, string action, string? entityType, int? entityId, string? oldValues, string? newValues, string? ipAddress, string? notes = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}
