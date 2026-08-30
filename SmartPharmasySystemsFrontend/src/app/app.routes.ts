import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { licenseGuard } from './core/guards/license.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { UnauthorizedComponent } from './features/unauthorized/unauthorized.component';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth/login',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
    },
    {
        // صفحة 403 — خارج الـ Layout الرئيسي لعرضها بشكل مستقل
        path: 'unauthorized',
        component: UnauthorizedComponent
    },
    {
        // شاشة التفعيل — متاحة للجميع قبل تسجيل الدخول
        path: 'lock-screen',
        loadComponent: () =>
            import('./features/lock-screen/lock-screen.component')
            .then(m => m.LockScreenComponent)
    },
    {
        path: '',
        component: MainLayoutComponent,
        // licenseGuard runs FIRST — if not activated, redirect to /lock-screen
        // authGuard runs SECOND  — if not logged in, redirect to /auth/login
        canActivate: [licenseGuard, authGuard],
        children: [
            // ---- لوحة التحكم (بدون قيد صلاحية — للجميع)
            {
                path: 'dashboard',
                loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
            },
            // ---- المخزون
            {
                path: 'inventory',
                canActivate: [permissionGuard],
                data: { permission: 'inventory.medicines.view' },
                loadChildren: () => import('./features/inventory/inventory.routes').then(m => m.INVENTORY_ROUTES)
            },
            // ---- المبيعات
            {
                path: 'sales',
                canActivate: [permissionGuard],
                data: { anyPermission: ['sales.invoices.view', 'sales.invoices.create', 'sales.create', 'sales.manage'] },
                loadChildren: () => import('./features/sales/sales.routes').then(m => m.SALES_ROUTES)
            },
            // ---- نقطة البيع / الكاشير (POS)
            {
                path: 'pos',
                redirectTo: 'sales/create',
                pathMatch: 'full'
            },
            // ---- المشتريات
            {
                path: 'purchases',
                canActivate: [permissionGuard],
                data: { permission: 'purchases.invoices.view' },
                loadChildren: () => import('./features/purchases/purchases.routes').then(m => m.PURCHASES_ROUTES)
            },
            // ---- الفروع
            {
                path: 'branches',
                canActivate: [permissionGuard],
                data: { permission: 'branches.manage' },
                loadChildren: () => import('./features/branches/branches.routes').then(m => m.BRANCHES_ROUTES)
            },
            // ---- الأقسام
            {
                path: 'departments',
                canActivate: [permissionGuard],
                data: { permission: 'branches.manage' },
                loadChildren: () => import('./features/departments/departments.routes').then(m => m.DEPARTMENTS_ROUTES)
            },
            // ---- الموظفين
            {
                path: 'employees',
                canActivate: [permissionGuard],
                data: { permission: 'hr.employees.view' },
                loadChildren: () => import('./features/employees/employees.routes').then(m => m.EMPLOYEES_ROUTES)
            },
            // ---- المخازن
            {
                path: 'warehouses',
                canActivate: [permissionGuard],
                data: { permission: 'inventory.warehouses.view' },
                loadChildren: () => import('./features/warehouses/warehouses.routes').then(m => m.WAREHOUSE_ROUTES)
            },
            // ---- العملاء
            {
                path: 'customers',
                canActivate: [permissionGuard],
                data: { permission: 'partners.customers.view' },
                loadChildren: () => import('./features/customers/customers.routes').then(m => m.CUSTOMERS_ROUTES)
            },
            // ---- الشركاء والموردين
            {
                path: 'partners',
                canActivate: [permissionGuard],
                data: { permission: 'partners.view' },
                loadChildren: () => import('./features/partners/partners.routes').then(m => m.PARTNERS_ROUTES)
            },
            // ---- المالية
            {
                path: 'financial',
                canActivate: [permissionGuard],
                data: { anyPermission: ['finance.treasury.view', 'finance.dashboard.view'] },
                loadChildren: () => import('./features/financial/financial.routes').then(m => m.FINANCIAL_ROUTES)
            },
            // ---- المصروفات
            {
                path: 'finance',
                canActivate: [permissionGuard],
                data: { anyPermission: ['finance.expenses.view', 'finance.expenses.create'] },
                loadChildren: () => import('./features/finance/finance.routes').then(m => m.FINANCE_ROUTES)
            },
            // ---- المستخدمين
            {
                path: 'users',
                canActivate: [permissionGuard],
                data: { anyPermission: ['admin.users.view', 'admin.users.manage'] },
                loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES)
            },
            // ---- الأدوار والصلاحيات
            {
                path: 'roles',
                canActivate: [permissionGuard],
                data: { anyPermission: ['access.roles.view', 'admin.roles.manage'] },
                loadChildren: () => import('./features/roles/roles.routes').then(m => m.rolesRoutes)
            },
            // ---- التنبيهات
            {
                path: 'system-alerts',
                canActivate: [permissionGuard],
                data: { permission: 'admin.alerts.manage' },
                loadChildren: () => import('./features/system-alerts/system-alerts.routes').then(m => m.SYSTEM_ALERTS_ROUTES)
            },
            // ---- التقارير
            {
                path: 'reports',
                canActivate: [permissionGuard],
                data: { permission: 'reports.sales.view' },
                loadChildren: () => import('./features/reports/reports.routes').then(m => m.REPORTS_ROUTES)
            },
            // ---- الطلبات الأونلاين
            {
                path: 'online-orders',
                canActivate: [permissionGuard],
                data: { permission: 'online_orders.manage' },
                loadChildren: () => import('./features/online-orders/online-orders.routes').then(m => m.ONLINE_ORDERS_ROUTES)
            },
            // ---- المحاسبة
            {
                path: 'accounting',
                canActivate: [permissionGuard],
                data: { anyPermission: ['accounting.chart.view', 'accounting.chart.manage', 'accounting.statements.view'] },
                loadChildren: () => import('./features/accounting/accounting.routes').then(m => m.ACCOUNTING_ROUTES)
            },
            // ---- الورديات
            {
                path: 'shifts',
                canActivate: [permissionGuard],
                data: { anyPermission: ['sales.shifts.manage', 'sales.invoices.create'] },
                loadChildren: () => import('./features/shifts/shifts.module').then(m => m.ShiftsModule)
            },
            // ---- الإعدادات
            {
                path: 'settings',
                canActivate: [permissionGuard],
                data: { anyPermission: ['settings.general.view', 'admin.settings.manage'] },
                loadChildren: () => import('./features/settings/settings.routes').then(m => m.SETTINGS_ROUTES)
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'dashboard'
    }
];
