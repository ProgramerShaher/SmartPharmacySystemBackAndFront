import { Routes } from '@angular/router';

export const CUSTOMERS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/customer-list/customer-list.component').then(m => m.CustomerListComponent)
    },
    {
        path: 'create',
        loadComponent: () => import('./components/customer-add-edit/customer-add-edit.component').then(m => m.CustomerAddEditComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/customer-add-edit/customer-add-edit.component').then(m => m.CustomerAddEditComponent)
    },
    {
        path: 'detail/:id',
        loadComponent: () => import('./components/customer-detail/customer-detail.component').then(m => m.CustomerDetailComponent)
    },
    {
        path: 'receipts',
        loadComponent: () => import('./components/customer-receipts/customer-receipts.component').then(m => m.CustomerReceiptsComponent)
    },
    {
        path: 'statement/:id',
        loadComponent: () => import('./components/customer-statement/customer-statement.component').then(m => m.CustomerStatementComponent)
    }
];
