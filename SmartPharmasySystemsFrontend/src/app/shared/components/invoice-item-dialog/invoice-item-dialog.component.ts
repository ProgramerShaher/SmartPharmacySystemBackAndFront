import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { InventoryService } from '../../../features/inventory/services/inventory.service';
import { Medicine, MedicineBatch } from '../../../core/models';

@Component({
    selector: 'app-invoice-item-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        DialogModule,
        AutoCompleteModule,
        DropdownModule,
        InputNumberModule,
        ButtonModule,
        CalendarModule,
        InputTextModule
    ],
    template: `
        <p-dialog [(visible)]="visible" [header]="isEdit ? 'تعديل بند توريد' : 'إضافة بند توريد جديد'"
            [modal]="true" [style]="{width: '500px'}" (onHide)="onClose()">
            <form [formGroup]="itemForm" class="p-fluid grid mt-2">
                <div class="col-12 mb-3">
                    <label class="font-bold block mb-2">اسم الصنف / الدواء</label>
                    <p-autoComplete [suggestions]="filteredMedicines" (completeMethod)="searchMedicines($event)"
                        field="name" (onSelect)="onMedicineSelect($event.value)" placeholder="ابحث عن الصنف في القاعدة..."
                        appendTo="body" [forceSelection]="true" [disabled]="isEdit"></p-autoComplete>
                </div>

                <div class="col-12 mb-3" *ngIf="invoiceType === 'Sales'">
                    <label class="font-bold block mb-2">الدفعة المخزنية (Batch)</label>
                    <p-dropdown [options]="batches" optionLabel="companyBatchNumber" placeholder="اختر الدفعة المتوفرة"
                        formControlName="batchId" optionValue="id" appendTo="body" (onChange)="onBatchSelect($event.value)">
                        <ng-template pTemplate="item" let-batch>
                            <div class="flex justify-content-between align-items-center">
                                <span>{{batch.companyBatchNumber}}</span>
                                <span class="text-sm" [class.text-danger]="batch.remainingQuantity === 0">
                                    {{batch.remainingQuantity}} وحدة متوفرة
                                </span>
                            </div>
                        </ng-template>
                    </p-dropdown>
                </div>

                <div class="col-12 mb-3" *ngIf="invoiceType === 'Purchase'">
                    <label class="font-bold block mb-2">رقم التشغيلة (Batch Number)</label>
                    <input pInputText formControlName="companyBatchNumber" placeholder="أدخل رقم التشغيلة المطبوع" />
                </div>

                <div class="col-12 mb-3" *ngIf="invoiceType === 'Purchase'">
                    <label class="font-bold block mb-2">تاريخ انتهاء الصلاحية</label>
                    <p-calendar formControlName="expiryDate" view="month" dateFormat="mm/yy" [showIcon]="true"
                        appendTo="body" placeholder="اختر شهر/سنة الانتهاء"></p-calendar>
                </div>

                <div class="col-6 mb-3">
                    <label class="font-bold block mb-2">الكمية الموردة</label>
                    <p-inputNumber formControlName="quantity" [min]="1" [showButtons]="true" buttonLayout="horizontal"
                        incrementButtonIcon="pi pi-plus" decrementButtonIcon="pi pi-minus"></p-inputNumber>
                </div>

                <div class="col-6 mb-3">
                    <label class="font-bold block mb-2">سعر شراء الوحدة</label>
                    <p-inputNumber formControlName="price" [min]="0" mode="decimal" [minFractionDigits]="2"
                        suffix=" ريال يمني"></p-inputNumber>
                </div>

                <div class="col-12 mt-2 bg-indigo-50 p-3 border-round border-1 border-indigo-100 flex justify-content-between align-items-center">
                    <span class="text-indigo-700 font-bold">إجمالي قيمة البند:</span>
                    <span class="text-2xl font-extrabold text-indigo-600">{{itemTotal | number:'1.2-2'}} ريال يمني</span>
                </div>
            </form>
            <ng-template pTemplate="footer">
                <p-button label="إلغاء الأمر" icon="pi pi-times" severity="secondary" outlined (onClick)="visible = false"></p-button>
                <p-button [label]="isEdit ? 'تعديل البند' : 'إضافة البند'" [icon]="isEdit ? 'pi pi-check' : 'pi pi-plus'"
                    severity="primary" [disabled]="itemForm.invalid" (onClick)="onSubmit()"></p-button>
            </ng-template>
        </p-dialog>
    `
})
export class InvoiceItemDialogComponent implements OnInit {
    @Input() invoiceType: 'Sales' | 'Purchase' = 'Sales';
    @Output() onSave = new EventEmitter<any>();

