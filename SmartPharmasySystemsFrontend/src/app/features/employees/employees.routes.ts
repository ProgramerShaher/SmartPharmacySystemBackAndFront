import { Routes } from '@angular/router';

export const EMPLOYEES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
    }
];
