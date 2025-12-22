import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { SupplierService } from '../../../partners/services/supplier.service';
import { PurchaseInvoice, Supplier, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { CalendarModule } from 'primeng/calendar';
import { InvoiceItemDialogComponent } from '../../../../shared/components/invoice-item-dialog/invoice-item-dialog.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
    selector: 'app-purchase-invoice-create',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        DropdownModule,
        ConfirmDialogModule,
        CardModule,
        ToolbarModule,
        TagModule,
        DividerModule,
        CalendarModule,
        InvoiceItemDialogComponent,
        ConfirmationDialogComponent
    ],
    template: `
        <div class="flex flex-column h-full" dir="rtl">
            <!-- ERP Header Toolbar -->
            <p-toolbar styleClass="erp-toolbar">
                <div class="p-toolbar-group-start">
                    <div class="flex align-items-center gap-4">
                        <p-button icon="pi pi-arrow-right" rounded text styleClass="text-white hover:bg-white-alpha-20" (onClick)="backToList()"></p-button>
                        <div class="flex flex-column">
                            <h2 class="m-0 text-3xl font-extrabold line-height-2">إدارة فواتير التوريد</h2>
                            <div class="flex align-items-center gap-2 mt-1">
                                <span class="status-badge" [ngClass]="getStatusClass()">{{getStatusLabel()}}</span>
                                <span class="text-white-alpha-70 text-sm">| &nbsp; نظام المحاسبة والمخازن - اليمن</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="p-toolbar-group-end flex gap-3">
                    <p-button *ngIf="!isReadOnly" label="حفظ المسودة" icon="pi pi-save" severity="warning" [loading]="saving" (onClick)="saveDraft()" styleClass="p-button-raised"></p-button>
                    <p-button *ngIf="!isReadOnly" label="اعتماد التوريد" icon="pi pi-check" severity="success" [loading]="saving" (onClick)="approveInvoice()" styleClass="p-button-raised"></p-button>
                    <p-button *ngIf="isReadOnly" label="طباعة الفاتورة" icon="pi pi-print" severity="secondary" outlined styleClass="text-white border-white-alpha-30"></p-button>
                </div>
            </p-toolbar>

            <div class="flex-grow-1 overflow-auto form-container">
                <form [formGroup]="purchaseForm" class="grid">
                    <!-- Right Sidebar: Financial Summary -->
                    <div class="col-12 lg:col-4 lg:order-2">
                        <p-card header="الخلاصة المالية" styleClass="financial-summary h-full">
                            <div class="flex flex-column gap-2">
                                <div class="summary-item">
                                    <span class="label">إجمالي عدد الأصناف</span>
                                    <span class="value">{{details.length}} أصناف</span>
                                </div>
                                <div class="summary-item">
                                    <span class="label">إجمالي الوحدات</span>
                                    <span class="value">{{calculateTotalQuantity()}} وحدة</span>
                                </div>
                                <div class="summary-item">
                                    <span class="label">المبلغ الصافي</span>
                                    <span class="value">{{calculateSubtotal() | number:'1.2-2'}} ريال يمني</span>
                                </div>
                                <div class="summary-item">
                                    <span class="label">الرسوم والضرائب</span>
                                    <span class="value">0.00 ريال يمني</span>
                                </div>
                                <div class="summary-item total">
                                    <span class="label">الإجمالي النهائي</span>
                                    <span class="value">{{calculateTotal() | number:'1.2-2'}} ريال يمني</span>
                                </div>
                            </div>
                            
                            <div class="mt-5" *ngIf="!isReadOnly">
                                <p-button label="اعتماد وترحيل المخزون" icon="pi pi-verified" 
                                    severity="success" [disabled]="details.length === 0" 
                                    styleClass="w-full py-3 text-xl font-bold" (onClick)="approveInvoice()"></p-button>
                                <p-divider></p-divider>
                                <p-button label="تصفير القائمة" icon="pi pi-refresh" severity="danger" text 
                                    styleClass="w-full" (onClick)="details.clear()"></p-button>
                            </div>
                        </p-card>
                    </div>

                    <!-- Main Content: Header Info & Details -->
                    <div class="col-12 lg:col-8 lg:order-1">
                        <!-- Invoice Info -->
                        <p-card header="بيانات الفاتورة" styleClass="mb-4">
                            <div class="grid p-fluid">
                                <div class="col-12 md:col-6">
                                    <label class="font-semibold block mb-2 text-primary">المورد المعتمد</label>
                                    <p-dropdown [options]="suppliers" formControlName="supplierId" 
                                        optionLabel="name" optionValue="id" placeholder="ابحث عن مورد..."
                                        [filter]="true" filterBy="name" [disabled]="isReadOnly"></p-dropdown>
                                </div>
                                <div class="col-12 md:col-6">
                                    <label class="font-semibold block mb-2">رقم فاتورة المورد (المرجع)</label>
                                    <input pInputText formControlName="supplierInvoiceNumber" 
                                        placeholder="أدخل الرقم المسجل على فاتورة المورد..." [readonly]="isReadOnly" />
                                </div>
                                <div class="col-12 md:col-6">
                                    <label class="font-semibold block mb-2">تاريخ التوريد</label>
                                    <p-calendar formControlName="purchaseDate" [showIcon]="true" 
                                        appendTo="body" [disabled]="isReadOnly" dateFormat="dd/mm/yy"></p-calendar>
                                </div>
                                <div class="col-12 md:col-6">
                                    <label class="font-semibold block mb-2">نظام السداد</label>
                                    <p-dropdown [options]="paymentMethods" formControlName="paymentMethod" 
                                        optionLabel="label" optionValue="value" [disabled]="isReadOnly"></p-dropdown>
                                </div>
                            </div>
                        </p-card>

                        <!-- Items Grid -->
                        <p-card header="الأصناف والبترول الموردة">
                            <div class="flex justify-content-between align-items-center mb-4">
                                <div class="flex align-items-center gap-2 text-secondary">
                                    <i class="pi pi-briefcase font-bold"></i>
                                    <span>قائمة البنود المراد ترحيلها للمخازن</span>
                                </div>
                                <p-button label="إضافة بند توريدي" icon="pi pi-plus" 
                                    severity="primary" [outlined]="true" size="small" 
                                    (onClick)="openItemDialog()" [disabled]="isReadOnly"></p-button>
                            </div>

                            <p-table [value]="details.value" responsiveLayout="scroll" 
                                styleClass="p-datatable-gridlines p-datatable-sm shadow-1 border-round-lg overflow-hidden">
                                <ng-template pTemplate="header">
                                    <tr>
                                        <th class="text-right">وصف الصنف / الدواء</th>
                                        <th class="text-center" style="width: 140px">رقم التشغيلة</th>
                                        <th class="text-center" style="width: 100px">الكمية</th>
                                        <th class="text-right" style="width: 150px">السعر (ريال يمني)</th>
                                        <th class="text-right" style="width: 150px">الإجمالي</th>
                                        <th class="text-center" style="width: 100px" *ngIf="!isReadOnly">إجراءات</th>
                                    </tr>
                                </ng-template>
                                <ng-template pTemplate="body" let-item let-i="rowIndex">
                                    <tr class="hover:surface-50 cursor-pointer" (click)="!isReadOnly && openItemDialog(item, i)">
                                        <td class="font-bold text-primary">{{item.medicineName}}</td>
                                        <td class="text-center font-mono py-2 bg-blue-50 text-blue-700 border-round">{{item.companyBatchNumber}}</td>
                                        <td class="text-center font-bold">{{item.quantity}}</td>
                                        <td class="text-right">{{item.purchasePrice | number:'1.2-2'}}</td>
                                        <td class="text-right font-bold text-indigo-700">{{item.total | number:'1.2-2'}}</td>
                                        <td class="text-center" *ngIf="!isReadOnly" (click)="$event.stopPropagation()">
                                            <div class="flex justify-content-center gap-1">
                                                <p-button icon="pi pi-pencil" rounded text severity="warning" (onClick)="openItemDialog(item, i)"></p-button>
                                                <p-button icon="pi pi-trash" rounded text severity="danger" (onClick)="removeItem(i)"></p-button>
                                            </div>
                                        </td>
                                    </tr>
                                </ng-template>
                                <ng-template pTemplate="emptymessage">
                                    <tr>
                                        <td colspan="6" class="text-center p-6 text-secondary bg-gray-50 border-round-bottom-lg">
                                            <div class="flex flex-column align-items-center gap-3">
                                                <i class="pi pi-inbox text-5xl opacity-20"></i>
                                                <span class="text-xl">شاغرة.. يرجى البدء بإدراج الأصناف</span>
                                                <p-button label="أضف أول صنف الآن" icon="pi pi-plus" text (onClick)="openItemDialog()" *ngIf="!isReadOnly"></p-button>
                                            </div>
                                        </td>
                                    </tr>
                                </ng-template>
                                <ng-template pTemplate="footer" *ngIf="details.length > 0">
                                    <tr>
                                        <td colspan="2" class="text-left font-bold bg-surface-section text-xl">الإجمالي للمستند</td>
                                        <td class="text-center bg-surface-section text-primary text-xl font-bold">{{calculateTotalQuantity()}}</td>
                                        <td class="bg-surface-section"></td>
                                        <td class="text-right font-extrabold text-primary bg-surface-section text-xl">{{calculateTotal() | number:'1.2-2'}}</td>
                                        <td *ngIf="!isReadOnly" class="bg-surface-section"></td>
                                    </tr>
                                </ng-template>
                            </p-table>
                        </p-card>
                    </div>
                </form>
            </div>
        </div>

        <app-invoice-item-dialog #itemDialog invoiceType="Purchase" (onSave)="onItemSaved($event)"></app-invoice-item-dialog>
        
        <app-confirmation-dialog #confirmDialog
            [header]="'تأكيد المراجعة والترحيل'"
            [message]="'هل تم مراجعة الكميات والأسعار بعناية؟'"
            [subMessage]="'عملية الاعتماد والترحيل ستؤثر على الأرصدة المخزنية والحسابات الجارية للموردين في النظام اليمني.'"
            [severity]="'success'"
            confirmLabel="نعم، ترحيل الفاتورة"
            (accept)="onConfirmApprove()">
        </app-confirmation-dialog>
    `,
    providers: [ConfirmationService]
})
export class PurchaseInvoiceCreateComponent implements OnInit {
    @ViewChild('itemDialog') itemDialog!: InvoiceItemDialogComponent;
    @ViewChild('confirmDialog') confirmDialog!: ConfirmationDialogComponent;

