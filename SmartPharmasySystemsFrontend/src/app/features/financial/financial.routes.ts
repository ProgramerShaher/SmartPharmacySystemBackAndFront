import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const FINANCIAL_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        canActivate: [permissionGuard],
        data: { anyPermission: ['finance.dashboard.view', 'finance.treasury.view'] },
        loadComponent: () => import('./components/dashboard/financial-dashboard.component').then(m => m.FinancialDashboardComponent)
    },
    {
        path: 'ledger',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.journal.view' },
        loadComponent: () => import('./components/general-ledger/general-ledger.component').then(m => m.GeneralLedgerComponent)
    },
    {
        path: 'reports',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.statements.view' },
        loadComponent: () => import('./components/annual-reports/annual-reports.component').then(m => m.AnnualReportsComponent)
    }
];
