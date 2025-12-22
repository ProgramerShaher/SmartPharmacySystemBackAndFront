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
import { MessageService } from 'primeng/api';
import { InventoryService } from '../../services/inventory.service';
import { MedicineBatch, StockMovementType } from '../../../../core/models';
import { differenceInDays, isBefore } from 'date-fns';
import { DialogModule } from 'primeng/dialog';
import { BatchActionsDialogComponent } from '../batch-actions-dialog/batch-actions-dialog.component';

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
        DialogModule,
        BatchActionsDialogComponent
    ],
    templateUrl: './batch-list.component.html',
    styleUrls: ['./batch-list.component.scss']
})
export class BatchListComponent implements OnInit {
    batches: MedicineBatch[] = [];
    filteredBatches: MedicineBatch[] = [];
    loading = true;

    // Dialog state
    displayActionDialog = false;
    selectedBatch: MedicineBatch | null = null;

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

    loadBatches() {
        this.loading = true;
        this.inventoryService.getAllBatches().subscribe({
            next: (data) => {
                this.batches = data.map(batch => this.calculateBatchFields(batch));
                this.applyFilters();
                this.loading = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة الدفعات' });
                this.loading = false;
            }
        });
    }

    calculateBatchFields(batch: MedicineBatch): MedicineBatch {
        const today = new Date();
        const expiry = new Date(batch.expiryDate);

        batch.soldQuantity = batch.quantity - batch.remainingQuantity;
        batch.daysUntilExpiry = differenceInDays(expiry, today);
        batch.isSellable = isBefore(today, expiry) && batch.remainingQuantity > 0 && batch.status === 'Active';

        return batch;
    }

    applyFilters() {
        this.filteredBatches = this.batches.filter(batch => {
            const matchesSearch = !this.searchTerm ||
                batch.medicineName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                batch.companyBatchNumber.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                batch.batchBarcode.toLowerCase().includes(this.searchTerm.toLowerCase());

            const matchesSellable = this.sellableFilter === null || batch.isSellable === this.sellableFilter;

            const matchesExpiry = this.expiryFilter === 'all' ||
                (this.expiryFilter === 'expiring' && batch.daysUntilExpiry > 0 && batch.daysUntilExpiry <= 30) ||
                (this.expiryFilter === 'expired' && batch.daysUntilExpiry <= 0);

            return matchesSearch && matchesSellable && matchesExpiry;
        });
    }

    getExpirySeverity(days: number): 'success' | 'warning' | 'danger' {
        if (days <= 0) return 'danger';
        if (days <= 30) return 'warning';
        return 'success';
    }

    getExpiryLabel(days: number): string {
        if (days <= 0) return 'منتهية';
        if (days <= 30) return `تنتهي خلال ${days} يوم`;
        return `صالحة (${days} يوم)`;
    }

    viewBatch(id: number) {
        this.router.navigate(['/inventory/batches/details', id]);
    }

    showAction(batch: MedicineBatch) {
        this.selectedBatch = batch;
        this.displayActionDialog = true;
    }

    onActionSuccess() {
        this.displayActionDialog = false;
        this.loadBatches();
    }
}
