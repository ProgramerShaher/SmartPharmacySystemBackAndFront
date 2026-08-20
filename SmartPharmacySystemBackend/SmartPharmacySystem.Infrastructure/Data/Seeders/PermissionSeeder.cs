using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Data.Seeders;

public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var newPermissions = new List<Permission>
        {
            // ===== 1. المبيعات ونقاط البيع (Sales & POS) =====
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Invoices", ActionAr = "عرض فواتير المبيعات", Code = "sales.invoices.view", Icon = "pi pi-list" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Create Invoice", ActionAr = "إنشاء فاتورة بيع جديدة", Code = "sales.invoices.create", Icon = "pi pi-plus" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Edit Invoice", ActionAr = "تعديل فاتورة بيع", Code = "sales.invoices.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Delete Invoice", ActionAr = "إلغاء فاتورة بيع", Code = "sales.invoices.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Credit Sale", ActionAr = "البيع الآجل (على الحساب)", Code = "sales.invoices.credit_sale", Icon = "pi pi-credit-card" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Exceed Credit Limit", ActionAr = "تجاوز الحد الائتماني للعميل", Code = "sales.invoices.exceed_credit_limit", Icon = "pi pi-exclamation-triangle" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Override Price", ActionAr = "تعديل سعر البيع يدوياً", Code = "sales.invoices.override_price", Icon = "pi pi-tag" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Sell Below Cost", ActionAr = "البيع بأقل من التكلفة", Code = "sales.invoices.sell_below_cost", Icon = "pi pi-arrow-down" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Add Discount", ActionAr = "إضافة خصم للعميل", Code = "sales.invoices.add_discount", Icon = "pi pi-percentage" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Profit", ActionAr = "عرض التكلفة والربح أثناء البيع", Code = "sales.invoices.view_profit", Icon = "pi pi-dollar" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Returns", ActionAr = "عرض مرتجعات المبيعات", Code = "sales.returns.view", Icon = "pi pi-undo" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Create Return", ActionAr = "إنشاء مرتجع مبيعات", Code = "sales.returns.create", Icon = "pi pi-plus" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Delete Return", ActionAr = "إلغاء مرتجع مبيعات", Code = "sales.returns.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Manage Shifts", ActionAr = "فتح وإغلاق الورديات", Code = "sales.shifts.manage", Icon = "pi pi-clock" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Manage Daily Closing", ActionAr = "الإغلاق اليومي للجرد النقدي", Code = "sales.daily_closing.manage", Icon = "pi pi-lock" },
            
            // ===== 2. المشتريات (Purchases) =====
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "View Invoices", ActionAr = "عرض فواتير المشتريات", Code = "purchases.invoices.view", Icon = "pi pi-list" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Create Invoice", ActionAr = "إنشاء فاتورة شراء", Code = "purchases.invoices.create", Icon = "pi pi-plus" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Edit Invoice", ActionAr = "تعديل فاتورة الشراء", Code = "purchases.invoices.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Approve Invoice", ActionAr = "اعتماد فاتورة الشراء", Code = "purchases.invoices.approve", Icon = "pi pi-check-circle" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Update Cost", ActionAr = "تحديث تكلفة الأدوية التلقائي", Code = "purchases.invoices.update_cost", Icon = "pi pi-sync" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "View Returns", ActionAr = "عرض مرتجعات المشتريات", Code = "purchases.returns.view", Icon = "pi pi-undo" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Create Return", ActionAr = "إنشاء إرجاع للمورد", Code = "purchases.returns.create", Icon = "pi pi-plus" },

            // ===== 3. المخزون (Inventory) =====
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Medicines", ActionAr = "عرض الأدوية", Code = "inventory.medicines.view", Icon = "pi pi-list" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Medicines", ActionAr = "إدارة الأدوية (إضافة/تعديل)", Code = "inventory.medicines.manage", Icon = "pi pi-plus" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Configs", ActionAr = "إعدادات حد الطلب للمستودعات", Code = "inventory.configs.manage", Icon = "pi pi-cog" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Batches", ActionAr = "إدارة الدفعات وتواريخ الصلاحية", Code = "inventory.batches.manage", Icon = "pi pi-calendar" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Damaged", ActionAr = "إدارة الأدوية التالفة", Code = "inventory.damaged.manage", Icon = "pi pi-exclamation-triangle" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Print Barcodes", ActionAr = "طباعة ملصقات الباركود", Code = "inventory.barcode.print", Icon = "pi pi-qrcode" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Create Transfer", ActionAr = "إنشاء تحويل بين المخازن", Code = "inventory.transfers.create", Icon = "pi pi-sync" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Approve Transfer", ActionAr = "اعتماد البضاعة المحولة", Code = "inventory.transfers.approve", Icon = "pi pi-check" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Create Stock Count", ActionAr = "إنشاء عملية جرد المخزون", Code = "inventory.stock_counts.create", Icon = "pi pi-plus" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Post Stock Count", ActionAr = "ترحيل الجرد واعتماد الفروقات", Code = "inventory.stock_counts.post", Icon = "pi pi-check-circle" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Warehouses", ActionAr = "عرض المخازن", Code = "inventory.warehouses.view", Icon = "pi pi-building" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Categories", ActionAr = "إدارة التصنيفات", Code = "inventory.categories.manage", Icon = "pi pi-tags" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Movements", ActionAr = "عرض حركة المخزون", Code = "inventory.movements.view", Icon = "pi pi-history" },

            // ===== 4. الموارد البشرية والرواتب (HR & Payroll) =====
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "View Employees", ActionAr = "عرض الموظفين", Code = "hr.employees.view", Icon = "pi pi-users" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Manage Employees", ActionAr = "إدارة بيانات الموظفين", Code = "hr.employees.manage", Icon = "pi pi-user-edit" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Manage Attendance", ActionAr = "إدارة الحضور والانصراف", Code = "hr.attendance.manage", Icon = "pi pi-calendar-times" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Manage Loans", ActionAr = "إدارة السلف للموظفين", Code = "hr.loans.manage", Icon = "pi pi-money-bill" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Generate Salaries", ActionAr = "توليد مسير الرواتب", Code = "hr.salaries.generate", Icon = "pi pi-calculator" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Approve Salaries", ActionAr = "اعتماد وصرف الرواتب", Code = "hr.salaries.approve", Icon = "pi pi-check-circle" },
            new Permission { Module = "HR", ModuleAr = "الموارد البشرية", Action = "Manage Branches", ActionAr = "إدارة الفروع", Code = "branches.manage", Icon = "pi pi-sitemap" },

            // ===== 5. الشركاء (Partners) =====
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "View Customers", ActionAr = "عرض العملاء", Code = "partners.customers.view", Icon = "pi pi-users" },
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "Manage Customers", ActionAr = "إدارة بيانات العملاء", Code = "partners.customers.manage", Icon = "pi pi-user-edit" },
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "View Customer Ledger", ActionAr = "كشف حساب العميل", Code = "partners.customers.ledger", Icon = "pi pi-book" },
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "View Suppliers", ActionAr = "عرض الموردين", Code = "partners.suppliers.view", Icon = "pi pi-truck" },
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "Manage Suppliers", ActionAr = "إدارة بيانات الموردين", Code = "partners.suppliers.manage", Icon = "pi pi-pencil" },
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "View Supplier Ledger", ActionAr = "كشف حساب المورد", Code = "partners.suppliers.ledger", Icon = "pi pi-book" },

            // ===== 6. المالية والخزينة (Finance) =====
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "Create Receipt", ActionAr = "إنشاء سند قبض", Code = "finance.receipts.create", Icon = "pi pi-arrow-down-left" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "Create Payment", ActionAr = "إنشاء سند صرف", Code = "finance.payments.create", Icon = "pi pi-arrow-up-right" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "Create Expense", ActionAr = "تسجيل مصروف نثري", Code = "finance.expenses.create", Icon = "pi pi-plus" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "Manage Cheques", ActionAr = "إدارة الشيكات (استلام، صرف، تحصيل)", Code = "finance.cheques.manage", Icon = "pi pi-credit-card" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "Inter-Branch Settlement", ActionAr = "التسويات المالية بين الفروع", Code = "finance.inter_branch.settle", Icon = "pi pi-sync" },

            // ===== 7. المحاسبة العمومية (Accounting) =====
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "Manage Chart", ActionAr = "تعديل شجرة الحسابات", Code = "accounting.chart.manage", Icon = "pi pi-cog" },
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "Create Journal", ActionAr = "تسجيل قيد يومية", Code = "accounting.journal.create", Icon = "pi pi-plus" },
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "Post Journal", ActionAr = "ترحيل واعتماد قيد اليومية", Code = "accounting.journal.post", Icon = "pi pi-check" },
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "Unpost Journal", ActionAr = "فك ترحيل قيد لتعديله", Code = "accounting.journal.unpost", Icon = "pi pi-unlock" },
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "View Statements", ActionAr = "عرض القوائم المالية والميزانية", Code = "accounting.statements.view", Icon = "pi pi-chart-pie" },

            // ===== 8. التقارير (Reports) =====
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Sales Reports", ActionAr = "عرض تقارير المبيعات", Code = "reports.sales.view", Icon = "pi pi-chart-line" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Purchases Reports", ActionAr = "عرض تقارير المشتريات", Code = "reports.purchases.view", Icon = "pi pi-chart-bar" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Inventory Reports", ActionAr = "عرض تقارير المخزون", Code = "reports.inventory.view", Icon = "pi pi-box" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Financial Reports", ActionAr = "عرض التقارير المالية", Code = "reports.financial.view", Icon = "pi pi-dollar" },

            // ===== 9. النظام والإدارة (Admin & Settings) =====
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "View Users", ActionAr = "عرض قائمة المستخدمين", Code = "admin.users.view", Icon = "pi pi-users" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Manage Users", ActionAr = "إدارة المستخدمين وحسابات الدخول", Code = "admin.users.manage", Icon = "pi pi-user-edit" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "View Roles", ActionAr = "عرض الأدوار والصلاحيات", Code = "access.roles.view", Icon = "pi pi-shield" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Manage Roles", ActionAr = "إنشاء وتعديل الأدوار والصلاحيات", Code = "admin.roles.manage", Icon = "pi pi-shield" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "View Audit Log", ActionAr = "سجل مراقبة النظام (Audit Log)", Code = "admin.audit_log.view", Icon = "pi pi-history" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "View General Settings", ActionAr = "عرض الإعدادات العامة للنظام", Code = "settings.general.view", Icon = "pi pi-cog" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Manage Settings", ActionAr = "إعدادات النظام العامة", Code = "admin.settings.manage", Icon = "pi pi-cog" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Manage Invoice Sequences", ActionAr = "تعديل ترقيم الفواتير", Code = "admin.invoice_sequences.manage", Icon = "pi pi-sort-numeric-up" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Manage Alerts", ActionAr = "إدارة تنبيهات النظام", Code = "admin.alerts.manage", Icon = "pi pi-bell" },
            new Permission { Module = "Admin", ModuleAr = "إدارة النظام", Action = "Online Orders", ActionAr = "إدارة طلبات الأونلاين", Code = "online_orders.manage", Icon = "pi pi-globe" },

            // ===== 10. المالية والخزينة — صلاحيات العرض =====
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "View Treasury Dashboard", ActionAr = "عرض لوحة تحكم الخزينة", Code = "finance.dashboard.view", Icon = "pi pi-chart-pie" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "View Treasury", ActionAr = "عرض الخزينة والأرصدة", Code = "finance.treasury.view", Icon = "pi pi-money-bill" },
            new Permission { Module = "Finance", ModuleAr = "المالية", Action = "View Expenses", ActionAr = "عرض المصروفات النثرية", Code = "finance.expenses.view", Icon = "pi pi-list" },

            // ===== 11. المحاسبة — صلاحيات العرض =====
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "View Chart", ActionAr = "عرض شجرة الحسابات", Code = "accounting.chart.view", Icon = "pi pi-sitemap" },
            new Permission { Module = "Accounting", ModuleAr = "المحاسبة", Action = "View Journal", ActionAr = "عرض قيود اليومية", Code = "accounting.journal.view", Icon = "pi pi-book" },

            // ===== 12. الشركاء والموردون =====
            new Permission { Module = "Partners", ModuleAr = "الشركاء", Action = "View Partners", ActionAr = "عرض قائمة الشركاء", Code = "partners.view", Icon = "pi pi-users" },

            // ===== 13. لوحة التحكم =====
            new Permission { Module = "Dashboard", ModuleAr = "لوحة التحكم", Action = "View Master Dashboard", ActionAr = "عرض لوحة التحكم الشاملة", Code = "dashboard.master", Icon = "pi pi-chart-line" }
        };

        var existingPermissions = await context.Permissions.ToListAsync();
        var permissionsToAdd = new List<Permission>();
        var permissionsToRemove = new List<Permission>();

        // Find permissions to remove
        foreach (var existing in existingPermissions)
        {
            if (!newPermissions.Any(np => np.Code == existing.Code))
            {
                permissionsToRemove.Add(existing);
            }
        }

        // Find permissions to add or update
        foreach (var np in newPermissions)
        {
            var existing = existingPermissions.FirstOrDefault(ep => ep.Code == np.Code);
            if (existing == null)
            {
                permissionsToAdd.Add(np);
            }
            else
            {
                existing.Module = np.Module;
                existing.ModuleAr = np.ModuleAr;
                existing.Action = np.Action;
                existing.ActionAr = np.ActionAr;
                existing.Icon = np.Icon;
                context.Permissions.Update(existing);
            }
        }

        if (permissionsToRemove.Any())
        {
            context.Permissions.RemoveRange(permissionsToRemove);
        }

        if (permissionsToAdd.Any())
        {
            await context.Permissions.AddRangeAsync(permissionsToAdd);
        }

        await context.SaveChangesAsync();
    }
}
