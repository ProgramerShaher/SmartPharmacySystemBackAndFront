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
    },
    {
        path: 'quotations',
        canActivate: [permissionGuard],
        data: { anyPermission: ['sales.invoices.view', 'sales.invoices.create', 'sales.create', 'sales.manage'] },
        loadComponent: () => import('./components/quotation-list/quotation-list.component').then(m => m.QuotationListComponent)
    },
    {
        path: 'quotations/create',
        canActivate: [permissionGuard],
        data: { anyPermission: ['sales.invoices.create', 'sales.create', 'sales.manage'] },
        loadComponent: () => import('./components/quotation-create/quotation-create.component').then(m => m.QuotationCreateComponent)
    },
    {
        path: 'quotations/edit/:id',
        canActivate: [permissionGuard],
        data: { anyPermission: ['sales.invoices.edit', 'sales.manage'] },
        loadComponent: () => import('./components/quotation-create/quotation-create.component').then(m => m.QuotationCreateComponent)
    },
    {
        path: 'quotations/:id',
        canActivate: [permissionGuard],
        data: { anyPermission: ['sales.invoices.view', 'sales.invoices.create', 'sales.manage'] },
        loadComponent: () => import('./components/quotation-details/quotation-details.component').then(m => m.QuotationDetailsComponent)
    }

];
