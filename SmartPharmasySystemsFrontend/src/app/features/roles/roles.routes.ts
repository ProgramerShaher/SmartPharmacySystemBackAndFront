import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';
import { RolesListComponent } from './components/roles-list/roles-list.component';

export const rolesRoutes: Routes = [
    {
        path: '',
        component: RolesListComponent,
        // canActivate: [permissionGuard],
        // data: { permission: 'roles.view' }
    }
];
