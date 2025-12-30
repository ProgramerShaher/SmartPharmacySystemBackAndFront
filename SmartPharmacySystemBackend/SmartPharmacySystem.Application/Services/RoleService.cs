using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Role;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// خدمة الأدوار
/// Role service implementation
/// </summary>
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        _logger.LogInformation("Getting all roles");

        var roles = await _unitOfWork.Roles.GetAllAsync();
        var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);

        // Add user count for each role
        foreach (var roleDto in roleDtos)
        {
            var role = roles.FirstOrDefault(r => r.Id == roleDto.Id);
            roleDto.UserCount = role?.Users?.Count ?? 0;
        }

        return roleDtos;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        _logger.LogInformation("Getting role by ID: {RoleId}", id);

        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            return null;
        }

        var roleDto = _mapper.Map<RoleDto>(role);
        roleDto.UserCount = role.Users?.Count ?? 0;

        return roleDto;
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
        _logger.LogInformation("Getting role by name: {RoleName}", name);

        var role = await _unitOfWork.Roles.GetByNameAsync(name);
        if (role == null)
        {
            return null;
        }

        var roleDto = _mapper.Map<RoleDto>(role);
        roleDto.UserCount = role.Users?.Count ?? 0;

        return roleDto;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        _logger.LogInformation("Creating new role: {RoleName}", dto.Name);

        // Check if role already exists
        var existingRole = await _unitOfWork.Roles.GetByNameAsync(dto.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"الدور '{dto.Name}' موجود بالفعل");
        }

        var role = _mapper.Map<Core.Entities.Role>(dto);
        role.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role created successfully: {RoleId}", role.Id);

        var roleDto = _mapper.Map<RoleDto>(role);
        roleDto.UserCount = 0;

        return roleDto;
    }

    public async Task UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        _logger.LogInformation("Updating role: {RoleId}", id);

        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"الدور برقم {id} غير موجود");
        }

        // Check if new name conflicts with another role
        if (role.Name != dto.Name)
        {
            var existingRole = await _unitOfWork.Roles.GetByNameAsync(dto.Name);
            if (existingRole != null && existingRole.Id != id)
            {
                throw new InvalidOperationException($"الدور '{dto.Name}' موجود بالفعل");
            }
        }

        _mapper.Map(dto, role);
        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role updated successfully: {RoleId}", id);
    }

    public async Task DeleteRoleAsync(int id)
    {
        _logger.LogInformation("Deleting role: {RoleId}", id);

        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"الدور برقم {id} غير موجود");
        }

        // Check if role has users
        if (role.Users != null && role.Users.Any())
        {
            throw new InvalidOperationException($"لا يمكن حذف الدور لأنه مرتبط بـ {role.Users.Count} مستخدم");
        }

        await _unitOfWork.Roles.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role deleted successfully: {RoleId}", id);
    }
}
