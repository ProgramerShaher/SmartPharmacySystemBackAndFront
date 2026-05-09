import { Routes } from '@angular/router';

export const PARTNERS_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'suppliers',
        pathMatch: 'full'
    },
    {
        path: 'suppliers',
        loadComponent: () => import('./components/supplier-list/supplier-list.component').then(m => m.SupplierListComponent)
    },
    {
        path: 'suppliers/create',
        redirectTo: 'suppliers',
        pathMatch: 'full'
    },
    {
        path: 'suppliers/edit/:id',
        redirectTo: 'suppliers',
        pathMatch: 'full'
    },
    {
        path: 'suppliers/detail/:id',
        loadComponent: () => import('./components/supplier-detail/supplier-detail.component').then(m => m.SupplierDetailComponent)
    },
    {
        path: 'suppliers/payments',
        redirectTo: 'suppliers',
        pathMatch: 'full'
    },
    {
        path: 'suppliers/statement/:id',
        loadComponent: () => import('./components/supplier-statement/supplier-statement.component').then(m => m.SupplierStatementComponent)
    }
];
