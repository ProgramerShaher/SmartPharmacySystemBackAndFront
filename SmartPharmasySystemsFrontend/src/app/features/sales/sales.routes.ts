import { Routes } from '@angular/router';

export const SALES_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/sales-invoice-list/sales-invoice-list.component').then(m => m.SalesInvoiceListComponent)
    },
    {
        path: 'create',
        loadComponent: () => import('./components/sale-invoice-create/sale-invoice-create.component').then(m => m.SalesInvoiceCreateComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/sale-invoice-create/sale-invoice-create.component').then(m => m.SalesInvoiceCreateComponent)
    },
    {
        path: ':id',
        loadComponent: () => import('./components/sales-invoice-details/sales-invoice-details.component').then(m => m.SalesInvoiceDetailsComponent)
    }
];
