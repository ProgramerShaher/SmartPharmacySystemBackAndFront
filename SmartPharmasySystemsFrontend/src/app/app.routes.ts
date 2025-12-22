import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
    },
    {
        path: '',
        component: MainLayoutComponent,
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
            },
            {
                path: 'inventory',
                loadChildren: () => import('./features/inventory/inventory.routes').then(m => m.INVENTORY_ROUTES)
            },
            {
                path: 'sales-invoices',
                loadChildren: () => import('./features/sales/sales.routes').then(m => m.SALES_ROUTES)
            },
            {
                path: 'purchase-invoices',
                loadChildren: () => import('./features/purchases/purchases.routes').then(m => m.PURCHASES_ROUTES)
            },
            {
                path: 'sales',
                loadChildren: () => import('./features/sales/sales.routes').then(m => m.SALES_ROUTES)
            },
            {
                path: 'purchases',
                loadChildren: () => import('./features/purchases/purchases.routes').then(m => m.PURCHASES_ROUTES)
            },
            {
                path: 'partners',
                loadChildren: () => import('./features/partners/partners.routes').then(m => m.PARTNERS_ROUTES)
            },
            {
                path: 'finance',
                loadChildren: () => import('./features/finance/finance.routes').then(m => m.FINANCE_ROUTES)
            },
            {
                path: 'users',
                loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES)
            },
            {
                path: 'system-alerts',
                loadChildren: () => import('./features/system-alerts/system-alerts.routes').then(m => m.SYSTEM_ALERTS_ROUTES)
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'dashboard'
    }
];
