import { Routes } from '@angular/router';

export const FINANCE_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/expense-list/expense-list.component').then(m => m.ExpenseListComponent)
    },
    {
        path: 'add',
        loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/expense-add-edit/expense-add-edit.component').then(m => m.ExpenseAddEditComponent)
    }
];
