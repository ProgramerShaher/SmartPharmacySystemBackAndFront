import { Routes } from '@angular/router';
import { WarehouseListComponent } from './components/warehouse-list/warehouse-list.component';

export const WAREHOUSE_ROUTES: Routes = [
    { path: '', component: WarehouseListComponent, title: 'المخازن' }
];
