import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { DamagedGoodsService, DamagedGoodsRecordDto } from '../../services/damaged-goods.service';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto } from '../../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-damaged-goods-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        TableModule,
        ButtonModule,
        CardModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        DropdownModule,
        FormsModule
    ],
    templateUrl: './damaged-goods-list.component.html',
    styleUrls: ['./damaged-goods-list.component.scss'],
    providers: [ConfirmationService]
})
export class DamagedGoodsListComponent implements OnInit {
    records: DamagedGoodsRecordDto[] = [];
    warehouses: WarehouseDto[] = [];
    loading = false;

    // Filters
    selectedWarehouseId?: number;
    selectedStatus?: number;

    statusOptions = [
        { label: 'الكل', value: undefined },
        { label: 'معلق (Pending)', value: 1 },
        { label: 'معتمد (Approved)', value: 2 },
        { label: 'مرفوض (Rejected)', value: 3 }
    ];

    damageTypes = [
        { label: 'الكل', value: undefined },
        { label: 'منتهي الصلاحية', value: 1 },
        { label: 'تالف/مكسور', value: 2 },
        { label: 'سوء تخزين', value: 3 },
        { label: 'أخرى', value: 4 }
    ];

    currentUserId = 1; // Simulated, usually from auth context

    constructor(
        private damagedService: DamagedGoodsService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private router: Router
    ) {}

    ngOnInit() {
        this.loadWarehouses();
        this.loadRecords();
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => this.warehouses = res);
    }

    loadRecords() {
        this.loading = true;
        this.damagedService.getAll({
            warehouseId: this.selectedWarehouseId,
            status: this.selectedStatus
        }).subscribe({
            next: (res) => {
                this.records = res;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل سجلات التوالف' });
                this.loading = false;
            }
        });
    }

    getStatusSeverity(status: number): 'warning' | 'success' | 'danger' | 'secondary' {
        switch (status) {
            case 1: return 'warning'; // Pending
            case 2: return 'success'; // Approved (Posted)
            case 3: return 'danger';  // Rejected
            default: return 'secondary';
        }
    }

    getStatusLabel(status: number): string {
        switch (status) {
            case 1: return 'معلق للاعتماد';
            case 2: return 'تم الترحيل والإهلاك';
            case 3: return 'مرفوض';
            default: return 'غير معروف';
        }
    }

    getDamageTypeLabel(type: number): string {
        switch (type) {
            case 1: return 'منتهي الصلاحية';
            case 2: return 'تالف/مكسور';
            case 3: return 'سوء تخزين';
            case 4: return 'أخرى';
            default: return 'غير محدد';
        }
    }

    approveRecord(record: DamagedGoodsRecordDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من اعتماد إذن الإهلاك التابع للصنف ${record.medicineName} بقيمة إجمالية قدرها ${record.damageValue} ر.ي؟ سيتم خصم الكمية من المخازن نهائياً وتوليد قيد مالي بالخسارة.`,
            header: 'تأكيد الإهلاك والترحيل المحاسبي',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد القيد',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.damagedService.approve(record.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد إذن الإهلاك وترحيل القيد المحاسبي بنجاح' });
                        this.loadRecords();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ في الاعتماد', detail: err.error?.message || 'فشل الترحيل المحاسبي' });
                    }
                });
            }
        });
    }

    deleteRecord(record: DamagedGoodsRecordDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف هذا الطلب؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptLabel: 'حذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.damagedService.delete(record.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف السجل بنجاح' });
                        this.loadRecords();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحذف' });
                    }
                });
            }
        });
    }

    get pendingCount(): number {
        return this.records.filter(r => r.status === 1).length;
    }

    calculateTotalLoss(): number {
        return this.records
            .filter(r => r.status === 2)
            .reduce((sum, r) => sum + r.damageValue, 0);
    }

    navigateToCreate() {
        this.router.navigate(['/warehouses/damaged/create']);
    }
}
