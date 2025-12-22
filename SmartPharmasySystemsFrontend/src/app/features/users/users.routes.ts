import { Routes } from '@angular/router';

export const USERS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/user-list/user-list.component').then(m => m.UserListComponent)
    },
    {
        path: 'add',
        loadComponent: () => import('./components/user-add-edit/user-add-edit.component').then(m => m.UserFormComponent)
    },
    {
        path: 'edit/:id',
        loadComponent: () => import('./components/user-add-edit/user-add-edit.component').then(m => m.UserFormComponent)
    }
];
