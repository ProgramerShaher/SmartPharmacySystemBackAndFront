import { Routes } from '@angular/router';

export const FINANCE_ROUTES: Routes = [
    {
        path: '',
        redirectTo: 'expenses',
        pathMatch: 'full'
    },
    {
        path: 'expenses',
        loadComponent: () => import('./components/expense-list/expense-list.component').then(m => m.ExpenseListComponent)
    },
    {
        path: 'expenses/add',
        loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    },
    {
      path: 'expenses/edit/:id',
      loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    },
    {
        path: 'expense-categories',
        loadComponent: () => import('./components/expense-category-list/expense-category-list.component').then(m => m.ExpenseCategoryListComponent)
  }
];
