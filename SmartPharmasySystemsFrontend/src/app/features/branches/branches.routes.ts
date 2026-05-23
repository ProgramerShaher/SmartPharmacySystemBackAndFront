import { Routes } from '@angular/router';

export const BRANCHES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/branch-list/branch-list.component').then(m => m.BranchListComponent)
    }
];