    visible = false;
    isEdit = false;
    itemForm: FormGroup;
    filteredMedicines: Medicine[] = [];
    batches: MedicineBatch[] = [];
    maxQuantity = 0;
    itemTotal = 0;

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) {
        this.itemForm = this.fb.group({
            medicineId: [null, Validators.required],
            medicineName: [''],
            batchId: [null], // For Sales
            companyBatchNumber: [''], // For Purchase
            expiryDate: [null], // For Purchase
            quantity: [1, [Validators.required, Validators.min(1)]],
            price: [0, [Validators.required, Validators.min(0)]],
            unitCost: [0]
        });

        // Dynamic Calculations with Precision
        this.itemForm.valueChanges.subscribe(val => {
            const qty = Number(val.quantity) || 0;
            const price = Number(val.price) || 0;
            const total = qty * price;
            // Use Number() and toFixed() to ensure absolute decimal precision
            this.itemTotal = Number(total.toFixed(2));
        });
    }

    ngOnInit() {
        this.updateValidators();
    }

    private updateValidators() {
        const batchNum = this.itemForm.get('companyBatchNumber');
        const expiryDate = this.itemForm.get('expiryDate');

        if (this.invoiceType === 'Purchase') {
            batchNum?.setValidators([Validators.required]);
            expiryDate?.setValidators([Validators.required]);
        } else {
            batchNum?.clearValidators();
            expiryDate?.clearValidators();
        }
        batchNum?.updateValueAndValidity();
        expiryDate?.updateValueAndValidity();
    }

    show(item?: any) {
        if (item) {
            this.isEdit = true;
            this.itemForm.patchValue(item);
            if (this.invoiceType === 'Sales' && item.medicineId) {
                this.loadBatches(item.medicineId);
            }
        } else {
            this.isEdit = false;
            this.itemForm.reset({ quantity: 1, price: 0 });
            this.maxQuantity = 0;
            this.batches = [];
        }
        this.visible = true;
    }

    onClose() {
        this.visible = false;
        this.isEdit = false;
    }

    searchMedicines(event: any) {
        this.inventoryService.searchMedicines({ search: event.query }).subscribe(res => {
            this.filteredMedicines = res.items;
        });
    }

    onMedicineSelect(medicine: Medicine) {
        // Only patch values that should change on selection
        const patchData: any = {
            medicineId: medicine.id,
            medicineName: medicine.name
        };

        // For new items (not editing), fetch the default price
        if (!this.isEdit) {
            patchData.price = this.invoiceType === 'Sales'
                ? (medicine.defaultSalePrice || 0)
                : (medicine.defaultPurchasePrice || 0);
        }

        this.itemForm.patchValue(patchData, { emitEvent: true });

        if (this.invoiceType === 'Sales') {
            this.loadBatches(medicine.id);
        }
    }

    loadBatches(medicineId: number) {
        this.inventoryService.getAvailableBatchesByMedicineId(medicineId).subscribe(res => {
            this.batches = res;
            if (this.isEdit) {
                const currentBatchId = this.itemForm.get('batchId')?.value;
                const batch = this.batches.find(b => b.id === currentBatchId);
                if (batch) {
                    this.maxQuantity = batch.remainingQuantity;
                    this.itemForm.patchValue({ unitCost: batch.unitPurchasePrice });
                }
            }
        });
    }

    onBatchSelect(batchId: number) {
        const batch = this.batches.find(b => b.id === batchId);
        if (batch) {
            this.maxQuantity = batch.remainingQuantity;
            this.itemForm.patchValue({
                price: batch.unitPurchasePrice * 1.2,
                unitCost: batch.unitPurchasePrice
            }); // Example Markup

            if (new Date(batch.expiryDate) < new Date()) {
                this.messageService.add({ severity: 'error', summary: 'تنبيه', detail: 'هذه الدفعة منتهية الصلاحية ولا يمكن بيعها' });
                this.itemForm.get('batchId')?.setValue(null);
            }
        }
    }

    onSubmit() {
        if (this.itemForm.invalid) return;

        const val = this.itemForm.getRawValue();
        if (this.invoiceType === 'Sales' && val.quantity > this.maxQuantity && !this.isEdit) {
            this.messageService.add({ severity: 'error', summary: 'خطأ في الكمية', detail: `الكمية المتاحة هي ${this.maxQuantity} فقط` });
            return;
        }

        this.onSave.emit(val);
        this.visible = false;
    }
}