    purchaseForm: FormGroup;
    saving = false;
    isEditMode = false;
    currentInvoiceId: number | null = null;
    editingIndex: number | null = null;
    suppliers: Supplier[] = [];
    status: DocumentStatus = DocumentStatus.DRAFT;

    paymentMethods = [
        { label: 'نقد (Cash)', value: 'Cash' },
        { label: 'آجل (On Credit)', value: 'Credit' }
    ];

    constructor(
        private fb: FormBuilder,
        private purchaseService: PurchaseInvoiceService,
        private supplierService: SupplierService,
        private route: ActivatedRoute,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.purchaseForm = this.fb.group({
            supplierId: [null, Validators.required],
            supplierInvoiceNumber: [''],
            purchaseDate: [new Date(), Validators.required],
            paymentMethod: ['Cash', Validators.required],
            notes: [''],
            purchaseInvoiceDetails: this.fb.array([])
        });
    }

    ngOnInit() {
        this.loadSuppliers();
        const id = this.route.snapshot.params['id'];
        if (id) {
            this.isEditMode = true;
            this.currentInvoiceId = +id;
            this.loadInvoice(id);
        }
    }

    loadSuppliers() {
        this.supplierService.GetAllSuppliers({ pageSize: 1000 } as any).subscribe((res: any) => this.suppliers = res.items);
    }

