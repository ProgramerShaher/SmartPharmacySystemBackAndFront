using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartPharmacySystem.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"RequirePermission:{permission}";
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role) && (
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "SystemAdmin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "مدير النظام", StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissionsClaim = context.User.FindFirst(c => c.Type == "Permissions");
        if (permissionsClaim == null)
        {
            // --- Fallback للصيدلاني في حال لم يتم تحديث التوكن (JWT Stale Token) ---
            if (!string.IsNullOrEmpty(role) && string.Equals(role, "Pharmacist", StringComparison.OrdinalIgnoreCase))
            {
                var pharmacistDefaultPermissions = new[] { 
                    "sales.shifts.manage", "sales.daily_closing.manage", "sales.invoices.view", 
                    "sales.invoices.create", "sales.invoices.edit", "sales.returns.view", 
                    "sales.returns.create", "inventory.medicines.view" 
                };
                
                if (pharmacistDefaultPermissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }

        var permissions = permissionsClaim.Value.Split(',');
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            // --- Fallback للصيدلاني أيضاً حتى لو كان التوكن يفتقر للصلاحية ---
            if (!string.IsNullOrEmpty(role) && string.Equals(role, "Pharmacist", StringComparison.OrdinalIgnoreCase))
            {
                var pharmacistDefaultPermissions = new[] { 
                    "sales.shifts.manage", "sales.daily_closing.manage", "sales.invoices.view", 
                    "sales.invoices.create", "sales.invoices.edit", "sales.returns.view", 
                    "sales.returns.create", "inventory.medicines.view" 
                };
                
                if (pharmacistDefaultPermissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("RequirePermission", StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            var permission = policyName.Substring("RequirePermission:".Length);
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
