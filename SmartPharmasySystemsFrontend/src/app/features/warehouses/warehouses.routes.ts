import { Routes } from '@angular/router';
import { WarehouseListComponent } from './components/warehouse-list/warehouse-list.component';
import { InternalTransferListComponent } from './components/internal-transfer-list/internal-transfer-list.component';
import { CreateStockTransferComponent } from './components/create-stock-transfer/create-stock-transfer.component';
import { DamagedGoodsListComponent } from './components/damaged-goods-list/damaged-goods-list.component';
import { CreateDamagedGoodsComponent } from './components/create-damaged-goods/create-damaged-goods.component';
import { WarehouseDetailComponent } from './components/warehouse-detail/warehouse-detail.component';
import { ExternalTransferListComponent } from './components/external-transfer-list/external-transfer-list.component';

export const WAREHOUSE_ROUTES: Routes = [
    { path: '', component: WarehouseListComponent, title: 'المخازن' },
    { path: 'transfers', redirectTo: 'transfers/internal', pathMatch: 'full' },
    { path: 'transfers/internal', component: InternalTransferListComponent, title: 'التحويلات الداخلية' },
    { path: 'transfers/internal/create', component: CreateStockTransferComponent, title: 'إنشاء تحويل داخلي' },
    { path: 'transfers/external', component: ExternalTransferListComponent, title: 'التحويلات الخارجية' },
    { path: 'transfers/external/create', component: CreateStockTransferComponent, title: 'إنشاء طلب تحويل خارجي' },
    { path: 'damaged', component: DamagedGoodsListComponent, title: 'إهلاك التوالف' },
    { path: 'damaged/create', component: CreateDamagedGoodsComponent, title: 'تسجيل تالف' },
    { path: ':id', component: WarehouseDetailComponent, title: 'تفاصيل المخزن' }
];
