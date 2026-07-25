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

    // Filters
    sourceWarehouseId?: number;
    destinationWarehouseId?: number;
    selectedStatus?: number;

    statusOptions = [
        { label: 'الكل', value: undefined },
        { label: 'مطلوب (Requested)', value: 1 },
        { label: 'معتمد (Approved)', value: 2 },
        { label: 'مشحون (Dispatched)', value: 3 },
        { label: 'مستلم (Received)', value: 4 },
        { label: 'ملغي (Cancelled)', value: 5 }
    ];

    // Detail Dialog
    displayDetailDialog = false;
    selectedTransfer?: StockTransferDto;

    // Receipt Dialog
    displayReceiveDialog = false;
    receiveItems: { itemId: number; medicineName: string; batchNumber: string; quantity: number; receivedQuantity: number; rejectionReason?: string; }[] = [];

    // Current logged in user (simulated, usually from AuthService)
    currentUserId = 1;

    countTransfersByStatus(status: number): number {
        return this.transfers.filter(t => t.status === status).length;
    }

    constructor(
        private transferService: StockTransferService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private router: Router
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

    getStatusSeverity(status: number): 'warning' | 'info' | 'success' | 'danger' | 'secondary' {
        switch (status) {
            case 1: return 'warning'; // Requested
            case 2: return 'info';    // Approved
            case 3: return 'warning'; // Dispatched (In transit)
            case 4: return 'success'; // Received
            case 5: return 'danger';  // Cancelled
            default: return 'secondary';
        }
    }

    getStatusLabel(status: number): string {
        switch (status) {
            case 1: return 'طلب معلق';
            case 2: return 'تم الاعتماد';
            case 3: return 'قيد الشحن';
            case 4: return 'مستلمة بالكامل';
            case 5: return 'ملغاة';
            default: return 'غير معروف';
        }
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
                this.transferService.approve(transfer.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد طلب التحويل' });
                        this.loadTransfers();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل اعتماد الطلب' });
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
                this.transferService.dispatch(transfer.id, this.currentUserId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم شحن الكميات بنجاح' });
                        this.loadTransfers();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل شحن الكميات' });
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
            quantity: item.quantity,
            receivedQuantity: item.quantity, // default to fully received
            rejectionReason: ''
        }));
        this.displayReceiveDialog = true;
    }

    confirmReceive() {
        if (!this.selectedTransfer) return;

        const payload = {
            items: this.receiveItems.map(item => ({
                itemId: item.itemId,
                receivedQuantity: item.receivedQuantity,
                rejectionReason: item.rejectionReason || undefined
            }))
        };

        this.transferService.receive(this.selectedTransfer.id, payload, this.currentUserId).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تأكيد استلام الشحنة وإضافتها للمخزن المستلم' });
                this.displayReceiveDialog = false;
                this.loadTransfers();
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل استلام الشحنة' });
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
