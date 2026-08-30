using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Permission;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PermissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PermissionGroupDto>> GetAllPermissionsGroupedAsync()
    {
        _logger.LogInformation("Getting all permissions grouped by module");

        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<PermissionDto>>(permissions);

        var grouped = dtos.GroupBy(p => new { p.Module, p.ModuleAr })
                          .Select(g => new PermissionGroupDto
                          {
                              Module = g.Key.Module,
                              ModuleAr = g.Key.ModuleAr,
                              Permissions = g.ToList()
                          })
                          .ToList();

        return grouped;
    }

    public async Task<List<string>> GetUserEffectivePermissionsAsync(int userId)
    {
        _logger.LogInformation("Calculating effective permissions for user {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return new List<string>();
        }

        var effectivePermissions = new HashSet<string>();

        // 1. Get Role Permissions
        if (user.RoleId > 0)
        {
            var rolePermissionIds = await _unitOfWork.Roles.GetPermissionIdsAsync(user.RoleId);
            var rolePermissions = await _unitOfWork.Permissions.FindAsync(p => rolePermissionIds.Contains(p.Id));
            foreach (var perm in rolePermissions)
            {
                effectivePermissions.Add(perm.Code);
            }
        }

        // 2. Get User Permission Overrides
        var overrides = await _unitOfWork.UserPermissionOverrides.GetActiveByUserIdAsync(userId);
        
        // Apply Denies first
        var deniedIds = overrides.Where(o => o.GrantType == GrantType.Deny).Select(o => o.PermissionId).ToList();
        if (deniedIds.Any())
        {
            var deniedPermissions = await _unitOfWork.Permissions.FindAsync(p => deniedIds.Contains(p.Id));
            foreach (var perm in deniedPermissions)
            {
                effectivePermissions.Remove(perm.Code);
            }
        }

        // Apply Grants
        var grantedIds = overrides.Where(o => o.GrantType == GrantType.Grant).Select(o => o.PermissionId).ToList();
        if (grantedIds.Any())
        {
            var grantedPermissions = await _unitOfWork.Permissions.FindAsync(p => grantedIds.Contains(p.Id));
            foreach (var perm in grantedPermissions)
            {
                effectivePermissions.Add(perm.Code);
            }
        }

        return effectivePermissions.ToList();
    }

    public async Task<List<int>> GetUserEffectivePermissionIdsAsync(int userId)
    {
        _logger.LogInformation("Calculating effective permission IDs for user {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return new List<int>();

        var effectivePermissionIds = new HashSet<int>();

        if (user.RoleId > 0)
        {
            var rolePermissionIds = await _unitOfWork.Roles.GetPermissionIdsAsync(user.RoleId);
            foreach (var id in rolePermissionIds)
            {
                effectivePermissionIds.Add(id);
            }
        }

        var overrides = await _unitOfWork.UserPermissionOverrides.GetActiveByUserIdAsync(userId);
        
        var deniedIds = overrides.Where(o => o.GrantType == GrantType.Deny).Select(o => o.PermissionId).ToList();
        foreach (var id in deniedIds)
        {
            effectivePermissionIds.Remove(id);
        }

        var grantedIds = overrides.Where(o => o.GrantType == GrantType.Grant).Select(o => o.PermissionId).ToList();
        foreach (var id in grantedIds)
        {
            effectivePermissionIds.Add(id);
        }

        return effectivePermissionIds.ToList();
    }

    public async Task AssignUserPermissionsAsync(int userId, List<int> selectedPermissionIds)
    {
        _logger.LogInformation("Assigning permissions for user {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException($"User {userId} not found");

        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            var rolePermissionIds = new List<int>();
            if (user.RoleId > 0)
            {
                var roleIds = await _unitOfWork.Roles.GetPermissionIdsAsync(user.RoleId);
                rolePermissionIds.AddRange(roleIds);
            }

            var currentOverrides = await _unitOfWork.UserPermissionOverrides.GetActiveByUserIdAsync(userId);
            foreach(var over in currentOverrides)
            {
                await _unitOfWork.UserPermissionOverrides.DeleteAsync(over.Id);
            }

            var basePermissionsSet = new HashSet<int>(rolePermissionIds);
            var selectedPermissionsSet = new HashSet<int>(selectedPermissionIds);

            var permissionsToGrant = selectedPermissionsSet.Except(basePermissionsSet).ToList();
            var permissionsToDeny = basePermissionsSet.Except(selectedPermissionsSet).ToList();

            foreach (var pId in permissionsToGrant)
            {
                await _unitOfWork.UserPermissionOverrides.AddAsync(new SmartPharmacySystem.Core.Entities.UserPermissionOverride
                {
                    UserId = userId,
                    PermissionId = pId,
                    GrantType = GrantType.Grant,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var pId in permissionsToDeny)
            {
                await _unitOfWork.UserPermissionOverrides.AddAsync(new SmartPharmacySystem.Core.Entities.UserPermissionOverride
                {
                    UserId = userId,
                    PermissionId = pId,
                    GrantType = GrantType.Deny,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions for user {UserId}", userId);
            throw;
        }
    }
}
