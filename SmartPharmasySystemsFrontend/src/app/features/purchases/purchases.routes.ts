import { Routes } from '@angular/router';

export const PURCHASES_ROUTES: Routes = [
    // Returns Routes (Must be before generic :id)
    {
        path: 'returns',
        loadComponent: () => import('./components/purchase-return-list/purchase-return-list.component').then(m => m.PurchaseReturnListComponent)
    },
    {
        path: 'returns/create',
        loadComponent: () => import('./components/purchase-return-create/purchase-return-create.component').then(m => m.PurchaseReturnCreateComponent)
    },

    // Invoice Routes
    {
        path: 'create',
        loadComponent: () => import('./components/purchase-create/purchase-create.component').then(m => m.PurchaseInvoiceCreateComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/purchase-create/purchase-create.component').then(m => m.PurchaseInvoiceCreateComponent)
    },
    {
        path: ':id',
        loadComponent: () => import('./components/purchase-details/purchase-details.component').then(m => m.PurchaseInvoiceDetailsComponent)
    },
    {
        path: '',
        loadComponent: () => import('./components/purchase-list/purchase-list.component').then(m => m.PurchaseInvoiceListComponent)
    }
];
