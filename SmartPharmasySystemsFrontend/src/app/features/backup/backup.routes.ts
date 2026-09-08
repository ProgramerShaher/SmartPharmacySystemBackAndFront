import { Routes } from '@angular/router';
import { BackupDashboardComponent } from './components/backup-dashboard/backup-dashboard.component';
import { BackupSettingsComponent } from './components/backup-settings/backup-settings.component';
import { BackupHistoryComponent } from './components/backup-history/backup-history.component';
import { permissionGuard } from '../../core/guards/permission.guard';

export const BACKUP_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    component: BackupDashboardComponent,
    canActivate: [permissionGuard],
    data: { permission: 'admin.backup.manage' }
  },
  {
    path: 'settings',
    component: BackupSettingsComponent,
    canActivate: [permissionGuard],
    data: { permission: 'admin.backup.manage' }
  },
  {
    path: 'history',
    component: BackupHistoryComponent,
    canActivate: [permissionGuard],
    data: { permission: 'admin.backup.manage' }
  }
];
