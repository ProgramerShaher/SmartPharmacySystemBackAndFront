import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/dashboard-stats/dashboard-stats.component').then(m => m.DashboardStatsComponent)
    }
];
