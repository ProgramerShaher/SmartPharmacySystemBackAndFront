import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { PaginatorModule } from 'primeng/paginator';
import { MessageService } from 'primeng/api';
import { InventoryService } from '../../services/inventory.service';
import { MedicineBatch } from '../../../../core/models';
import { DialogModule } from 'primeng/dialog';
import { BatchActionsDialogComponent } from '../batch-actions-dialog/batch-actions-dialog.component';
import { BatchDetailsComponent } from '../batch-details/batch-details.component';

@Component({
    selector: 'app-batch-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        DropdownModule,
        TagModule,
        TooltipModule,
        PaginatorModule,
        DialogModule,
        BatchActionsDialogComponent,
        BatchDetailsComponent
    ],
    templateUrl: './batch-list.component.html',
    styleUrls: ['./batch-list.component.scss']
})
export class BatchListComponent implements OnInit {
    batches: MedicineBatch[] = [];
    filteredBatches: MedicineBatch[] = [];
    loading = true;

    // View Mode: Table vs Grid Cards
    viewMode: 'table' | 'grid' = (typeof localStorage !== 'undefined' && localStorage.getItem('batches-view-mode') as 'table' | 'grid') || 'table';
    gridPage = 0;
    gridRows = 12;

    // Dialog state
    displayActionDialog = false;
    selectedBatch: MedicineBatch | null = null;
    displayDetailsDialog = false;
    selectedBatchId: number | null = null;

    // Filters
    searchTerm = '';
    sellableFilter: boolean | null = null;
    expiryFilter: 'all' | 'expiring' | 'expired' = 'all';

    sellableOptions = [
        { label: 'الكل', value: null },
        { label: 'قابل للبيع', value: true },
        { label: 'غير قابل للبيع', value: false }
    ];

    expiryOptions = [
        { label: 'الكل', value: 'all' },
        { label: 'تنتهي قريباً (30 يوم)', value: 'expiring' },
        { label: 'منتهية الصلاحية', value: 'expired' }
    ];

    constructor(
        private inventoryService: InventoryService,
        private messageService: MessageService,
        private router: Router
    ) { }

    ngOnInit() {
        this.loadBatches();
    }

    setViewMode(mode: 'table' | 'grid') {
        this.viewMode = mode;
        if (typeof localStorage !== 'undefined') {
            localStorage.setItem('batches-view-mode', mode);
        }
    }

    get pagedGridBatches(): MedicineBatch[] {
        const start = this.gridPage * this.gridRows;
        return this.filteredBatches.slice(start, start + this.gridRows);
    }

    onGridPageChange(event: any) {
        this.gridPage = event.page;
        this.gridRows = event.rows;
    }

    loadBatches() {
        this.loading = true;
        this.inventoryService.getAllBatches().subscribe({
            next: (data: any) => {
                this.batches = data;
                this.applyFilters();
                this.loading = false;
            },
            error: (err: any) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة الدفعات' });
                this.loading = false;
            }
        });
    }

    applyFilters() {
        this.filteredBatches = this.batches.filter(batch => {
            const matchesSearch = !this.searchTerm ||
                batch.medicineName?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                batch.companyBatchNumber?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                batch.batchBarcode?.toLowerCase().includes(this.searchTerm.toLowerCase());

            const matchesSellable = this.sellableFilter === null || batch.isSellable === this.sellableFilter;

            const matchesExpiry = this.expiryFilter === 'all' ||
                (this.expiryFilter === 'expiring' && batch.daysUntilExpiry! > 0 && batch.daysUntilExpiry! <= 30) ||
                (this.expiryFilter === 'expired' && batch.daysUntilExpiry! <= 0);

            return matchesSearch && matchesSellable && matchesExpiry;
        });
        this.gridPage = 0; // reset grid page on filter change
    }

    getExpirySeverity(days: number | undefined | null): 'success' | 'warning' | 'danger' {
        if (days === undefined || days === null || days <= 0) return 'danger';
        if (days <= 30) return 'warning';
        return 'success';
    }

    getExpiryLabel(days: number | undefined | null): string {
        if (days === undefined || days === null || days <= 0) return 'منتهية الصلاحية';
        if (days <= 30) return `تنتهي خلال ${days} يوم`;
        return `صالحة (${days} يوم)`;
    }

    getExpiryClass(days: number | undefined | null): string {
        if (days === undefined || days === null || days <= 0) return 'expired';
        if (days <= 30) return 'warning';
        return 'good';
    }

    viewBatch(id: number) {
        this.selectedBatchId = id;
        this.displayDetailsDialog = true;
    }

    showAction(batch: MedicineBatch) {
        this.selectedBatch = batch;
        this.displayActionDialog = true;
    }

    toggleStatus(batch: MedicineBatch) {
        const isActive = batch.status === 'Active';
        const newStatus = isActive ? 'Quarantine' : 'Active';
        const originalStatus = batch.status;
        const originalIsSellable = batch.isSellable;

        // Optimistic update
        batch.status = newStatus;

        this.inventoryService.updateBatchStatus(batch.id, newStatus).subscribe({
            next: (response: any) => {
                const updatedBatch = response?.data || response;
                if (updatedBatch && updatedBatch.status) {
                    batch.status = updatedBatch.status;
                    batch.isSellable = updatedBatch.isSellable;
                }
                this.messageService.add({
                    severity: 'success',
                    summary: 'نجاح',
                    detail: `تم تغيير حالة الدفعة إلى (${batch.status === 'Active' ? 'نشط' : 'موقوف'}) بنجاح`
                });
            },
            error: (err: any) => {
                batch.status = originalStatus;
                batch.isSellable = originalIsSellable;
                const errorMsg = err?.error?.message || 'فشل في تحديث حالة الدفعة';
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: errorMsg });
            }
        });
    }

    onActionSuccess() {
        this.displayActionDialog = false;
        this.loadBatches();
    }
}
