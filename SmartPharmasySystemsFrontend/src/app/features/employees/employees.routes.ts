import { Routes } from '@angular/router';

export const EMPLOYEES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
    },
    {
        path: 'attendance',
        loadComponent: () => import('./components/attendance/attendance.component').then(m => m.AttendanceComponent)
    },
    {
        path: 'payroll',
        loadComponent: () => import('./components/payroll/payroll.component').then(m => m.PayrollComponent)
    }
];
