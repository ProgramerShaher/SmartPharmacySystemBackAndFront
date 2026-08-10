using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Data.Seeders;

public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
        {
            return; // Already seeded
        }

        var permissions = new List<Permission>
        {
            // ===== وحدة المبيعات (Sales) =====
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Dashboard", ActionAr = "عرض لوحة تحكم المبيعات", Code = "sales.dashboard.view", Icon = "pi pi-chart-line" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Invoices", ActionAr = "عرض قائمة الفواتير", Code = "sales.invoices.view", Icon = "pi pi-list" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Create Invoice", ActionAr = "إنشاء فاتورة بيع جديدة", Code = "sales.invoices.create", Icon = "pi pi-plus" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Edit Invoice", ActionAr = "تعديل فاتورة", Code = "sales.invoices.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Delete Invoice", ActionAr = "حذف فاتورة", Code = "sales.invoices.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Approve Invoice", ActionAr = "اعتماد وترحيل الفاتورة", Code = "sales.invoices.approve", Icon = "pi pi-check-circle" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Print Invoice", ActionAr = "طباعة الفاتورة", Code = "sales.invoices.print", Icon = "pi pi-print" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Export Invoices", ActionAr = "تصدير الفواتير", Code = "sales.invoices.export", Icon = "pi pi-file-export" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Profit", ActionAr = "عرض الربح في الفاتورة", Code = "sales.invoices.view_profit", Icon = "pi pi-dollar" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Change Price", ActionAr = "تعديل سعر البيع يدوياً", Code = "sales.invoices.change_price", Icon = "pi pi-tag" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Add Discount", ActionAr = "إضافة خصومات", Code = "sales.invoices.add_discount", Icon = "pi pi-percentage" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "View Returns", ActionAr = "عرض إرجاعات المبيعات", Code = "sales.returns.view", Icon = "pi pi-undo" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Create Return", ActionAr = "إنشاء إرجاع مبيعات", Code = "sales.returns.create", Icon = "pi pi-plus" },
            new Permission { Module = "Sales", ModuleAr = "المبيعات", Action = "Approve Return", ActionAr = "اعتماد الإرجاع", Code = "sales.returns.approve", Icon = "pi pi-check" },

            // ===== وحدة المشتريات (Purchases) =====
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "View Dashboard", ActionAr = "عرض لوحة المشتريات", Code = "purchases.dashboard.view", Icon = "pi pi-chart-bar" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "View Invoices", ActionAr = "عرض فواتير الشراء", Code = "purchases.invoices.view", Icon = "pi pi-list" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Create Invoice", ActionAr = "إنشاء فاتورة شراء", Code = "purchases.invoices.create", Icon = "pi pi-plus" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Edit Invoice", ActionAr = "تعديل فاتورة", Code = "purchases.invoices.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Delete Invoice", ActionAr = "حذف فاتورة", Code = "purchases.invoices.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Approve Invoice", ActionAr = "اعتماد الفاتورة", Code = "purchases.invoices.approve", Icon = "pi pi-check-circle" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Print Invoice", ActionAr = "طباعة الفاتورة", Code = "purchases.invoices.print", Icon = "pi pi-print" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "View Returns", ActionAr = "عرض إرجاعات المشتريات", Code = "purchases.returns.view", Icon = "pi pi-undo" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Create Return", ActionAr = "إنشاء إرجاع مشتريات", Code = "purchases.returns.create", Icon = "pi pi-plus" },
            new Permission { Module = "Purchases", ModuleAr = "المشتريات", Action = "Approve Return", ActionAr = "اعتماد إرجاع المشتريات", Code = "purchases.returns.approve", Icon = "pi pi-check" },

            // ===== وحدة المخزون (Inventory) =====
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Dashboard", ActionAr = "عرض لوحة المخزون", Code = "inventory.dashboard.view", Icon = "pi pi-box" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Medicines", ActionAr = "عرض الأدوية", Code = "inventory.medicines.view", Icon = "pi pi-list" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Create Medicine", ActionAr = "إضافة دواء جديد", Code = "inventory.medicines.create", Icon = "pi pi-plus" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Edit Medicine", ActionAr = "تعديل بيانات دواء", Code = "inventory.medicines.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Delete Medicine", ActionAr = "حذف دواء", Code = "inventory.medicines.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Import Medicines", ActionAr = "استيراد بيانات الأدوية", Code = "inventory.medicines.import", Icon = "pi pi-upload" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Batches", ActionAr = "عرض الدفعات (Batches)", Code = "inventory.batches.view", Icon = "pi pi-tags" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Batches", ActionAr = "إدارة الدفعات", Code = "inventory.batches.manage", Icon = "pi pi-cog" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Adjustments", ActionAr = "عرض تسويات المخزون", Code = "inventory.adjustments.view", Icon = "pi pi-sort-alt" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Create Adjustment", ActionAr = "إنشاء تسوية مخزون", Code = "inventory.adjustments.create", Icon = "pi pi-plus" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Approve Adjustment", ActionAr = "اعتماد التسوية", Code = "inventory.adjustments.approve", Icon = "pi pi-check-circle" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "View Warehouses", ActionAr = "عرض المستودعات", Code = "inventory.warehouses.view", Icon = "pi pi-building" },
            new Permission { Module = "Inventory", ModuleAr = "المخزون", Action = "Manage Warehouses", ActionAr = "إدارة المستودعات", Code = "inventory.warehouses.manage", Icon = "pi pi-cog" },

            // ===== وحدة العملاء (Customers) =====
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "View Customers", ActionAr = "عرض العملاء", Code = "customers.view", Icon = "pi pi-users" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "Create Customer", ActionAr = "إنشاء عميل", Code = "customers.create", Icon = "pi pi-user-plus" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "Edit Customer", ActionAr = "تعديل عميل", Code = "customers.edit", Icon = "pi pi-user-edit" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "Delete Customer", ActionAr = "حذف عميل", Code = "customers.delete", Icon = "pi pi-user-minus" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "View Statement", ActionAr = "عرض كشف الحساب", Code = "customers.statement.view", Icon = "pi pi-file" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "View Receipts", ActionAr = "عرض سندات القبض", Code = "customers.receipts.view", Icon = "pi pi-receipt" },
            new Permission { Module = "Customers", ModuleAr = "العملاء", Action = "Create Receipt", ActionAr = "إنشاء سند قبض", Code = "customers.receipts.create", Icon = "pi pi-plus" },

            // ===== وحدة الموردين (Suppliers) =====
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "View Suppliers", ActionAr = "عرض الموردين", Code = "suppliers.view", Icon = "pi pi-truck" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "Create Supplier", ActionAr = "إنشاء مورد", Code = "suppliers.create", Icon = "pi pi-plus" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "Edit Supplier", ActionAr = "تعديل مورد", Code = "suppliers.edit", Icon = "pi pi-pencil" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "Delete Supplier", ActionAr = "حذف مورد", Code = "suppliers.delete", Icon = "pi pi-trash" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "View Statement", ActionAr = "عرض كشف الحساب", Code = "suppliers.statement.view", Icon = "pi pi-file" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "View Payments", ActionAr = "عرض سندات الصرف", Code = "suppliers.payments.view", Icon = "pi pi-money-bill" },
            new Permission { Module = "Suppliers", ModuleAr = "الموردون", Action = "Create Payment", ActionAr = "إنشاء سند صرف", Code = "suppliers.payments.create", Icon = "pi pi-plus" },

            // ===== وحدة الخزينة (Treasury) =====
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "View Balance", ActionAr = "عرض رصيد الخزينة", Code = "treasury.view", Icon = "pi pi-wallet" },
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "View Income", ActionAr = "عرض الإيرادات", Code = "treasury.income.view", Icon = "pi pi-arrow-down-left" },
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "Create Income", ActionAr = "إضافة إيراد", Code = "treasury.income.create", Icon = "pi pi-plus" },
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "View Expense", ActionAr = "عرض المصروفات", Code = "treasury.expense.view", Icon = "pi pi-arrow-up-right" },
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "Create Expense", ActionAr = "إضافة مصروف", Code = "treasury.expense.create", Icon = "pi pi-plus" },
            new Permission { Module = "Treasury", ModuleAr = "الخزينة", Action = "View Reports", ActionAr = "تقارير الخزينة", Code = "treasury.reports.view", Icon = "pi pi-chart-pie" },

            // ===== وحدة التقارير (Reports) =====
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Sales Reports", ActionAr = "تقارير المبيعات", Code = "reports.sales.view", Icon = "pi pi-chart-line" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Purchases Reports", ActionAr = "تقارير المشتريات", Code = "reports.purchases.view", Icon = "pi pi-chart-bar" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Inventory Reports", ActionAr = "تقارير المخزون", Code = "reports.inventory.view", Icon = "pi pi-box" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "View Financial Reports", ActionAr = "تقارير مالية و أرباح", Code = "reports.profits.view", Icon = "pi pi-dollar" },
            new Permission { Module = "Reports", ModuleAr = "التقارير", Action = "Export Reports", ActionAr = "تصدير التقارير", Code = "reports.export", Icon = "pi pi-file-export" },

            // ===== وحدة الموظفين (Employees) =====
            new Permission { Module = "Employees", ModuleAr = "الموظفون", Action = "View Employees", ActionAr = "عرض الموظفين", Code = "employees.view", Icon = "pi pi-users" },
            new Permission { Module = "Employees", ModuleAr = "الموظفون", Action = "Manage Employees", ActionAr = "إدارة الموظفين (إضافة/تعديل)", Code = "employees.manage", Icon = "pi pi-user-edit" },
            new Permission { Module = "Employees", ModuleAr = "الموظفون", Action = "Manage Salaries", ActionAr = "إدارة الرواتب", Code = "employees.salary.manage", Icon = "pi pi-money-bill" },

            // ===== وحدة الإعدادات والصلاحيات (Settings & Access) =====
            new Permission { Module = "Settings", ModuleAr = "الإعدادات", Action = "View Settings", ActionAr = "عرض الإعدادات العامة", Code = "settings.general.view", Icon = "pi pi-cog" },
            new Permission { Module = "Settings", ModuleAr = "الإعدادات", Action = "Edit Settings", ActionAr = "تعديل الإعدادات العامة", Code = "settings.general.edit", Icon = "pi pi-cog" },
            new Permission { Module = "Settings", ModuleAr = "الإعدادات", Action = "View Audit Log", ActionAr = "عرض سجل العمليات", Code = "settings.audit_log.view", Icon = "pi pi-history" },
            
            new Permission { Module = "Access", ModuleAr = "الصلاحيات", Action = "View Roles", ActionAr = "عرض الأدوار", Code = "access.roles.view", Icon = "pi pi-id-card" },
            new Permission { Module = "Access", ModuleAr = "الصلاحيات", Action = "Manage Roles", ActionAr = "إدارة الأدوار (إضافة/تعديل)", Code = "access.roles.manage", Icon = "pi pi-shield" },
            new Permission { Module = "Access", ModuleAr = "الصلاحيات", Action = "View Users", ActionAr = "عرض المستخدمين", Code = "access.users.view", Icon = "pi pi-users" },
            new Permission { Module = "Access", ModuleAr = "الصلاحيات", Action = "Manage Users", ActionAr = "إدارة المستخدمين", Code = "access.users.manage", Icon = "pi pi-user-edit" },
            new Permission { Module = "Access", ModuleAr = "الصلاحيات", Action = "Assign Permissions", ActionAr = "تعيين الصلاحيات والاستثناءات", Code = "access.permissions.assign", Icon = "pi pi-key" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
}
