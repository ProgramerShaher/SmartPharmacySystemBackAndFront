using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.AuditLog;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;

namespace SmartPharmacySystem.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] string? action, [FromQuery] string? entityType)
    {
        var logs = await _auditLogService.GetAllAsync(userId, action, entityType);
        return Ok(ApiResponse<IEnumerable<AuditLogDto>>.Succeeded(logs, "Audit logs retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _auditLogService.GetByIdAsync(id);
        if (log == null) return NotFound(ApiResponse<object>.Failed("سجل العملية غير موجود"));

        return Ok(ApiResponse<AuditLogDto>.Succeeded(log, "Audit log retrieved successfully"));
    }
}
