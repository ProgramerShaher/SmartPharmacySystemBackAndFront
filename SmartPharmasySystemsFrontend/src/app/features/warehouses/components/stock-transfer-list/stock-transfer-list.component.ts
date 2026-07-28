import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { StockTransferService, StockTransferDto } from '../../services/stock-transfer.service';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto } from '../../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
    selector: 'app-stock-transfer-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        TableModule,
        ButtonModule,
        CardModule,
        DropdownModule,
        TagModule,
        TooltipModule,
        DialogModule,
        ConfirmDialogModule,
        InputTextModule,
        InputNumberModule,
        FormsModule
    ],
    templateUrl: './stock-transfer-list.component.html',
    styleUrls: ['./stock-transfer-list.component.scss'],
    providers: [ConfirmationService]
})
export class StockTransferListComponent implements OnInit {
    transfers: StockTransferDto[] = [];
    warehouses: WarehouseDto[] = [];
    loading = false;
    processingAction = false;

    // Filters
    sourceWarehouseId?: number;
    destinationWarehouseId?: number;
    selectedStatus?: number;

    statusOptions = [
        { label: 'الكل', value: undefined },
        { label: 'مطلوب (Requested)', value: 'Requested' },
        { label: 'معتمد (Approved)', value: 'Approved' },
        { label: 'مشحون (Dispatched)', value: 'Dispatched' },
        { label: 'مستلم (Received)', value: 'Received' },
        { label: 'ملغي (Cancelled)', value: 'Cancelled' }
    ];

    // Detail Dialog
    displayDetailDialog = false;
    selectedTransfer?: StockTransferDto;

    // Receipt Dialog
    displayReceiveDialog = false;
    receiveItems: { itemId: number; medicineName: string; batchNumber: string; quantityRequested: number; quantityReceived: number; rejectionReason?: string; }[] = [];

    // Current logged in user — resolved from AuthService
    get currentUserId(): number {
        return this.authService.currentUserValue?.userId ?? 1;
    }

    countTransfersByStatus(status: string | number): number {
        return this.transfers.filter(t => t.status?.toString() === status.toString()).length;
    }

    constructor(
        private transferService: StockTransferService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private router: Router,
        private authService: AuthService
    ) { }

    ngOnInit() {
        this.loadWarehouses();
        this.loadTransfers();
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => this.warehouses = res);
    }

    loadTransfers() {
        this.loading = true;
        this.transferService.getAll({
            sourceWarehouseId: this.sourceWarehouseId,
            destinationWarehouseId: this.destinationWarehouseId,
            status: this.selectedStatus
        }).subscribe({
            next: (res) => {
                this.transfers = res;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات التحويلات' });
                this.loading = false;
            }
        });
    }

    getStatusSeverity(status: string | number): 'warning' | 'info' | 'success' | 'danger' | 'secondary' {
        const s = status?.toString();
        switch (s) {
            case '1':
            case 'Requested': return 'warning';
            case '2':
            case 'Approved': return 'info';
            case '3':
            case 'Dispatched': return 'warning';
            case '5':
            case 'Received': return 'success';
            case '6':
            case 'Cancelled': return 'danger';
            default: return 'secondary';
        }
    }

    getStatusLabel(status: string | number): string {
        const s = status?.toString();
        switch (s) {
            case '1':
            case 'Requested': return 'طلب معلق';
            case '2':
            case 'Approved': return 'تم الاعتماد';
            case '3':
            case 'Dispatched': return 'قيد الشحن';
            case '5':
            case 'Received': return 'مستلمة بالكامل';
            case '6':
            case 'Cancelled': return 'ملغاة';
            default: return 'غير معروف';
        }
    }

    isExpiringSoon(date?: string | Date): boolean {
        if (!date) return false;
        const expiryDate = new Date(date);
        const today = new Date();
        const diffTime = Math.abs(expiryDate.getTime() - today.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays <= 0; // Already expired or expires today
    }

    isNearExpiry(date?: string | Date): boolean {
        if (!date) return false;
        const expiryDate = new Date(date);
        const today = new Date();
        const diffTime = Math.abs(expiryDate.getTime() - today.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays > 0 && diffDays <= 90; // Expires within 90 days
    }

    viewDetails(transfer: StockTransferDto) {
        this.transferService.getById(transfer.id).subscribe(res => {
            this.selectedTransfer = res;
            this.displayDetailDialog = true;
        });
    }

    approveTransfer(transfer: StockTransferDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من اعتماد طلب التحويل المخزني رقم ${transfer.transferCode}؟`,
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            accept: () => {
                if (this.processingAction) return;
                this.processingAction = true;
                this.transferService.approve(transfer.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد طلب التحويل' });
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

    dispatchTransfer(transfer: StockTransferDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من شحن الكميات الخاصة بالطلب رقم ${transfer.transferCode}؟ سيتم خصم الكميات من المخزن الأصلي.`,
            header: 'تأكيد الشحن والخصم',
            icon: 'pi pi-truck',
            acceptLabel: 'نعم، اشحن',
            rejectLabel: 'إلغاء',
            accept: () => {
                if (this.processingAction) return;
                this.processingAction = true;
                this.transferService.dispatch(transfer.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم شحن الكميات بنجاح' });
                        this.loadTransfers();
                        this.processingAction = false;
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل شحن الكميات' });
                        this.processingAction = false;
                    }
                });
            }
        });
    }

    openReceiveDialog(transfer: StockTransferDto) {
        this.selectedTransfer = transfer;
        this.receiveItems = transfer.items.map(item => ({
            itemId: item.id,
            medicineName: item.medicineName,
            batchNumber: item.batchNumber,
            quantityRequested: item.quantityRequested,
            quantityReceived: item.quantityRequested, // default to fully received
            rejectionReason: ''
        }));
        this.displayReceiveDialog = true;
    }

    confirmReceive() {
        if (!this.selectedTransfer || this.processingAction) return;

        this.processingAction = true;
        const payload = {
            items: this.receiveItems.map(item => ({
                stockTransferItemId: item.itemId,
                quantityReceived: item.quantityReceived
            }))
        };

        this.transferService.receive(this.selectedTransfer.id, payload, this.currentUserId).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تأكيد استلام الشحنة وإضافتها للمخزن المستلم' });
                this.displayReceiveDialog = false;
                this.loadTransfers();
                this.processingAction = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل استلام الشحنة' });
                this.processingAction = false;
            }
        });
    }

    deleteTransfer(transfer: StockTransferDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف طلب التحويل رقم ${transfer.transferCode}؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptLabel: 'حذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.transferService.delete(transfer.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف طلب التحويل بنجاح' });
                        this.loadTransfers();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحذف' });
                    }
                });
            }
        });
    }

    navigateToCreate() {
        this.router.navigate(['/warehouses/transfers/create']);
    }
}
