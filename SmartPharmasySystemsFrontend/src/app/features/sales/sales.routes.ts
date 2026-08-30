import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const SALES_ROUTES: Routes = [
    {
        path: '',
        canActivate: [permissionGuard],
        data: { anyPermission: ['sales.invoices.view', 'sales.invoices.create'] },
        loadComponent: () => import('./components/sales-invoice-list/sales-invoice-list.component').then(m => m.SalesInvoiceListComponent)
    },
    {
        path: 'create',
        canActivate: [permissionGuard],
        data: { permission: 'sales.invoices.create' },
        loadComponent: () => import('./components/sale-invoice-create/sale-invoice-create.component').then(m => m.SaleInvoiceCreateComponent)
    },
    {
        path: 'daily-closing',
        canActivate: [permissionGuard],
        data: { permission: 'sales.daily_closing.manage' },
        loadComponent: () => import('./components/daily-closing/daily-closing.component').then(m => m.DailyClosingComponent)
    },
    {
        path: 'edit/:id',
        canActivate: [permissionGuard],
        data: { permission: 'sales.invoices.edit' },
        loadComponent: () => import('./components/sale-invoice-create/sale-invoice-create.component').then(m => m.SaleInvoiceCreateComponent)
    },
    {
        path: 'returns',
        canActivate: [permissionGuard],
        data: { permission: 'sales.returns.view' },
        loadComponent: () => import('./components/sales-return-list/sales-return-list.component').then(m => m.SalesReturnListComponent)
    },
    {
        path: 'returns/create',
        canActivate: [permissionGuard],
        data: { permission: 'sales.returns.create' },
        loadComponent: () => import('./components/sales-return-create/sales-return-create.component').then(m => m.SalesReturnCreateComponent)
    },
    {
        path: 'returns/:id', // Detail view
        canActivate: [permissionGuard],
        data: { permission: 'sales.returns.view' },
        loadComponent: () => import('./components/sales-return-create/sales-return-create.component').then(m => m.SalesReturnCreateComponent)
    },
    {
        path: 'pricelists',
        loadComponent: () => import('./components/pricelist-list/pricelist-list.component').then(m => m.PricelistListComponent)
    }

];
