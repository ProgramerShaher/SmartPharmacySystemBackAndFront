import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SalesService } from '../../services/sales.service';
import { SaleInvoice } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-sale-invoice-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ToolbarModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        ConfirmDialogModule,
        TooltipModule
    ],
    templateUrl: './sale-invoice-list.component.html',
    providers: [ConfirmationService]
})
export class SaleInvoiceListComponent implements OnInit {
    sales: SaleInvoice[] = [];
    loading = true;

    constructor(
        private salesService: SalesService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadSales();
    }

    loadSales() {
        this.loading = true;
        this.salesService.getAll().subscribe({
            next: (data) => {
                this.sales = data;
                this.loading = false;
            },
            error: (e) => {
                console.error(e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل فواتير المبيعات' });
                this.loading = false;
            }
        });
    }

    createSale() {
        this.router.navigate(['/sales/invoices/create']);
    }

    viewSale(sale: SaleInvoice) {
        this.router.navigate(['/sales/invoices', sale.id]);
    }

    approveSale(sale: SaleInvoice) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من اعتماد الفاتورة رقم ${sale.invoiceNumber || '#' + sale.id}؟ سيتم تحديث المخزون فوراً.`,
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.salesService.approve(sale.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الاعتماد', detail: 'تم اعتماد الفاتورة وتحديث المخزون بنجاح' });
                        this.loadSales();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في اعتماد الفاتورة' });
                    }
                });
            }
        });
    }

    cancelSale(sale: SaleInvoice) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من إلغاء الفاتورة رقم ${sale.invoiceNumber || '#' + sale.id}؟`,
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            accept: () => {
                this.salesService.cancel(sale.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء الفاتورة بنجاح' });
                        this.loadSales();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في إلغاء الفاتورة' });
                    }
                });
            }
        });
    }

    getStatusSeverity(status: string) {
        switch (status) {
            case 'Approved': return 'success';
            case 'Draft': return 'warning';
            case 'Cancelled': return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: string) {
        switch (status) {
            case 'Approved': return 'معتمدة';
            case 'Draft': return 'مسودة';
            case 'Cancelled': return 'ملغاة';
            default: return status;
        }
    }
}
