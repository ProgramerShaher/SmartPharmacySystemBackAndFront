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
    // المسارات الثابتة يجب أن تأتي قبل المسارات الديناميكية
    {
        path: 'suppliers/create',
        loadComponent: () => import('./components/supplier-add-edit/supplier-add-edit.component').then(m => m.SupplierAddEditComponent)
    },
    {
        path: 'suppliers/edit/:id',
        loadComponent: () => import('./components/supplier-add-edit/supplier-add-edit.component').then(m => m.SupplierAddEditComponent)
    },
    // المسار الديناميكي يجب أن يأتي في النهاية
    {
        path: 'suppliers/detail/:id',
        loadComponent: () => import('./components/supplier-detail/supplier-detail.component').then(m => m.SupplierDetailComponent)
    }
];
