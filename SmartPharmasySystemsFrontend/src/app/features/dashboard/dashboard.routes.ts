import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./dashboard.component').then(m => m.DashboardComponent)
    },
    {
        path: 'master',
        loadComponent: () => import('./master-dashboard.component').then(m => m.MasterDashboardComponent)
  }
];
