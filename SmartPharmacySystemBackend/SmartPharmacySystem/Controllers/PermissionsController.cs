using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Permission;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;

namespace SmartPharmacySystem.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _permissionService.GetAllPermissionsGroupedAsync();
        return Ok(ApiResponse<IEnumerable<PermissionGroupDto>>.Succeeded(groups, "Permissions retrieved successfully"));
    }
}
