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
import { MessageService, Message } from 'primeng/api'; // Added Message
import { MessagesModule } from 'primeng/messages'; // Added MessagesModule
import { InventoryService } from '../../../features/inventory/services/inventory.service';
import { AlertService } from '../../../core/services/alert.service'; // Added AlertService
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
        InputTextModule,
        MessagesModule // Added
    ],
    templateUrl: './invoice-item-dialog.component.html',
    styleUrls: ['./invoice-item-dialog.component.scss']
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

    // UI States for Batch Analysis
    isLoadingBatches = false;
    hasExpiredBatches = false;
    noStockAvailable = false;
    systemDate = new Date(); // Use actual current date

    isSyncing = false;
    msgs: Message[] = []; 

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService,
        private alertService: AlertService
    ) {
        this.itemForm = this.fb.group({
            medicineId: [null, Validators.required],
            medicineName: [''],
            batchId: [null],
            companyBatchNumber: [''],
            expiryDate: [null],
            quantity: [1, [Validators.required, Validators.min(1)]],
            bonusQuantity: [0],
            price: [0, [Validators.required, Validators.min(0)]],
            salePrice: [0],
            unitCost: [0]
        });

        this.itemForm.valueChanges.subscribe(val => {
            const qty = Number(val.quantity) || 0;
            const price = Number(val.price) || 0;
            const total = qty * price;
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
        this.resetState();

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

    resetState() {
        this.hasExpiredBatches = false;
        this.noStockAvailable = false;
        this.msgs = [];
        this.batches = [];
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
        const patchData: any = {
            medicineId: medicine.id,
            medicineName: medicine.name
        };

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
        this.isLoadingBatches = true;
        this.resetState();

        // 1. Get ALL batches to match database state
        this.inventoryService.getBatchesByMedicineId(medicineId).subscribe({
            next: (res) => {
                console.log('📦 All Batches from DB:', res);

                // Show ALL batches, just sort them: Valid & Available first, then others
                this.batches = res.sort((a, b) => {
                    // Custom sort logic: 
                    // 1. Has Quantity?
                    // 2. Not Expired?
                    // 3. Newest?
                    const aValid = a.remainingQuantity > 0 && new Date(a.expiryDate) > this.systemDate;
                    const bValid = b.remainingQuantity > 0 && new Date(b.expiryDate) > this.systemDate;

                    if (aValid && !bValid) return -1;
                    if (!aValid && bValid) return 1;
                    return 0;
                });

                this.isLoadingBatches = false;

                if (this.batches.length === 0) {
                    this.diagnoseMissingBatches(medicineId);
                } else {
                    // Auto-select if only one VALID batch
                    const validBatches = this.batches.filter(b => b.remainingQuantity > 0 && new Date(b.expiryDate) > this.systemDate);
                    if (validBatches.length === 1) {
                        this.itemForm.patchValue({ batchId: validBatches[0].id });
                        this.onBatchSelect(validBatches[0].id);
                    }
                }

                if (this.isEdit) {
                    const currentBatchId = this.itemForm.get('batchId')?.value;
                    const batch = this.batches.find(b => b.id === currentBatchId);
                    if (batch) {
                        this.maxQuantity = batch.remainingQuantity;
                        this.itemForm.patchValue({ unitCost: batch.unitPurchasePrice });
                    }
                }
            },
            error: () => this.isLoadingBatches = false
        });
    }

    diagnoseMissingBatches(medicineId: number) {
        // Fetch ALL batches to find out why none are available
        this.inventoryService.getBatchesByMedicineId(medicineId).subscribe(allBatches => {
            if (!allBatches || allBatches.length === 0) {
                // CASE: No batches ever created
                this.msgs = [{ severity: 'info', summary: 'عذراً', detail: 'لم يتم توريد هذا الصنف من قبل (الرصيد الكلي صفر).' }];
                return;
            }

            // Check for Expired vs Stock Out
            const expiredCount = allBatches.filter(b => new Date(b.expiryDate) < this.systemDate).length;
            const validButNoStock = allBatches.filter(b => new Date(b.expiryDate) >= this.systemDate && b.remainingQuantity <= 0).length;

            if (expiredCount > 0) {
                this.hasExpiredBatches = true;
                this.msgs.push({
                    severity: 'error',
                    summary: 'تنبيه انتهاء الصلاحية',
                    detail: `يوجد ${expiredCount} تشغيلة منتهية الصلاحية (أقدم من 2026).`
                });
            }

            if (validButNoStock > 0) {
                this.noStockAvailable = true;
                this.msgs.push({
                    severity: 'warn',
                    summary: 'نفاد مخزون',
                    detail: 'الدواء متاح ولكن الكمية في المخزن 0. يرجى توريد كميات جديدة.'
                });
            }

            if (expiredCount === 0 && validButNoStock === 0) {
                // Might be "Deleted" or "Quarantine"
                this.msgs.push({ severity: 'warn', summary: 'غير متاح', detail: 'لا توجد تشغيلات صالحة للبيع حالياً.' });
            }
        });
    }

    forceSync() {
        const medicineId = this.itemForm.get('medicineId')?.value;
        if (!medicineId) return;

        this.isSyncing = true;
        this.alertService.syncMedicineAlerts(medicineId).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم التحديث', detail: 'تم تحديث البيانات مع السيرفر' });
                this.loadBatches(medicineId);
                this.isSyncing = false;
            },
            error: () => {
                this.isSyncing = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التحديث' });
            }
        });
    }

    onBatchSelect(batchId: number) {
        const batch = this.batches.find(b => b.id === batchId);
        if (batch) {
            this.maxQuantity = batch.remainingQuantity;
            this.itemForm.patchValue({
                price: batch.unitPurchasePrice * 1.2, // Default markup if not set
                unitCost: batch.unitPurchasePrice
            }); 

            if (new Date(batch.expiryDate) < this.systemDate) {
                this.messageService.add({ severity: 'error', summary: 'تنبيه', detail: 'هذه الدفعة منتهية الصلاحية ولا يمكن بيعها' });
                this.itemForm.get('batchId')?.setValue(null);
            }
        }
    }

    isBatchExpired(batch: MedicineBatch): boolean {
        return new Date(batch.expiryDate) < this.systemDate;
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
