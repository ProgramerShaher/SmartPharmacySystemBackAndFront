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

        // إعطاء صلاحيات محددة للصيدلاني
        var pharmacistRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Pharmacist");
        if (pharmacistRole != null)
        {
            var pharmacistPermissionCodes = new List<string>
            {
                "sales.invoices.view",
                "sales.invoices.create",
                "sales.invoices.edit",
                "sales.invoices.add_discount",
                "sales.returns.view",
                "sales.returns.create",
                "sales.shifts.manage",
                "sales.daily_closing.manage",
                "inventory.medicines.view",
                "inventory.movements.view",
                "partners.customers.view",
                "partners.customers.manage",
                "finance.treasury.view",
                "finance.receipts.create",
                "finance.expenses.create",
                "dashboard.master"
            };

            var pharmacistPermissionIds = await context.Permissions
                .Where(p => pharmacistPermissionCodes.Contains(p.Code))
                .Select(p => p.Id)
                .ToListAsync();

            var existingPharmacistPermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == pharmacistRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var newPharmacistPermissionIds = pharmacistPermissionIds.Except(existingPharmacistPermissions).ToList();

            if (newPharmacistPermissionIds.Any())
            {
                var rolePermissions = newPharmacistPermissionIds.Select(pid => new RolePermission
                {
                    RoleId = pharmacistRole.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow
                });

                await context.RolePermissions.AddRangeAsync(rolePermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
