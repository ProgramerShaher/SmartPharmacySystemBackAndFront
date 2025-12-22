import { Routes } from '@angular/router';

export const SYSTEM_ALERTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/alert-list/alert-list.component').then(
        (m) => m.AlertListComponent
      ),
  },
  {
    path: 'detail/:id',
    loadComponent: () =>
      import('./components/alert-detail/alert-detail.component').then(
        (m) => m.AlertDetailComponent
      ),
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./components/alert-add-edit/alert-add-edit.component').then(
        (m) => m.AlertAddEditComponent
      ),
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./components/alert-add-edit/alert-add-edit.component').then(
        (m) => m.AlertAddEditComponent
      ),
  },
];
