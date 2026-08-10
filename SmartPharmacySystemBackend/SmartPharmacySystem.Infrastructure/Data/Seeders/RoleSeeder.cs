using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Data.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin", NameAr = "مدير النظام", Description = "صلاحيات كاملة على النظام", Color = "#ef4444", IsSystemRole = true, IsActive = true },
                new Role { Name = "Pharmacist", NameAr = "صيدلاني", Description = "إدارة المخزون والمبيعات", Color = "#3b82f6", IsSystemRole = true, IsActive = true },
                new Role { Name = "Cashier", NameAr = "كاشير", Description = "نقطة البيع فقط", Color = "#10b981", IsSystemRole = true, IsActive = true },
                new Role { Name = "Accountant", NameAr = "محاسب", Description = "إدارة الحسابات والخزينة", Color = "#f59e0b", IsSystemRole = true, IsActive = true },
                new Role { Name = "HR", NameAr = "موارد بشرية", Description = "إدارة شؤون الموظفين", Color = "#8b5cf6", IsSystemRole = true, IsActive = true }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // إعطاء جميع الصلاحيات لمدير النظام
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            var allPermissionIds = await context.Permissions.Select(p => p.Id).ToListAsync();
            var existingAdminPermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var newPermissionIds = allPermissionIds.Except(existingAdminPermissions).ToList();

            if (newPermissionIds.Any())
            {
                var rolePermissions = newPermissionIds.Select(pid => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow
                });

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