    get isReadOnly() {
        return this.status !== DocumentStatus.DRAFT;
    }

    get details() {
        return this.purchaseForm.get('purchaseInvoiceDetails') as FormArray;
    }

    loadInvoice(id: number) {
        this.purchaseService.getById(id).subscribe((data: PurchaseInvoice) => {
            if (data.status !== DocumentStatus.DRAFT) {
                this.router.navigate(['/purchase-invoices', id]);
                return;
            }
            this.purchaseForm.patchValue({
                ...data,
                purchaseDate: new Date(data.purchaseDate)
            });
            this.status = data.status || DocumentStatus.DRAFT;
            this.details.clear();
            data.purchaseInvoiceDetails?.forEach(d => this.addDetailToForm(d));
        });
    }

    addDetailToForm(detail: any) {
        const group = this.fb.group({
            id: [detail.id || 0],
            medicineId: [detail.medicineId, Validators.required],
            medicineName: [detail.medicineName],
            companyBatchNumber: [detail.companyBatchNumber, Validators.required],
            expiryDate: [detail.expiryDate ? new Date(detail.expiryDate) : null],
            quantity: [detail.quantity, [Validators.required, Validators.min(1)]],
            purchasePrice: [detail.purchasePrice, Validators.required],
            total: [detail.total || (detail.quantity * detail.purchasePrice)]
        });
        this.details.push(group);
    }

