import { Routes } from '@angular/router';

export const BRANCHES_ROUTES: Routes = [
    {
        path: 'dashboard',
        loadComponent: () => import('./components/branch-dashboard/branch-dashboard.component').then(m => m.BranchDashboardComponent)
    },
    {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./components/branch-list/branch-list.component').then(m => m.BranchListComponent)
    },
    // {
    //     path: ':id',
    //     loadComponent: () => import('./components/branch-details/branch-details.component').then(m => m.BranchDetailsComponent)
    // },
    {
        path: 'new',
        loadComponent: () => import('./components/branch-form/branch-form.component').then(m => m.BranchFormComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/branch-form/branch-form.component').then(m => m.BranchFormComponent)
    }
];
