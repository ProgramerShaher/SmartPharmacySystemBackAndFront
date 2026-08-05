import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
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
        FormsModule,
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

    unitOptions: any[] = [];
    baseUnitName = 'حبة';
    selectedMedicine: Medicine | null = null;

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
            unitCost: [0],
            selectedUnit: ['base', Validators.required],
            storageLocation: ['']
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
            // Batch number is now optional as requested by the user, backend will auto-generate if empty
            batchNum?.clearValidators();
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
            
            // In edit mode, we must fetch the medicine to populate selectedMedicine and unitOptions
            if (item.medicineId) {
                this.inventoryService.getMedicineById(item.medicineId).subscribe({
                    next: (medicine) => {
                        this.selectedMedicine = medicine;
                        this.baseUnitName = medicine.baseUnitName || 'حبة';
                        this.buildUnitOptions(medicine);
                    }
                });
                
                if (this.invoiceType === 'Sales') {
                    this.loadBatches(item.medicineId);
                }
            }
        } else {
            this.isEdit = false;
            this.itemForm.reset({ quantity: 1, price: 0 });
            this.maxQuantity = 0;
            this.batches = [];
            this.selectedMedicine = null;
        }
        this.visible = true;
    }

    private buildUnitOptions(medicine: Medicine) {
        this.unitOptions = [
            {
                label: `${this.baseUnitName} (وحدة صغرى أساسية)`,
                value: 'base',
                conversionFactor: 1,
                purchasePrice: medicine.defaultPurchasePrice || 0,
                salePrice: medicine.defaultSalePrice || 0
            }
        ];

        if (medicine.medicineUnits && medicine.medicineUnits.length > 0) {
            medicine.medicineUnits.forEach(u => {
                this.unitOptions.push({
                    label: `${u.name} (تحتوي على ${u.conversionFactor} ${this.baseUnitName})`,
                    value: u.id,
                    conversionFactor: u.conversionFactor,
                    purchasePrice: u.defaultPurchasePrice,
                    salePrice: u.defaultSalePrice
                });
            });
        }
    }

    saveItem() {
        const itemData = this.itemForm.getRawValue();
        const payload = {
            medicineId: itemData.medicineId,
            medicineName: itemData.medicineName,
            batchId: itemData.batchId,
            companyBatchNumber: itemData.companyBatchNumber,
            expiryDate: itemData.expiryDate,
            quantity: itemData.quantity,
            purchaseUnitId: itemData.unitId,
            bonusQuantity: itemData.bonusQuantity,
            purchasePrice: itemData.price,
            salePrice: itemData.salePrice,
            total: itemData.quantity * itemData.price,
            storageLocation: itemData.storageLocation
        };
        this.onSave.emit(payload);
        this.onClose();
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
        this.selectedMedicine = null;
    }

    prefillMedicineById(id: number) {
        this.resetState();
        this.isEdit = false;
        this.itemForm.reset({ quantity: 1, price: 0 });
        this.maxQuantity = 0;
        this.batches = [];
        this.visible = true;

        this.inventoryService.getMedicineById(id).subscribe({
            next: (medicine) => {
                this.selectedMedicine = medicine;
                this.onMedicineSelect(medicine);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات الدواء' });
            }
        });
    }

    searchMedicines(event: any) {
        this.inventoryService.searchMedicines({ search: event.query }).subscribe(res => {
            this.filteredMedicines = res.items;
        });
    }

    onMedicineSelect(medicine: Medicine) {
        this.selectedMedicine = medicine;
        this.baseUnitName = medicine.baseUnitName || 'حبة';

        this.buildUnitOptions(medicine);

        const patchData: any = {
            medicineId: medicine.id,
            medicineName: medicine.name,
            selectedUnit: 'base'
        };

        if (!this.isEdit) {
            patchData.price = this.invoiceType === 'Sales'
                ? (medicine.defaultSalePrice || 0)
                : (medicine.defaultPurchasePrice || 0);
            patchData.salePrice = medicine.defaultSalePrice || 0;
        }

        this.itemForm.patchValue(patchData, { emitEvent: true });

        if (this.invoiceType === 'Sales') {
            this.loadBatches(medicine.id);
        }
    }

    onUnitSelect(unitVal: any) {
        const option = this.unitOptions.find(o => o.value === unitVal);
        if (option) {
            const patch: any = {
                price: this.invoiceType === 'Sales' ? option.salePrice : option.purchasePrice
            };
            if (this.invoiceType === 'Purchase') {
                patch.salePrice = option.salePrice;
            }
            this.itemForm.patchValue(patch);
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
        
        // Find selected unit conversion factor
        const selectedUnitVal = val.selectedUnit;
        const option = this.unitOptions.find(o => o.value === selectedUnitVal);
        const factor = option ? option.conversionFactor : 1;

        // If it's a packaging unit, append its name to the medicine name for user reference
        if (option && selectedUnitVal !== 'base') {
            const unitName = option.label.split(' ')[0]; // E.g., "علبة"
            val.medicineName = `${val.medicineName} (${unitName})`;
            val.unitId = option.value;
        } else {
            val.unitId = null;
        }

        // Calculate base units for validation only! Backend handles the actual conversion.
        const baseUnitsQty = (val.quantity || 0) * factor;

        if (this.invoiceType === 'Sales' && baseUnitsQty > this.maxQuantity && !this.isEdit) {
            this.messageService.add({ severity: 'error', summary: 'خطأ في الكمية', detail: `الكمية المتاحة هي ${this.maxQuantity} وحدة أساسية فقط` });
            return;
        }

        // Emit the payload
        this.onSave.emit(val);
        this.visible = false;
    }
}