    openItemDialog(item?: any, index?: number) {
        this.editingIndex = index !== undefined ? index : null;
        const dialogData = item ? {
            medicineId: item.medicineId,
            medicineName: item.medicineName,
            companyBatchNumber: item.companyBatchNumber,
            expiryDate: item.expiryDate,
            quantity: item.quantity,
            price: item.purchasePrice
        } : null;
        this.itemDialog.show(dialogData);
    }

    onItemSaved(itemData: any) {
        const detail = {
            medicineId: itemData.medicineId,
            medicineName: itemData.medicineName,
            companyBatchNumber: itemData.companyBatchNumber,
            expiryDate: itemData.expiryDate,
            quantity: itemData.quantity,
            purchasePrice: itemData.price,
            total: itemData.quantity * itemData.price
        };

        if (this.editingIndex !== null) {
            this.details.at(this.editingIndex).patchValue(detail);
        } else {
            this.addDetailToForm(detail);
        }
    }

    removeItem(index: number) {
        this.details.removeAt(index);
    }

    calculateTotal() {
        const rawTotal = this.details.controls.reduce((acc, ctrl) => acc + (ctrl.get('total')?.value || 0), 0);
        return Number(rawTotal.toFixed(2));
    }

    calculateTotalQuantity() {
        return this.details.controls.reduce((acc, ctrl) => acc + (ctrl.get('quantity')?.value || 0), 0);
    }

    calculateSubtotal() {
        return this.calculateTotal(); // Assuming no tax/discount logic yet
    }

