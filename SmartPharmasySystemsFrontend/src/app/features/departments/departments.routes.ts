import { Routes } from '@angular/router';

export const DEPARTMENTS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/department-list/department-list.component').then(m => m.DepartmentListComponent)
    }
];
