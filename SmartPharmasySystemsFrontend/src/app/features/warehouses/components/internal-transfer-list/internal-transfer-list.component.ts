import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';

import { StockTransferService } from '../../services/stock-transfer.service';
import { WarehouseService } from '../../services/warehouse.service';
import { AuthService } from '../../../auth/services/auth.service';
import { WarehouseDto } from '../../../../core/models';
import { StockTransferDto } from '../../services/stock-transfer.service';

@Component({
    selector: 'app-internal-transfer-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, RouterModule,
        TableModule, ButtonModule, DropdownModule, TagModule,
        TooltipModule, DialogModule, ToastModule, ConfirmDialogModule,
        CardModule
    ],
    templateUrl: './internal-transfer-list.component.html',
    styleUrls: ['./internal-transfer-list.component.scss'],
    providers: [ConfirmationService]
})
export class InternalTransferListComponent implements OnInit {
    transfers: StockTransferDto[] = [];
    warehouses: WarehouseDto[] = [];
    loading = false;
    processingAction = false;

    // Detail Dialog
    displayDetailDialog = false;
    selectedTransfer?: StockTransferDto;

    constructor(
        private transferService: StockTransferService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit() {
        this.loadWarehouses();
        this.loadTransfers();
    }

    get currentBranchId(): number {
        const user = this.authService.currentUserValue;
        return user?.branchId ? Number(user.branchId) : 0;
    }

    get currentUserId(): number {
        const user = this.authService.currentUserValue;
        return user?.userId ? Number(user.userId) : 0;
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => this.warehouses = res);
    }

    loadTransfers() {
        this.loading = true;
        // transferType: 4 = Internal
        this.transferService.getAll({ transferType: 4 }).subscribe({
            next: (res) => {
                // Filter to show only transfers where source or destination belongs to the current branch
                // (Though since it's internal, both source and destination are in the same branch)
                this.transfers = res.filter(t =>
                    this.warehouses.some(w => (w.id === t.sourceWarehouseId || w.id === t.destinationWarehouseId) && w.branchId === this.currentBranchId)
                );
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات التحويلات الداخلية' });
                this.loading = false;
            }
        });
    }

    getStatusSeverity(status: string | number): 'warning' | 'info' | 'success' | 'danger' | 'secondary' {
        const s = status?.toString();
        switch (s) {
            case '1': case 'Requested': return 'warning';
            case '2': case 'Approved': return 'info';
            case '3': case 'Dispatched': return 'warning';
            case '5': case 'Received': return 'success';
            case '6': case 'Cancelled': return 'danger';
            default: return 'secondary';
        }
    }

    getStatusLabel(status: string | number): string {
        const s = status?.toString();
        switch (s) {
            case '1': case 'Requested': return 'طلب معلق';
            case '2': case 'Approved': return 'تم الاعتماد';
            case '3': case 'Dispatched': return 'قيد الشحن';
            case '5': case 'Received': return 'مستلمة بالكامل';
            case '6': case 'Cancelled': return 'ملغاة';
            default: return 'غير معروف';
        }
    }

    countByStatus(status: string | number): number {
        return this.transfers.filter(t => t.status?.toString() === status.toString()).length;
    }

    viewDetails(transfer: StockTransferDto) {
        this.transferService.getById(transfer.id).subscribe(res => {
            this.selectedTransfer = res;
            this.displayDetailDialog = true;
        });
    }

    approveTransfer(transfer: StockTransferDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من اعتماد طلب التحويل المخزني الداخلي رقم ${transfer.transferCode}؟`,
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-check-circle',
            acceptLabel: 'اعتماد',
            rejectLabel: 'إلغاء',
            accept: () => {
                if (this.processingAction) return;
                this.processingAction = true;
                this.transferService.approve(transfer.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد الطلب بنجاح' });
                        this.loadTransfers();
                        this.processingAction = false;
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل اعتماد الطلب' });
                        this.processingAction = false;
                    }
                });
            }
        });
    }

    deleteTransfer(transfer: StockTransferDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف طلب التحويل المخزني رقم ${transfer.transferCode}؟ لا يمكن التراجع عن هذا الإجراء.`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            acceptButtonStyleClass: 'p-button-danger',
            rejectLabel: 'إلغاء',
            accept: () => {
                if (this.processingAction) return;
                this.processingAction = true;
                this.transferService.delete(transfer.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الطلب' });
                        this.loadTransfers();
                        this.processingAction = false;
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحذف' });
                        this.processingAction = false;
                    }
                });
            }
        });
    }
}
