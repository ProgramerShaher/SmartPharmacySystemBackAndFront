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
    selectedStatus?: string | number;

    statusOptions = [
        { label: 'الكل', value: undefined },
        { label: 'معلق (Pending)', value: 'PendingApproval' },
        { label: 'معتمد (Approved)', value: 'Approved' },
        { label: 'مرفوض (Rejected)', value: 'Rejected' }
    ];

    damageTypes = [
        { label: 'الكل', value: undefined },
        { label: 'منتهي الصلاحية', value: 'Expired' },
        { label: 'تالف/مكسور', value: 'PhysicalDamage' },
        { label: 'عيب تصنيع', value: 'ManufacturingDefect' }
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

    getStatusSeverity(status: string | number): 'warning' | 'success' | 'danger' | 'secondary' {
        const s = status?.toString();
        switch (s) {
            case '1':
            case 'PendingApproval': return 'warning';
            case '2':
            case 'Approved': return 'success';
            case '3':
            case 'Rejected': return 'danger';
            default: return 'secondary';
        }
    }

    getStatusLabel(status: string | number): string {
        const s = status?.toString();
        switch (s) {
            case '1':
            case 'PendingApproval': return 'معلق للاعتماد';
            case '2':
            case 'Approved': return 'تم الترحيل والإهلاك';
            case '3':
            case 'Rejected': return 'مرفوض';
            default: return 'غير معروف';
        }
    }

    getDamageTypeLabel(type: string | number): string {
        const t = type?.toString();
        switch (t) {
            case '1':
            case 'Expired': return 'منتهي الصلاحية';
            case '2':
            case 'PhysicalDamage': return 'تالف/مكسور';
            case '3':
            case 'ManufacturingDefect': return 'عيب تصنيع';
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
        return this.records.filter(r => r.status?.toString() === 'PendingApproval' || r.status?.toString() === '1').length;
    }

    calculateTotalLoss(): number {
        return this.records
            .filter(r => r.status?.toString() === 'Approved' || r.status?.toString() === '2')
            .reduce((sum, r) => sum + r.damageValue, 0);
    }

    navigateToCreate() {
        this.router.navigate(['/warehouses/damaged/create']);
    }
}
