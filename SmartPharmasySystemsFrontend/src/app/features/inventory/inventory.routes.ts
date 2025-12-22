import { Routes } from '@angular/router';

export const INVENTORY_ROUTES: Routes = [
    {
        path: 'medicines',
        loadComponent: () => import('./components/medicine-list/medicine-list.component').then(m => m.MedicineListComponent)
    },
    {
        path: 'medicines/details/:id',
        loadComponent: () => import('./components/medicine-details/medicine-details.component').then(m => m.MedicineDetailsComponent)
    },
    {
        path: 'categories',
        loadComponent: () => import('./components/category-list/category-list.component').then(m => m.CategoryListComponent)
    },
    {
        path: 'movements',
        loadComponent: () => import('./components/movement-history/movement-history.component').then(m => m.MovementHistoryComponent)
    },
    {
        path: 'batches',
        loadComponent: () => import('./components/batch-list/batch-list.component').then(m => m.BatchListComponent)
    },
    {
        path: 'batches/details/:id',
        loadComponent: () => import('./components/batch-details/batch-details.component').then(m => m.BatchDetailsComponent)
    }
];
