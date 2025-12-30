using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Role;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Roles management controller
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على جميع الأدوار
    /// Get all roles
    /// </summary>
    /// <access>Admin</access>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<IEnumerable<RoleDto>>.Succeeded(roles, "تم جلب الأدوار بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return StatusCode(500, ApiResponse<IEnumerable<RoleDto>>.Failed("حدث خطأ أثناء جلب الأدوار"));
        }
    }

    /// <summary>
    /// الحصول على دور بالمعرف
    /// Get role by ID
    /// </summary>
    /// <access>Admin</access>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(ApiResponse<RoleDto>.Failed($"الدور برقم {id} غير موجود"));
            }

            return Ok(ApiResponse<RoleDto>.Succeeded(role, "تم جلب الدور بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role {RoleId}", id);
            return StatusCode(500, ApiResponse<RoleDto>.Failed("حدث خطأ أثناء جلب الدور"));
        }
    }

    /// <summary>
    /// إنشاء دور جديد
    /// Create new role
    /// </summary>
    /// <access>Admin</access>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id },
                ApiResponse<RoleDto>.Succeeded(role, "تم إنشاء الدور بنجاح"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role creation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, ApiResponse<RoleDto>.Failed("حدث خطأ أثناء إنشاء الدور"));
        }
    }

    /// <summary>
    /// تحديث دور
    /// Update role
    /// </summary>
    /// <access>Admin</access>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            await _roleService.UpdateRoleAsync(id, dto);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث الدور بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Role not found: {Message}", ex.Message);
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role update failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تحديث الدور"));
        }
    }

    /// <summary>
    /// حذف دور
    /// Delete role
    /// </summary>
    /// <access>Admin</access>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _roleService.DeleteRoleAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "تم حذف الدور بنجاح"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Role not found: {Message}", ex.Message);
            return NotFound(ApiResponse<object>.Failed(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role deletion failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء حذف الدور"));
        }
    }
}
