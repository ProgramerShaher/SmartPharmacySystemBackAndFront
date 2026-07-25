import { Routes } from '@angular/router';
import { WarehouseListComponent } from './components/warehouse-list/warehouse-list.component';
import { StockTransferListComponent } from './components/stock-transfer-list/stock-transfer-list.component';
import { CreateStockTransferComponent } from './components/create-stock-transfer/create-stock-transfer.component';
import { DamagedGoodsListComponent } from './components/damaged-goods-list/damaged-goods-list.component';
import { CreateDamagedGoodsComponent } from './components/create-damaged-goods/create-damaged-goods.component';

export const WAREHOUSE_ROUTES: Routes = [
    { path: '', component: WarehouseListComponent, title: 'المخازن' },
    { path: 'transfers', component: StockTransferListComponent, title: 'التحويلات المخزنية' },
    { path: 'transfers/create', component: CreateStockTransferComponent, title: 'إنشاء طلب تحويل' },
    { path: 'damaged', component: DamagedGoodsListComponent, title: 'إهلاك التوالف' },
    { path: 'damaged/create', component: CreateDamagedGoodsComponent, title: 'تسجيل تالف' }
];
