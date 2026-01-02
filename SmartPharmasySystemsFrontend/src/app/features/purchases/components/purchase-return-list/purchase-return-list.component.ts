import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseReturnService } from '../../services/purchase-return.service';
import { PurchaseReturn } from '../../../../core/models/purchase-return.interface';
import { DocumentStatus } from '../../../../core/models/stock-movement.enums';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
    selector: 'app-purchase-return-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        MenuModule,
        ConfirmDialogModule,
        ToastModule
    ],
    providers: [ConfirmationService],
    templateUrl: './purchase-return-list.component.html',
    styleUrls: ['./purchase-return-list.component.scss']
})
export class PurchaseReturnListComponent implements OnInit {
    returns: PurchaseReturn[] = [];
    loading = true;

    constructor(
        private purchaseReturnService: PurchaseReturnService,
        private router: Router,
        private confirmationService: ConfirmationService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadReturns();
    }

    loadReturns() {
        this.loading = true;
        this.purchaseReturnService.getAll().subscribe({
            next: (data) => {
                this.returns = data;
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل البيانات' });
            }
        });
    }

    createReturn() {
        this.router.navigate(['/purchases/returns/create']);
    }

    viewReturn(ret: PurchaseReturn) {
        this.router.navigate(['/purchases/returns', ret.id]);
    }

    editReturn(ret: PurchaseReturn) {
        this.router.navigate(['/purchases/returns/edit', ret.id]);
    }

    deleteReturn(ret: PurchaseReturn) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف هذا المرتجع؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.purchaseReturnService.delete(ret.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المرتجع بنجاح' });
                        this.loadReturns();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' });
                    }
                });
            }
        });
    }

    getStatusLabel(status: any): string {
        if (!status) return 'غير محدد';
        const statusNum = typeof status === 'number' ? status : parseInt(status.toString());

        switch (statusNum) {
            case 2: // Approved
                return 'معتمد';
            case 1: // Draft
                return 'مسودة';
            case 3: // Cancelled
                return 'ملغى';
            default:
                return 'غير محدد';
        }
    }

    getStatusSeverity(status: any): 'success' | 'warning' | 'danger' | 'info' {
        if (!status) return 'info';
        const statusNum = typeof status === 'number' ? status : parseInt(status.toString());

        switch (statusNum) {
            case 2: // Approved
                return 'success';
            case 1: // Draft
                return 'warning';
            case 3: // Cancelled
                return 'danger';
            default:
                return 'info';
        }
    }

    isDraft(status: any): boolean {
        const statusNum = typeof status === 'number' ? status : parseInt(status?.toString() || '0');
        return statusNum === 1; // Draft
    }

    isApproved(status: any): boolean {
        const statusNum = typeof status === 'number' ? status : parseInt(status?.toString() || '0');
        return statusNum === 2; // Approved
    }

    isCancelled(status: any): boolean {
        const statusNum = typeof status === 'number' ? status : parseInt(status?.toString() || '0');
        return statusNum === 3; // Cancelled
    }
}
