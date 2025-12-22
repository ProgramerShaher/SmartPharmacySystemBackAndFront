import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SalesInvoiceService } from '../../services/sales-invoice.service';
import { SaleInvoice } from '../../../../core/models';
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
    selector: 'app-sales-invoice-list',
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
    template: `
        <div class="card p-0 shadow-3 border-round-xl overflow-hidden" dir="rtl">
            <p-toolbar styleClass="bg-primary border-none p-4 text-white">
                <div class="p-toolbar-group-start">
                    <div class="flex align-items-center gap-3">
                        <i class="pi pi-file-export text-4xl"></i>
                        <div>
                            <h2 class="m-0 text-2xl font-bold">فواتير المبيعات</h2>
                            <small class="opacity-80">إدارة ومراجعة مبيعات الصيدلية</small>
                        </div>
                    </div>
                </div>
                <div class="p-toolbar-group-end">
                    <p-button label="إنشاء فاتورة جديدة" icon="pi pi-plus" severity="success" 
                        class="shadow-2" (onClick)="navigateToCreate()"></p-button>
                </div>
            </p-toolbar>

            <div class="p-4 bg-gray-50">
                <p-table #dt [value]="invoices" [rows]="10" [paginator]="true" [loading]="loading"
                    [globalFilterFields]="['saleInvoiceNumber', 'customerName', 'status']"
                    responsiveLayout="stack" [breakpoint]="'960px'"
                    styleClass="p-datatable-gridlines shadow-1 border-round overflow-hidden"
                    [showCurrentPageReport]="true"
                    currentPageReportTemplate="عرض {first} إلى {last} من أصل {totalRecords}">
                    
                    <ng-template pTemplate="caption">
                        <div class="flex flex-column md:flex-row justify-content-between align-items-center gap-3">
                            <span class="p-input-icon-left w-full md:w-25rem">
                                <i class="pi pi-search"></i>
                                <input pInputText type="text" (input)="dt.filterGlobal($any($event.target).value, 'contains')"
                                    placeholder="بحث شامل (رقم، عميل، حالة)..." class="w-full" />
                            </span>
                            <div class="flex gap-2">
                                <p-button icon="pi pi-refresh" severity="secondary" outlined (onClick)="loadInvoices()"></p-button>
                            </div>
                        </div>
                    </ng-template>

                    <ng-template pTemplate="header">
                        <tr>
                            <th pSortableColumn="saleInvoiceNumber" class="text-right">رقم الفاتورة <p-sortIcon field="saleInvoiceNumber"></p-sortIcon></th>
                            <th pSortableColumn="saleInvoiceDate" class="text-right">التاريخ <p-sortIcon field="saleInvoiceDate"></p-sortIcon></th>
                            <th pSortableColumn="customerName" class="text-right">العميل <p-sortIcon field="customerName"></p-sortIcon></th>
                            <th pSortableColumn="totalAmount" class="text-center">الإجمالي <p-sortIcon field="totalAmount"></p-sortIcon></th>
                            <th pSortableColumn="status" class="text-center">الحالة <p-sortIcon field="status"></p-sortIcon></th>
                            <th class="text-center">الإجراءات</th>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="body" let-invoice>
                        <tr>
                            <td>
                                <span class="p-column-title font-bold">رقم الفاتورة</span>
                                <span class="font-bold text-primary">{{invoice.saleInvoiceNumber || '#' + invoice.id}}</span>
                            </td>
                            <td>
                                <span class="p-column-title font-bold">التاريخ</span>
                                {{invoice.saleInvoiceDate | date:'dd/MM/yyyy HH:mm'}}
                            </td>
                            <td>
                                <span class="p-column-title font-bold">العميل</span>
                                {{invoice.customerName || 'عميل نقدي'}}
                            </td>
                            <td class="text-center font-bold">
                                <span class="p-column-title font-bold">الإجمالي</span>
                                {{invoice.totalAmount | number:'1.2-2'}} ر.ي
                            </td>
                            <td class="text-center">
                                <span class="p-column-title font-bold">الحالة</span>
                                <p-tag [severity]="getStatusSeverity(invoice.status)" [value]="getStatusLabel(invoice.status)"
                                    styleClass="px-3"></p-tag>
                            </td>
                            <td class="text-center">
                                <div class="flex justify-content-center gap-2">
                                    <p-button icon="pi pi-eye" severity="info" rounded text 
                                        pTooltip="عرض التفاصيل" (onClick)="viewDetails(invoice.id)"></p-button>
                                    <p-button *ngIf="invoice.status === 'DRAFT'" icon="pi pi-pencil" severity="warning" rounded text 
                                        pTooltip="تعديل" (onClick)="editInvoice(invoice.id)"></p-button>
                                    <p-button *ngIf="invoice.status === 'DRAFT'" icon="pi pi-check" severity="success" rounded text 
                                        pTooltip="اعتماد" (onClick)="approveInvoice(invoice.id)"></p-button>
                                    <p-button *ngIf="invoice.status === 'APPROVED'" icon="pi pi-times" severity="danger" rounded text 
                                        pTooltip="إلغاء" (onClick)="cancelInvoice(invoice.id)"></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="emptymessage">
                        <tr>
                            <td colspan="6" class="text-center p-5 text-lg text-secondary">لا توجد فواتير مطابقة للبحث</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>
        <p-confirmDialog [style]="{width: '450px'}"></p-confirmDialog>
    `,
    providers: [ConfirmationService]
})
export class SalesInvoiceListComponent implements OnInit {
    invoices: SaleInvoice[] = [];
    loading = true;

    constructor(
        private salesService: SalesInvoiceService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadInvoices();
    }

    loadInvoices() {
        this.loading = true;
        this.salesService.getAll().subscribe({
            next: (data) => {
                this.invoices = data;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الفواتير' });
                this.loading = false;
            }
        });
    }

    navigateToCreate() {
        this.router.navigate(['/sales-invoices/create']);
    }

    viewDetails(id: number) {
        this.router.navigate(['/sales-invoices', id]);
    }

    editInvoice(id: number) {
        this.router.navigate(['/sales-invoices/edit', id]); // Or use create component with ID parameter
    }

    approveInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد هذه الفاتورة؟ سيتم خصم الكميات من المخزون.',
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.salesService.approve(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد الفاتورة' });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    cancelInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء هذه الفاتورة؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            accept: () => {
                this.salesService.cancel(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء الفاتورة' });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    handleError(err: any) {
        this.messageService.add({ severity: 'error', summary: 'فشل العملية', detail: err.error?.message || 'وقع خطأ ما' });
    }

    getStatusSeverity(status: string) {
        if (!status) return 'info';
        switch (status.toUpperCase()) {
            case 'APPROVED': return 'success';
            case 'DRAFT': return 'warning';
            case 'CANCELLED': return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: string) {
        if (!status) return 'غير معروف';
        switch (status.toUpperCase()) {
            case 'APPROVED': return 'معتمدة';
            case 'DRAFT': return 'مسودة';
            case 'CANCELLED': return 'ملغاة';
            default: return status;
        }
    }
}
