import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SalesService } from '../../services/sales.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { SaleInvoice } from '../../../../core/models';
import { DocumentStatus } from '../../../../core/models/stock-movement.enums';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { MedicineBatch } from '../../../../core/models';

@Component({
    selector: 'app-sale-invoice-details',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ButtonModule,
        TableModule,
        TagModule,
        ConfirmDialogModule,
        CardModule,
        DividerModule,
        TooltipModule,
        ProgressSpinnerModule
    ],
    templateUrl: './sale-invoice-details.component.html',
    providers: [ConfirmationService]
})
export class SaleInvoiceDetailsComponent implements OnInit {
    invoice: SaleInvoice | null = null;
    loading = true;

    // Action Dialog State
    actionDialogVisible = false;
    selectedBatch: MedicineBatch | null = null;

    DocumentStatus = DocumentStatus;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private salesService: SalesService,
        private inventoryService: InventoryService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        const id = this.route.snapshot.params['id'];
        if (id) {
            this.loadInvoice(id);
        }
    }

    loadInvoice(id: number) {
        this.loading = true;
        forkJoin({
            header: this.salesService.getById(id),
            details: this.salesService.getDetailsByInvoiceId(id)
        }).subscribe({
            next: (res) => {
                this.invoice = res.header;
                this.invoice.items = res.details;
                this.loading = false;
            },
            error: (e) => {
                console.error(e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل تفاصيل الفاتورة' });
                this.loading = false;
            }
        });
    }

    approveInvoice() {
        if (!this.invoice) return;
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد هذه الفاتورة؟ سيتم خصم الكميات من المخزون.',
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.salesService.approve(this.invoice!.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الاعتماد', detail: 'تم اعتماد الفاتورة بنجاح' });
                        this.loadInvoice(this.invoice!.id);
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في اعتماد الفاتورة' });
                    }
                });
            }
        });
    }

    cancelInvoice() {
        if (!this.invoice) return;
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء هذه الفاتورة؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            accept: () => {
                this.salesService.cancel(this.invoice!.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء الفاتورة بنجاح' });
                        this.loadInvoice(this.invoice!.id);
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في إلغاء الفاتورة' });
                    }
                });
            }
        });
    }

    backToList() {
        this.router.navigate(['/sales']);
    }

    getStatusSeverity(status: DocumentStatus): 'success' | 'warning' | 'danger' | 'info' {
        switch (status) {
            case DocumentStatus.Approved: return 'success';
            case DocumentStatus.Draft: return 'warning';
            case DocumentStatus.Cancelled: return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: DocumentStatus): string {
        switch (status) {
            case DocumentStatus.Approved: return 'معتمدة';
            case DocumentStatus.Draft: return 'مسودة';
            case DocumentStatus.Cancelled: return 'ملغاة';
            default: return 'غير معروف';
        }
    }

    openActionDialog(detail: any) {
        // We need to map detail to a MedicineBatch object or just pass relevant data
        this.selectedBatch = {
            id: detail.batchId,
            companyBatchNumber: detail.companyBatchNumber,
            remainingQuantity: detail.remainingQuantity || 0 // Assuming it's in the detail or fetched
        } as MedicineBatch;
        this.actionDialogVisible = true;
    }

    onActionComplete() {
        if (this.invoice) {
            this.loadInvoice(this.invoice.id);
        }
    }

    /**
     * Convert number to Arabic text
     */
    convertToArabicText(amount: number): string {
        if (!amount) return 'صفر ريال';

        const ones = ['', 'واحد', 'اثنان', 'ثلاثة', 'أربعة', 'خمسة', 'ستة', 'سبعة', 'ثمانية', 'تسعة'];
        const tens = ['', 'عشرة', 'عشرون', 'ثلاثون', 'أربعون', 'خمسون', 'ستون', 'سبعون', 'ثمانون', 'تسعون'];
        const hundreds = ['', 'مئة', 'مئتان', 'ثلاثمئة', 'أربعمئة', 'خمسمئة', 'ستمئة', 'سبعمئة', 'ثمانمئة', 'تسعمئة'];

        const intPart = Math.floor(amount);

        if (intPart < 10) {
            return ones[intPart] + ' ريال';
        } else if (intPart < 100) {
            const ten = Math.floor(intPart / 10);
            const one = intPart % 10;
            return (ones[one] ? ones[one] + ' و ' : '') + tens[ten] + ' ريال';
        } else if (intPart < 1000) {
            const hundred = Math.floor(intPart / 100);
            const remainder = intPart % 100;
            let text = hundreds[hundred];
            if (remainder > 0) {
                const ten = Math.floor(remainder / 10);
                const one = remainder % 10;
                text += ' و ' + (ones[one] ? ones[one] + ' و ' : '') + tens[ten];
            }
            return text + ' ريال';
        } else if (intPart < 10000) {
            const thousands = Math.floor(intPart / 1000);
            const remainder = intPart % 1000;
            let text = ones[thousands] + ' آلاف';
            if (remainder > 0) {
                text += ' و ' + this.convertToArabicText(remainder).replace(' ريال', '');
            }
            return text + ' ريال';
        }

        return intPart.toString() + ' ريال';
    }
}
