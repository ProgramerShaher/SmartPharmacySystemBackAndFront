import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const ACCOUNTING_ROUTES: Routes = [
  {
    path: '',
    children: [
      {
        path: 'chart',
        canActivate: [permissionGuard],
        data: { anyPermission: ['accounting.chart.view', 'accounting.chart.manage'], title: 'شجرة الحسابات' },
        loadComponent: () => import('./components/chart-of-accounts/chart-of-accounts.component').then(m => m.ChartOfAccountsComponent)
      },
      {
        path: 'journal',
        canActivate: [permissionGuard],
        data: { anyPermission: ['accounting.journal.view', 'accounting.journal.create'], title: 'القيود اليومية' },
        loadComponent: () => import('./components/journal-entry-list/journal-entry-list.component').then(m => m.JournalEntryListComponent)
      },
      {
        path: 'trial-balance',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.statements.view', title: 'ميزان المراجعة' },
        loadComponent: () => import('./components/trial-balance/trial-balance.component').then(m => m.TrialBalanceComponent)
      },
      {
        path: 'financial-statements',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.statements.view', title: 'القوائم المالية' },
        loadComponent: () => import('./components/financial-statements/financial-statements.component').then(m => m.FinancialStatementsComponent)
      },
      {
        path: 'balances',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.statements.view', title: 'أرصدة الحسابات' },
        loadComponent: () => import('./components/accounts-balances/accounts-balances.component').then(m => m.AccountsBalancesComponent)
      },
      {
        path: 'daily-closing',
        canActivate: [permissionGuard],
        data: { permission: 'sales.daily_closing.manage', title: 'الإقفال اليومي' },
        loadComponent: () => import('./components/daily-closings/daily-closings.component').then(m => m.DailyClosingsComponent)
      },
      {
        path: 'financial-periods',
        canActivate: [permissionGuard],
        data: { permission: 'accounting.statements.view', title: 'الفترات المحاسبية' },
        loadComponent: () => import('./components/financial-periods/financial-periods.component').then(m => m.FinancialPeriodsComponent)
      },
      { path: '', redirectTo: 'chart', pathMatch: 'full' }
    ]
  }
];
