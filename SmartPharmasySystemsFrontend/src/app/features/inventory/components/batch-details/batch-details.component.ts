import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { MedicineBatch, StockCardDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { BatchActionsDialogComponent } from '../batch-actions-dialog/batch-actions-dialog.component';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
    selector: 'app-batch-details',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        TagModule,
        TooltipModule,
        DialogModule,
        BatchActionsDialogComponent,
        ProgressSpinnerModule
    ],
    providers: [],
    templateUrl: './batch-details.component.html',
    styleUrl: './batch-details.component.scss'
})
export class BatchDetailsComponent implements OnInit {
    batch: MedicineBatch | null = null;
    stockCard: StockCardDto[] = [];
    loading = true;
    displayActionDialog = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        const id = this.route.snapshot.params['id'];
        if (id) {
            this.loadBatch(id);
        }
    }

    loadBatch(id: number) {
        this.loading = true;
        this.inventoryService.getBatchById(id).subscribe({
            next: (data) => {
                console.log('📦 Batch data from backend:', data);
                console.log('🔍 medicineId present?', data.medicineId);

                // Fix: If medicineId is missing, try to get it from medicine navigation property
                if (!data.medicineId && data.medicine?.id) {
                    console.log('⚠️ medicineId missing, using medicine.id');
                    data.medicineId = data.medicine.id;
                }

                // If still missing, we need to fetch batch list to find medicineId
                if (!data.medicineId) {
                    console.error('❌ medicineId is still missing! Backend needs to include it.');
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'تحذير',
                        detail: 'بعض البيانات ناقصة. قد لا تعمل التسوية اليدوية بشكل صحيح.'
                    });
                }

                this.batch = data;
                if (this.batch.medicineId) {
                    this.loadStockCard(this.batch.medicineId, id);
                } else {
                    this.loading = false;
                }
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل تفاصيل الدفعة' });
                this.loading = false;
            }
        });
    }

    loadStockCard(medicineId: number, batchId: number) {
        this.inventoryService.getStockCard(medicineId, batchId).subscribe({
            next: (data) => {
                this.stockCard = data;
                this.loading = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل سجل الحركات' });
                this.loading = false;
            }
        });
    }

    showActionDialog() {
        // Reload batch to ensure we have all data including medicineId
        if (this.batch?.id) {
            this.inventoryService.getBatchById(this.batch.id).subscribe({
                next: (refreshedBatch) => {
                    console.log('🔄 Refreshed batch for dialog:', refreshedBatch);
                    this.batch = refreshedBatch;
                    this.displayActionDialog = true;
                },
                error: (err) => {
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في تحميل بيانات الدفعة'
                    });
                }
            });
        } else {
            this.displayActionDialog = true;
        }
    }

    onActionSuccess() {
        this.displayActionDialog = false;
        if (this.batch) {
            this.loadBatch(this.batch.id); // Refresh data
        }
    }

    getMovementSeverity(type: string): 'success' | 'info' | 'warning' | 'danger' {
        switch (type) {
            case 'IN': return 'success';
            case 'OUT': return 'danger';
            case 'RETURN': return 'info';
            case 'DAMAGE': return 'danger';
            case 'ADJUSTMENT': return 'warning';
            default: return 'info';
        }
    }

    getMovementTypeLabel(type: any): string {
        const typeStr = String(type).toLowerCase();
        if (typeStr.includes('purchase')) return 'شراء / توريد';
        if (typeStr.includes('sale')) return 'بيع مباشر';
        if (typeStr.includes('return')) return 'مرتجع';
        if (typeStr.includes('adjustment')) return 'تعديل جردي';
        if (typeStr.includes('damage')) return 'تالف';
        if (typeStr.includes('opening')) return 'رصيد أول';
        if (typeStr.includes('stockin') || typeStr === 'in') return 'إضافة مخزنية';
        if (typeStr.includes('stockout') || typeStr === 'out') return 'صرف مخزني';
        return type || 'غير معروف';
    }

    getExpiryLabel(days: number): string {
        if (days <= 0) return 'منتهية الصلاحية';
        if (days <= 30) return `تنتهي خلال ${days} يوم`;
        return `صالحة (${days} يوم)`;
    }

    getExpirySeverity(days: number): 'success' | 'warning' | 'danger' {
        if (days <= 0) return 'danger';
        if (days <= 30) return 'warning';
        return 'success';
    }

    goBack() {
        this.router.navigate(['/inventory/batches']);
    }
}
