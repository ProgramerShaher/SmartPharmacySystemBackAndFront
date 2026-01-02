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
    templateUrl: './purchase-create.component.html',
    styleUrls: ['./purchase-create.component.scss'],
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
    status: DocumentStatus = DocumentStatus.Draft;

    paymentMethods = [
        { label: 'نقد (Cash)', value: 1 },
        { label: 'آجل (On Credit)', value: 2 }
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
            paymentMethod: [1, Validators.required],
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
        this.supplierService.getAll({ pageSize: 1000 }).subscribe((res: any) => this.suppliers = res.items);
    }

    get isReadOnly() {
        return this.status !== DocumentStatus.Draft;
    }

    get details() {
        return this.purchaseForm.get('purchaseInvoiceDetails') as FormArray;
    }

    loadInvoice(id: number) {
        this.purchaseService.getById(id).subscribe((data: PurchaseInvoice) => {
            if (data.status !== DocumentStatus.Draft) {
                this.router.navigate(['/purchases', id]);
                return;
            }
            this.purchaseForm.patchValue({
                ...data,
                purchaseDate: new Date(data.purchaseDate)
            });
            this.status = data.status || DocumentStatus.Draft;
            this.details.clear();
            data.items?.forEach((d: any) => this.addDetailToForm(d));
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
            bonusQuantity: [detail.bonusQuantity || 0],
            purchasePrice: [detail.purchasePrice, Validators.required],
            salePrice: [detail.salePrice, Validators.required],
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
            bonusQuantity: item.bonusQuantity,
            price: item.purchasePrice,
            salePrice: item.salePrice
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
            bonusQuantity: itemData.bonusQuantity,
            purchasePrice: itemData.price,
            salePrice: itemData.salePrice,
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
        return this.details.controls.reduce((acc, ctrl) => {
            const qty = ctrl.get('quantity')?.value || 0;
            const bonus = ctrl.get('bonusQuantity')?.value || 0;
            return acc + qty + bonus;
        }, 0);
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

        // Prepare items array
        const items = (formValue.purchaseInvoiceDetails || []).map((d: any) => ({
            medicineId: d.medicineId,
            companyBatchNumber: d.companyBatchNumber,
            expiryDate: d.expiryDate,
            quantity: d.quantity,
            bonusQuantity: d.bonusQuantity,
            purchasePrice: d.purchasePrice,
            salePrice: d.salePrice,
            batchBarcode: d.batchBarcode // Optional but good to have if used
        }));

        const payload = {
            supplierId: formValue.supplierId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber,
            purchaseDate: formValue.purchaseDate,
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes,
            items: items
        };

        const obs = this.isEditMode && this.currentInvoiceId
            ? this.purchaseService.update(this.currentInvoiceId, { ...payload, id: this.currentInvoiceId })
            : this.purchaseService.create(payload);

        obs.subscribe({
            next: (res) => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الفاتورة وتفاصيلها بنجاح' });
                this.router.navigate(['/purchases']);
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

        // Prepare items array
        const items = (formValue.purchaseInvoiceDetails || []).map((d: any) => ({
            medicineId: d.medicineId,
            companyBatchNumber: d.companyBatchNumber,
            expiryDate: d.expiryDate,
            quantity: d.quantity,
            bonusQuantity: d.bonusQuantity,
            purchasePrice: d.purchasePrice,
            salePrice: d.salePrice
        }));

        const payload = {
            supplierId: formValue.supplierId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber,
            purchaseDate: formValue.purchaseDate,
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes,
            items: items
        };

        const saveObs = this.isEditMode && this.currentInvoiceId
            ? this.purchaseService.update(this.currentInvoiceId, { ...payload, id: this.currentInvoiceId })
            : this.purchaseService.create(payload);

        saveObs.subscribe({
            next: (res) => {
                this.purchaseService.approve(res.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم التوريد والترحيل للمخازن بنجاح' });
                        this.router.navigate(['/purchases', res.id]);
                    },
                    error: (err) => {
                        this.saving = false;
                        this.logInvoiceError(err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ في الاعتماد', detail: err.error?.message });
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
        this.router.navigate(['/purchases']);
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
