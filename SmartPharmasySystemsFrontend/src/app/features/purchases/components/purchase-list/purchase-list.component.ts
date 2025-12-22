import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { PurchaseInvoice } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToolbarModule } from 'primeng/toolbar';

@Component({
    selector: 'app-purchase-invoice-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        ToolbarModule
    ],
    template: `
        <div class="card p-0 shadow-3 border-round-xl overflow-hidden" dir="rtl">
            <p-toolbar styleClass="bg-indigo-600 border-none p-4 text-white">
                <div class="p-toolbar-group-start">
                    <div class="flex align-items-center gap-3">
                        <i class="pi pi-briefcase text-4xl"></i>
                        <div>
                            <h2 class="m-0 text-2xl font-bold">فواتير المشتريات</h2>
                            <small class="opacity-80">إدارة توريدات الأدوية من الموردين</small>
                        </div>
                    </div>
                </div>
                <div class="p-toolbar-group-end">
                    <p-button label="تسجيل فاتورة توريد" icon="pi pi-plus" severity="success" 
                        class="shadow-2" (onClick)="navigateToCreate()"></p-button>
                </div>
            </p-toolbar>

            <div class="p-4 bg-gray-50">
                <p-table #dt [value]="invoices" [rows]="10" [paginator]="true" [loading]="loading"
                    [globalFilterFields]="['purchaseInvoiceNumber', 'supplierName', 'status']"
                    responsiveLayout="stack" [breakpoint]="'960px'"
                    styleClass="p-datatable-gridlines shadow-1 border-round overflow-hidden"
                    [showCurrentPageReport]="true"
                    currentPageReportTemplate="عرض {first} إلى {last} من أصل {totalRecords}">
                    
                    <ng-template pTemplate="caption">
                        <div class="flex flex-column md:flex-row justify-content-between align-items-center gap-3">
                            <span class="p-input-icon-left w-full md:w-25rem">
                                <i class="pi pi-search"></i>
                                <input pInputText type="text" (input)="dt.filterGlobal($any($event.target).value, 'contains')"
                                    placeholder="بحث (رقم الفاتورة، المورد، الحالة)..." class="w-full" />
                            </span>
                            <p-button icon="pi pi-refresh" severity="secondary" outlined (onClick)="loadInvoices()"></p-button>
                        </div>
                    </ng-template>

                    <ng-template pTemplate="header">
                        <tr>
                            <th pSortableColumn="purchaseInvoiceNumber" class="text-right">رقم الفاتورة <p-sortIcon field="purchaseInvoiceNumber"></p-sortIcon></th>
                            <th pSortableColumn="purchaseDate" class="text-right">تاريخ التوريد <p-sortIcon field="purchaseDate"></p-sortIcon></th>
                            <th pSortableColumn="supplierName" class="text-right">المورد <p-sortIcon field="supplierName"></p-sortIcon></th>
                            <th pSortableColumn="totalAmount" class="text-center">الإجمالي <p-sortIcon field="totalAmount"></p-sortIcon></th>
                            <th pSortableColumn="status" class="text-center">الحالة <p-sortIcon field="status"></p-sortIcon></th>
                            <th class="text-center">الإجراءات</th>
                        </tr>
                    </ng-template>

                    <ng-template pTemplate="body" let-invoice>
                        <tr>
                            <td>
                                <span class="p-column-title font-bold">رقم الفاتورة</span>
                                <span class="font-bold text-indigo-700">{{invoice.purchaseInvoiceNumber || '#' + invoice.id}}</span>
                            </td>
                            <td>
                                <span class="p-column-title font-bold">تاريخ التوريد</span>
                                {{invoice.purchaseDate | date:'dd/MM/yyyy'}}
                            </td>
                            <td>
                                <span class="p-column-title font-bold">المورد</span>
                                <span class="text-700 font-semibold">{{ invoice.supplierName && invoice.supplierName !== 'string' ? invoice.supplierName : 'مورد غير محدد' }}</span>
                            </td>
                            <td class="text-center">
                                <span class="p-column-title font-bold">الإجمالي</span>
                                <span class="font-bold text-indigo-700 text-lg">
                                    {{ (invoice.totalAmount > 0 ? invoice.totalAmount : (invoice.total || 0)) | number:'1.2-2' }}
                                    <small class="text-xs font-normal text-600 mr-1">ر.ي</small>
                                </span>
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
                                    <p-button *ngIf="invoice.status === 'Draft' || invoice.status === 'DRAFT'" icon="pi pi-check" severity="success" rounded text 
                                        pTooltip="اعتماد التوريد" (onClick)="approveInvoice(invoice.id)"></p-button>
                                    <p-button *ngIf="invoice.status === 'Approved' || invoice.status === 'APPROVED'" icon="pi pi-times" severity="danger" rounded text 
                                        pTooltip="إلغاء الفاتورة" (onClick)="cancelInvoice(invoice.id)"></p-button>
                                    <p-button *ngIf="invoice.status === 'Draft' || invoice.status === 'DRAFT'" icon="pi pi-trash" severity="danger" rounded text 
                                        pTooltip="حذف المسودة" (onClick)="deleteDraft(invoice.id)"></p-button>
                                </div>
                            </td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>
        <p-confirmDialog [style]="{width: '450px'}"></p-confirmDialog>
    `,
    providers: [ConfirmationService]
})
export class PurchaseInvoiceListComponent implements OnInit {
    invoices: PurchaseInvoice[] = [];
    loading = true;

    constructor(
        private purchaseService: PurchaseInvoiceService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadInvoices();
    }

    loadInvoices() {
        this.loading = true;
        this.purchaseService.getAll().subscribe({
            next: (data) => {
                this.invoices = data;
                this.loading = false;
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل فواتير المشتريات' });
                this.loading = false;
            }
        });
    }

    navigateToCreate() {
        this.router.navigate(['/purchase-invoices/create']);
    }

    viewDetails(id: number) {
        this.router.navigate(['/purchase-invoices', id]);
    }

    approveInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد التوريد؟ سيتم إضافة الأصناف للمخزون.',
            header: 'تأكيد التوريد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، توريد',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.purchaseService.approve(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد التوريد بنجاح' });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    cancelInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء فاوترة التوريد؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، إلغاء',
            rejectLabel: 'تراجع',
            accept: () => {
                this.purchaseService.cancel(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء الفاتورة' });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    deleteDraft(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف هذه المسودة نهائياً؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.purchaseService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المسودة بنجاح' });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    handleError(err: any) {
        this.messageService.add({ severity: 'error', summary: 'فشل العملية', detail: err.error?.message || 'فشل الاتصال بالخادم' });
    }

    getStatusSeverity(status: any) {
        if (status === undefined || status === null) return 'info';
        const s = status.toString().toUpperCase();
        switch (s) {
            case 'APPROVED':
            case '1':
            case 'TRUE': return 'success';
            case 'DRAFT':
            case '0': return 'warning';
            case 'CANCELLED':
            case '2': return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: any) {
        if (status === undefined || status === null) return 'غير محدد';
        const s = status.toString().toUpperCase();
        switch (s) {
            case 'APPROVED':
            case '1':
            case 'TRUE': return 'تم الاعتماد والتوريد';
            case 'DRAFT':
            case '0': return 'مسودة (قيد التحرير)';
            case 'CANCELLED':
            case '2': return 'ملغاة';
            default: return status;
        }
    }
}