    saveDraft() {
        if (this.purchaseForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال الحقول المطلوبة' });
            return;
        }

        if (this.details.length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن حفظ فاتورة بدون أصناف' });
            return;
        }

        this.saving = true;
        const formValue = this.purchaseForm.getRawValue();
        const headerPayload = {
            supplierId: formValue.supplierId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber,
            purchaseDate: formValue.purchaseDate,
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes,
            createdBy: 1
        };

        const obs = this.isEditMode && this.currentInvoiceId
            ? this.purchaseService.update(this.currentInvoiceId, headerPayload)
            : this.purchaseService.create(headerPayload);

        obs.subscribe({
            next: (res) => {
                const details = formValue.purchaseInvoiceDetails || [];
                const detailsPayload = details.map((d: any) => ({
                    ...d,
                    purchaseInvoiceId: res.id
                }));

                const { from, concatMap, toArray } = require('rxjs');
                from(detailsPayload).pipe(
                    concatMap((detail: any) => {
                        if (detail.id > 0) {
                            return this.purchaseService.updateDetail(detail.id, detail);
                        } else {
                            return this.purchaseService.createDetail(detail);
                        }
                    }),
                    toArray()
                ).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الفاتورة وتفاصيلها بنجاح' });
                        this.router.navigate(['/purchase-invoices']);
                    },
                    error: (err: any) => {
                        this.logInvoiceError(err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'تم حفظ الفاتورة لكن فشل حفظ بعض التفاصيل' });
                        this.saving = false;
                    }
                });
            },
            error: (err) => {
                this.logInvoiceError(err);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحفظ' });
                this.saving = false;
            }
        });
    }

    approveInvoice() {
        if (this.details.length === 0) return;
        this.confirmDialog.show();
    }

    onConfirmApprove() {
        this.saving = true;
        const formValue = this.purchaseForm.getRawValue();
        const headerPayload = {
            supplierId: formValue.supplierId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber,
            purchaseDate: formValue.purchaseDate,
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes,
            createdBy: 1
        };

        const saveObs = this.isEditMode && this.currentInvoiceId
            ? this.purchaseService.update(this.currentInvoiceId, headerPayload)
            : this.purchaseService.create(headerPayload);

        saveObs.subscribe({
            next: (res) => {
                const details = formValue.purchaseInvoiceDetails || [];

                const completeWorkflow = () => {
                    this.purchaseService.approve(res.id).subscribe({
                        next: () => {
                            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم التوريد والترحيل للمخازن بنجاح' });
                            this.router.navigate(['/purchase-invoices', res.id]);
                        },
                        error: (err) => {
                            this.saving = false;
                            this.logInvoiceError(err);
                            this.messageService.add({ severity: 'error', summary: 'خطأ في الاعتماد', detail: err.error?.message });
                        }
                    });
                };

                if (details.length === 0) {
                    completeWorkflow();
                    return;
                }

                const detailsPayload = details.map((d: any) => ({
                    ...d,
                    purchaseInvoiceId: res.id
                }));

                const { from, concatMap, toArray } = require('rxjs');
                from(detailsPayload).pipe(
                    concatMap((detail: any) => {
                        if (detail.id > 0) {
                            return (this.purchaseService as any).updateDetail(detail.id, detail);
                        } else {
                            return this.purchaseService.createDetail(detail);
                        }
                    }),
                    toArray()
                ).subscribe({
                    next: () => completeWorkflow(),
                    error: (err: any) => {
                        this.saving = false;
                        this.logInvoiceError(err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حفظ التفاصيل قبل الاعتماد' });
                    }
                });
            },
            error: (err) => {
                this.saving = false;
                this.logInvoiceError(err);
                this.messageService.add({ severity: 'error', summary: 'خطأ في الحفظ', detail: err.error?.message });
            }
        });
    }

    getPurchaseInvoiceError(detail: any): string {
        if (!detail) return 'خطأ غير معروف في البيانات';
        if (detail.quantity <= 0) return 'الكمية يجب أن تكون أكبر من صفر';
        if (!detail.medicineId) return 'يجب اختيار الصنف أولاً';
        if (!detail.purchasePrice || detail.purchasePrice <= 0) return 'سعر الوحدة غير صالح';
        return 'بيانات البند غير مكتملة أو غير صالحة';
    }

    logInvoiceError(error: any): void {
        console.error('--- ERP ERROR LOG ---');
        console.error('Timestamp:', new Date().toISOString());
        console.error('Error Code:', error.status);
        console.error('Message:', error.message);
        console.error('Details:', error.error);
        console.error('---------------------');
    }

    backToList() {
        this.router.navigate(['/purchase-invoices']);
    }

    getStatusLabel() {
        if (this.status === undefined || this.status === null) return 'مسودة';
        const s = this.status.toString().toUpperCase();
        switch (s) {
            case 'APPROVED':
            case '1': return 'تم التوريد';
            case 'DRAFT':
            case '0': return 'مسودة';
            case 'CANCELLED':
            case '2': return 'ملغاة';
            default: return this.status;
        }
    }

    getStatusClass() {
        if (this.status === undefined || this.status === null) return 'draft';
        const s = this.status.toString().toUpperCase();
        switch (s) {
            case 'APPROVED':
            case '1': return 'approved';
            case 'DRAFT':
            case '0': return 'draft';
            case 'CANCELLED':
            case '2': return 'cancelled';
            default: return 'draft';
        }
    }
}
