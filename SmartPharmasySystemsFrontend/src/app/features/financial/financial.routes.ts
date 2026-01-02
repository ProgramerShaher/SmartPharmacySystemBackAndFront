import { Routes } from '@angular/router';

export const FINANCIAL_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        loadComponent: () => import('./components/dashboard/financial-dashboard.component').then(m => m.FinancialDashboardComponent)
    },
    {
        path: 'ledger',
        loadComponent: () => import('./components/general-ledger/general-ledger.component').then(m => m.GeneralLedgerComponent)
    },
    {
        path: 'reports',
        loadComponent: () => import('./components/annual-reports/annual-reports.component').then(m => m.AnnualReportsComponent)
    }
];
