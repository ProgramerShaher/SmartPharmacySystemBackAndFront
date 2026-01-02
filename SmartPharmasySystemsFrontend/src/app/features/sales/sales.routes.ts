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
        loadComponent: () => import('./components/sale-invoice-add/sale-invoice-add.component').then(m => m.SaleInvoiceAddComponent)
    },
    {
        path: 'returns',
        loadComponent: () => import('./components/sales-return-list/sales-return-list.component').then(m => m.SalesReturnListComponent)
    },
    {
        path: 'returns/create',
        loadComponent: () => import('./components/sales-return-create/sales-return-create.component').then(m => m.SalesReturnCreateComponent)
    },
    {
        path: 'returns/:id', // Detail view
        loadComponent: () => import('./components/sales-return-create/sales-return-create.component').then(m => m.SalesReturnCreateComponent) // Re-use create for view/edit or make separate? Let's assume re-use for now or specific detail.
    },
    {
        path: ':id',
        loadComponent: () => import('./components/sales-invoice-details/sales-invoice-details.component').then(m => m.SalesInvoiceDetailsComponent)
    }
];
