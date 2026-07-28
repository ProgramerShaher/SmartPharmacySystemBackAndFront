import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto, InventoryStockDto, WarehouseType } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ToastModule as ToastM } from 'primeng/toast';
import { SkeletonModule } from 'primeng/skeleton';

@Component({
    selector: 'app-warehouse-detail',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ToastModule,
        SkeletonModule
    ],
    templateUrl: './warehouse-detail.component.html',
    styleUrls: ['./warehouse-detail.component.scss'],
    providers: [MessageService]
})
export class WarehouseDetailComponent implements OnInit {

    // ── State ──────────────────────────────────────────────
    warehouse = signal<WarehouseDto | null>(null);
    allStocks  = signal<InventoryStockDto[]>([]);
    loading    = signal(false);
    searchTerm = signal('');
    showExpiringSoon = signal(false);

    // ── KPIs (computed) ────────────────────────────────────
    totalBatches   = computed(() => this.allStocks().length);
    totalQuantity  = computed(() => this.allStocks().reduce((s, x) => s + x.quantity, 0));
    expiredCount   = computed(() => this.allStocks().filter(x => x.daysUntilExpiry <= 0).length);
    expiringSoon   = computed(() => this.allStocks().filter(x => x.daysUntilExpiry > 0 && x.daysUntilExpiry <= 30).length);

    // ── Filtered list (computed) ───────────────────────────
    filteredStocks = computed(() => {
        let list = this.allStocks();
        const q  = this.searchTerm().trim().toLowerCase();
        if (q)   list = list.filter(s => s.medicineName.toLowerCase().includes(q) || s.batchNumber.toLowerCase().includes(q));
        if (this.showExpiringSoon()) list = list.filter(s => s.daysUntilExpiry > 0 && s.daysUntilExpiry <= 30);
        return list;
    });

    // ── FEFO set: IDs of batches with earliest expiry per medicine ─
    fefoSet = computed<Set<number>>(() => {
        const map = new Map<number, InventoryStockDto>();
        for (const s of this.allStocks()) {
            const cur = map.get(s.medicineId);
            if (!cur || s.daysUntilExpiry < cur.daysUntilExpiry) map.set(s.medicineId, s);
        }
        return new Set([...map.values()].map(s => s.id));
    });

    WarehouseType = WarehouseType;
    warehouseId!: number;
    private searchSubject = new Subject<string>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private warehouseService: WarehouseService,
        private messageService: MessageService
    ) {
        this.searchSubject.pipe(debounceTime(350), distinctUntilChanged())
            .subscribe(() => {}); // filtering is done via computed signals
    }

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.warehouseId = +params['id'];
            this.loadWarehouse();
            this.loadInventory();
        });
    }

    loadWarehouse() {
        this.warehouseService.getById(this.warehouseId).subscribe({
            next: w => this.warehouse.set(w),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'تعذّر تحميل بيانات المخزن' })
        });
    }

    loadInventory() {
        this.loading.set(true);
        this.warehouseService.getInventoryStocks(this.warehouseId).subscribe({
            next: data => {
                // Sort by daysUntilExpiry ASC (FEFO order)
                const sorted = [...data].sort((a, b) => a.daysUntilExpiry - b.daysUntilExpiry);
                this.allStocks.set(sorted);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'تعذّر تحميل المخزون' });
                this.loading.set(false);
            }
        });
    }

    onSearch(val: string) {
        this.searchTerm.set(val);
        this.searchSubject.next(val);
    }

    toggleExpiringSoon() {
        this.showExpiringSoon.set(!this.showExpiringSoon());
    }

    navigateToTransfer() {
        this.router.navigate(['/warehouses/transfers/create'], {
            queryParams: { sourceWarehouseId: this.warehouseId }
        });
    }

    goBack() { this.router.navigate(['/warehouses']); }

    // ── Helpers ────────────────────────────────────────────
    getExpiryBadge(stock: InventoryStockDto): 'danger' | 'warning' | 'success' {
        if (stock.daysUntilExpiry <= 0)  return 'danger';
        if (stock.daysUntilExpiry <= 30) return 'warning';
        return 'success';
    }

    getExpiryLabel(stock: InventoryStockDto): string {
        if (stock.daysUntilExpiry <= 0)  return 'منتهي';
        if (stock.daysUntilExpiry <= 30) return `${stock.daysUntilExpiry} يوم`;
        return 'صالح';
    }

    getTypeIcon(t: WarehouseType) {
        switch (t) {
            case WarehouseType.Main:    return 'pi pi-building';
            case WarehouseType.Branch:  return 'pi pi-sitemap';
            case WarehouseType.Damaged: return 'pi pi-exclamation-triangle';
            default: return 'pi pi-question';
        }
    }
}
