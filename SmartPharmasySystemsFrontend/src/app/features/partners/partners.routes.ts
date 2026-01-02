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
        loadComponent: () => import('./components/supplier-add-edit/supplier-add-edit.component').then(m => m.SupplierAddEditComponent)
    },
    {
        path: 'suppliers/edit/:id',
        loadComponent: () => import('./components/supplier-add-edit/supplier-add-edit.component').then(m => m.SupplierAddEditComponent)
    },
    {
        path: 'suppliers/detail/:id',
        loadComponent: () => import('./components/supplier-detail/supplier-detail.component').then(m => m.SupplierDetailComponent)
    },
    {
        path: 'suppliers/payments',
        loadComponent: () => import('./components/supplier-payments/supplier-payments.component').then(m => m.SupplierPaymentsComponent)
    },
    {
        path: 'suppliers/statement/:id',
        loadComponent: () => import('./components/supplier-statement/supplier-statement.component').then(m => m.SupplierStatementComponent)
    }
];
