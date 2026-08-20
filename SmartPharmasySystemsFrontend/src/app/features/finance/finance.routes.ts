import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const FINANCE_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'expenses',
        pathMatch: 'full'
    },
    {
        path: 'expenses',
        canActivate: [permissionGuard],
        data: { permission: 'finance.expenses.view' },
        loadComponent: () => import('./components/expense-list/expense-list.component').then(m => m.ExpenseListComponent)
    },
    {
        path: 'expenses/add',
        canActivate: [permissionGuard],
        data: { permission: 'finance.expenses.create' },
        loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    },
    {
      path: 'expenses/edit/:id',
      canActivate: [permissionGuard],
      data: { permission: 'finance.expenses.create' },
      loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    },
    {
        path: 'expense-categories',
        canActivate: [permissionGuard],
        data: { anyPermission: ['finance.expenses.view', 'finance.expenses.create'] },
        loadComponent: () => import('./components/expense-category-list/expense-category-list.component').then(m => m.ExpenseCategoryListComponent)
  }
];
