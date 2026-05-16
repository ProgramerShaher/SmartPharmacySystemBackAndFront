import { Routes } from '@angular/router';

export const ACCOUNTING_ROUTES: Routes = [
  {
    path: '',
    children: [
      {
        path: 'chart',
        loadComponent: () => import('./components/chart-of-accounts/chart-of-accounts.component').then(m => m.ChartOfAccountsComponent),
        data: { title: 'شجرة الحسابات' }
      },
      {
        path: 'journal',
        loadComponent: () => import('./components/journal-entry-list/journal-entry-list.component').then(m => m.JournalEntryListComponent),
        data: { title: 'القيود اليومية' }
      },
      {
        path: 'trial-balance',
        loadComponent: () => import('./components/trial-balance/trial-balance.component').then(m => m.TrialBalanceComponent),
        data: { title: 'ميزان المراجعة' }
      },
      {
        path: 'financial-statements',
        loadComponent: () => import('./components/financial-statements/financial-statements.component').then(m => m.FinancialStatementsComponent),
        data: { title: 'القوائم المالية' }
      },
      { path: '', redirectTo: 'chart', pathMatch: 'full' }
    ]
  }
];
