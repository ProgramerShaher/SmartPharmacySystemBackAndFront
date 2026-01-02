import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SalesReturnService } from '../../services/sales-return.service';
import { SalesReturn, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';

@Component({
    selector: 'app-sales-return-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        CardModule,
        ToolbarModule
    ],
    templateUrl: './sales-return-list.component.html',
    styleUrls: ['./sales-return-list.component.scss'],
    providers: [ConfirmationService]
})
export class SalesReturnListComponent implements OnInit {
    returns: SalesReturn[] = [];
    loading = true;

    constructor(
        private salesReturnService: SalesReturnService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadReturns();
    }

    loadReturns() {
        this.loading = true;
        this.salesReturnService.getAll().subscribe({
            next: (data) => {
                this.returns = data;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل المردودات' });
                this.loading = false;
            }
        });
    }

    navigateToCreate() {
        this.router.navigate(['/sales/returns/create']);
    }

    viewDetails(id: number) {
        // reuse create component in read-only mode or creating key?
        // or a details component if I had one. 
        // For now: /returns/:id
        this.router.navigate(['/sales/returns', id]);
    }

    DocumentStatus = DocumentStatus;

    getStatusSeverity(status: any): any {
        if (!status && status !== 0) return 'info';
        // Handle enum numbers
        if (typeof status === 'number') {
            if (status === 2) return 'success';  // Approved
            if (status === 1) return 'warning';  // Draft
            if (status === 3) return 'danger';   // Cancelled
        }
        // Handle strings
        const statusStr = status.toString().toUpperCase();
        switch (statusStr) {
            case 'APPROVED':
            case '2': return 'success';
            case 'DRAFT':
            case '1': return 'warning';
            case 'CANCELLED':
            case '3': return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: any): string {
        if (!status && status !== 0) return 'غير معروف';
        // Handle enum numbers
        if (typeof status === 'number') {
            if (status === 2) return 'معتمد';
            if (status === 1) return 'مسودة';
            if (status === 3) return 'ملغى';
        }
        // Handle strings
        const statusStr = status.toString().toUpperCase();
        switch (statusStr) {
            case 'APPROVED':
            case '2': return 'معتمد';
            case 'DRAFT':
            case '1': return 'مسودة';
            case 'CANCELLED':
            case '3': return 'ملغى';
            default: return 'غير معروف';
        }
    }

    approveReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد هذا المرتجع؟ سيتم إضافة الكميات للمخزون.',
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-check-circle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-success',
            accept: () => {
                this.salesReturnService.approve(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الاعتماد' })
                });
            }
        });
    }

    cancelReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء هذا المرتجع؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.salesReturnService.cancel(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الإلغاء' })
                });
            }
        });
    }

    deleteReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف هذه المسودة؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'تراجع',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.salesReturnService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' })
                });
            }
        });
    }
}
