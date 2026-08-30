import { Component, OnInit, ViewChild, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { SupplierService } from '../../../partners/services/supplier.service';
import { PurchaseInvoice, Supplier, DocumentStatus, WarehouseDto, WarehouseType, Medicine } from '../../../../core/models';
import { WarehouseService } from '../../../warehouses/services/warehouse.service';
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
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { BarcodeService } from '../../../../core/services/barcode.service';
import { BarcodeSimulatorComponent } from '../../../../shared/components/barcode-simulator/barcode-simulator.component';
import { TransactionType } from '../../../../core/models/barcode.interface';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputNumberModule } from 'primeng/inputnumber';

@Component({
    selector: 'app-purchase-invoice-create',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
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
        DialogModule,
        TooltipModule,
        ConfirmationDialogComponent,
        BarcodeSimulatorComponent,
        AutoCompleteModule,
        InputNumberModule
    ],
    templateUrl: './purchase-create.component.html',
    styleUrls: ['./purchase-create.component.scss'],
    providers: [ConfirmationService]
})

export class PurchaseInvoiceCreateComponent implements OnInit {
    @ViewChild('confirmDialog') confirmDialog!: ConfirmationDialogComponent;
    @ViewChild('medicineAutoComplete') medicineAutoComplete: any;
    @ViewChild('qtyInputEl') qtyInputEl: any;
    @ViewChild('paymentDropdown') paymentDropdown: any;

    purchaseForm: FormGroup;
    inlineItemForm: FormGroup;
    saving = false;
    isEditMode = false;
    currentInvoiceId: number | null = null;
    editingIndex: number | null = null;
    suppliers: Supplier[] = [];
    warehouses: WarehouseDto[] = [];
    status: DocumentStatus = DocumentStatus.Draft;

    // Modal & Dialog Controls
    itemModalVisible = false;
    shortcutsHelpVisible = false;
    searchQueryText: any = '';
    minExpiryDate: Date = new Date();

    // Inline Entry Properties
    filteredMedicines: Medicine[] = [];
    unitOptions: any[] = [];
    selectedMedicine: Medicine | null = null;
    baseUnitName = 'حبة';

    paymentMethods = [
        { label: 'نقد (Cash)', value: 1 },
        { label: 'آجل (On Credit)', value: 2 }
    ];

    constructor(
        private fb: FormBuilder,
        private purchaseService: PurchaseInvoiceService,
        private supplierService: SupplierService,
        private warehouseService: WarehouseService,
        private route: ActivatedRoute,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private barcodeService: BarcodeService,
        private inventoryService: InventoryService
    ) {
        this.purchaseForm = this.fb.group({
            supplierId: [null],
            warehouseId: [null, Validators.required],
            supplierInvoiceNumber: ['', null],
            purchaseDate: [new Date(), Validators.required],
            paymentMethod: [1, Validators.required],
            notes: [''],
            storageLocation: [''],
            purchaseInvoiceDetails: this.fb.array([])
        });

        this.inlineItemForm = this.fb.group({
            medicineId: [null, Validators.required],
            medicineName: [''],
            companyBatchNumber: [''],
            expiryDate: [this.getDefaultExpiryDate(), Validators.required],
            quantity: [1, [Validators.required, Validators.min(1)]],
            bonusQuantity: [0],
            price: [0, [Validators.required, Validators.min(0)]],
            salePrice: [0],
            selectedUnit: ['base', Validators.required],
            unitId: [null],
            storageLocation: ['']
        });
    }


    getDefaultExpiryDate(): Date {
        const d = new Date();
        d.setMonth(d.getMonth() + 3);
        return d;
    }


    ngOnInit() {
        this.loadSuppliers();
        this.loadWarehouses();

        // Dynamic validation for Supplier based on Payment Method
        this.purchaseForm.get('paymentMethod')?.valueChanges.subscribe(method => {
            const supplierCtrl = this.purchaseForm.get('supplierId');
            if (method === 2) {
                // الآجل (Credit) - المورد إلزامي
                supplierCtrl?.setValidators([Validators.required]);
            } else {
                // النقدي (Cash) - المورد اختياري
                supplierCtrl?.clearValidators();
            }
            supplierCtrl?.updateValueAndValidity();
        });
        // Trigger initial validation
        this.purchaseForm.get('paymentMethod')?.updateValueAndValidity();
        const id = this.route.snapshot.params['id'];
        if (id) {
            this.isEditMode = true;
            this.currentInvoiceId = +id;
            this.loadInvoice(id);
        } else {
            // Check for pre-filled medicine from query params
            const medicineId = this.route.snapshot.queryParams['medicineId'];
            if (medicineId) {
                this.inventoryService.getMedicineById(+medicineId).subscribe({
                    next: (medicine) => {
                        this.onMedicineSelect(medicine);
                    }
                });
            }
        }
    }

    // 🔍 BARCODE SCANNER & KEYBOARD SHORTCUTS ENGINE
    private barcodeBuffer = '';
    private lastKeyTime = 0;
    simulatorVisible = false;
    readonly transactionType = TransactionType.Purchase;

    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        // Prevent handling hotkeys when user is typing inside explicit non-modal inputs if needed
        const currentTime = new Date().getTime();

        if (currentTime - this.lastKeyTime > 50) {
            this.barcodeBuffer = '';
        }

        // Handle Barcode Scanners (fast sequence ending with Enter)
        if (event.key === 'Enter') {
            if (this.barcodeBuffer.length > 3) {
                this.processScannedBarcode(this.barcodeBuffer);
                this.barcodeBuffer = '';
                event.preventDefault();
                return;
            }
        } else if (event.key.length === 1 && !event.altKey && !event.ctrlKey && !event.metaKey) {
            this.barcodeBuffer += event.key;
        }

        this.lastKeyTime = currentTime;

        // Global Function Keys Shortcuts (F1, F2, F3, Esc)
        if (event.key === 'F1') {
            event.preventDefault();
            this.shortcutsHelpVisible = !this.shortcutsHelpVisible;
            return;
        }

        if (event.key === 'F2') {
            event.preventDefault();
            if (!this.isReadOnly && this.details.length > 0 && !this.saving) {
                this.approveInvoice();
            }
            return;
        }

        if (event.key === 'F3') {
            event.preventDefault();
            if (!this.isReadOnly && !this.saving) {
                this.saveDraft();
            }
            return;
        }

        if (event.key === 'Escape') {
            if (this.itemModalVisible) {
                event.preventDefault();
                this.closeItemModal();
                return;
            }
            if (this.shortcutsHelpVisible) {
                event.preventDefault();
                this.shortcutsHelpVisible = false;
                return;
            }
        }
    }

    processScannedBarcode(barcode: string) {
        this.messageService.add({ severity: 'info', summary: 'جاري البحث', detail: `تم مسح الباركود: ${barcode}` });

        this.barcodeService.processBarcode({
            barcode: barcode,
            transactionType: TransactionType.Purchase
        }).subscribe({
            next: (res) => {
                if (res.success && res.data) {
                    this.addBarcodeItemToInvoice(res.data);
                } else {
                    this.messageService.add({ severity: 'error', summary: 'فشل', detail: res.message || 'الصنف غير موجود' });
                }
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: err.error?.message || 'حدث خطأ أثناء معالجة الباركود'
                });
            }
        });
    }

    private addBarcodeItemToInvoice(data: any) {
        const existingDetails = this.details.controls;
        const existingIndex = existingDetails.findIndex(ctrl => ctrl.get('medicineId')?.value === data.medicineId && ctrl.get('companyBatchNumber')?.value === data.batchNumber);

        if (existingIndex !== -1) {
            const ctrl = this.details.at(existingIndex);
            const currentQty = ctrl.get('quantity')?.value || 0;
            ctrl.patchValue({
                quantity: currentQty + 1,
                total: (currentQty + 1) * (ctrl.get('purchasePrice')?.value || 0)
            });
            this.messageService.add({ severity: 'success', summary: 'تحديث الكمية', detail: `تم زيادة كمية ${data.tradeName}` });
        } else {
            const detail = {
                medicineId: data.medicineId,
                medicineName: data.tradeName,
                companyBatchNumber: data.batchNumber || 'جديد',
                expiryDate: data.expiryDate ? new Date(data.expiryDate) : null,
                quantity: 1,
                bonusQuantity: 0,
                purchasePrice: data.salePrice || 0,
                salePrice: data.salePrice || 0,
                total: data.salePrice || 0
            };
            this.addDetailToForm(detail);
            this.messageService.add({ severity: 'success', summary: 'إضافة صنف', detail: `تم إضافة ${data.tradeName} للفاتورة` });
        }
    }

    toggleSimulator() {
        this.simulatorVisible = !this.simulatorVisible;
    }

    loadSuppliers() {
        this.supplierService.getAll({ pageSize: 1000 }).subscribe((res: any) => this.suppliers = res.items);
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe((warehouses) => {
            this.warehouses = warehouses;
            if (!this.purchaseForm.get('warehouseId')?.value) {
                const defaultWarehouse = warehouses.find(w => w.type === WarehouseType.Main) || warehouses[0];
                if (defaultWarehouse) {
                    this.purchaseForm.patchValue({ warehouseId: defaultWarehouse.id });
                }
            }
        });
    }

    onSupplierChange() {
        // Auto focus next field (Payment Method) after picking supplier
        setTimeout(() => {
            if (this.paymentDropdown) {
                this.paymentDropdown.focus();
            }
        }, 100);
    }

    get isReadOnly() {
        if (!this.status) return false;
        const s = this.status.toString().toUpperCase();
        if (s === 'DRAFT' || s === '1' || Number(this.status) === DocumentStatus.Draft) {
            return false;
        }
        return true;
    }

    getStatusLabel() {
        if (this.status === undefined || this.status === null) return 'مسودة';
        const s = this.status.toString().toUpperCase();
        if (s === 'APPROVED' || s === '2' || Number(this.status) === DocumentStatus.Approved) {
            return 'تم التوريد';
        }
        if (s === 'DRAFT' || s === '1' || Number(this.status) === DocumentStatus.Draft) {
            return 'مسودة';
        }
        if (s === 'CANCELLED' || s === '3' || Number(this.status) === DocumentStatus.Cancelled) {
            return 'ملغاة';
        }
        return 'مسودة';
    }

    getStatusClass() {
        if (this.status === undefined || this.status === null) return 'draft';
        const s = this.status.toString().toUpperCase();
        if (s === 'APPROVED' || s === '2' || Number(this.status) === DocumentStatus.Approved) {
            return 'approved';
        }
        if (s === 'DRAFT' || s === '1' || Number(this.status) === DocumentStatus.Draft) {
            return 'draft';
        }
        if (s === 'CANCELLED' || s === '3' || Number(this.status) === DocumentStatus.Cancelled) {
            return 'cancelled';
        }
        return 'draft';
    }

    get details() {
        return this.purchaseForm.get('purchaseInvoiceDetails') as FormArray;
    }


    loadInvoice(id: number) {
        this.purchaseService.getById(id).subscribe((data: PurchaseInvoice) => {
            const isDraft = (data.status as any) === 'Draft' || data.status === DocumentStatus.Draft || Number(data.status) === 1;
            if (!isDraft) {
                this.router.navigate(['/purchases', id]);
                return;
            }
            this.purchaseForm.patchValue({
                ...data,
                purchaseDate: new Date(data.purchaseDate)
            });
            this.status = data.status || DocumentStatus.Draft;
            this.details.clear();

            if (data.items && data.items.length > 0) {
                const firstItemWithLocation = data.items.find(i => (i as any).storageLocation);
                if (firstItemWithLocation) {
                    this.purchaseForm.patchValue({ storageLocation: (firstItemWithLocation as any).storageLocation });
                }
            }

            data.items?.forEach((d: any) => this.addDetailToForm(d));
        });
    }

    addDetailToForm(detail: any) {
        const displayQuantity = detail.quantityInPurchaseUnit || detail.quantity;

        const group = this.fb.group({
            id: [detail.id || 0],
            medicineId: [detail.medicineId, Validators.required],
            medicineName: [detail.medicineName],
            companyBatchNumber: [detail.companyBatchNumber],
            expiryDate: [detail.expiryDate ? new Date(detail.expiryDate) : null],
            quantity: [displayQuantity, [Validators.required, Validators.min(1)]],
            purchaseUnitId: [detail.purchaseUnitId || null],
            bonusQuantity: [detail.bonusQuantity || 0],
            purchasePrice: [detail.purchasePrice, Validators.required],
            salePrice: [detail.salePrice, Validators.required],
            total: [detail.total || (displayQuantity * detail.purchasePrice)],
            storageLocation: [detail.storageLocation || null]
        });
        this.details.push(group);
    }

    // --- Search & Medicine Selection Workflow ---

    searchMedicines(event: any) {
        this.inventoryService.searchMedicines({ search: event.query }).subscribe(res => {
            this.filteredMedicines = res.items;
        });
    }

    onMedicineSelect(medicine: Medicine) {
        this.selectedMedicine = medicine;
        this.baseUnitName = medicine.baseUnitName || 'حبة';
        this.buildUnitOptions(medicine);

        // Auto-generate batch number: BAT-YYMMDD-XXXX
        const date = new Date();
        const yy = String(date.getFullYear()).slice(-2);
        const mm = String(date.getMonth() + 1).padStart(2, '0');
        const dd = String(date.getDate()).padStart(2, '0');
        const randomString = Math.random().toString(36).substring(2, 6).toUpperCase();
        const autoBatchNumber = `BAT-${yy}${mm}${dd}-${randomString}`;

        this.inlineItemForm.patchValue({
            medicineId: medicine.id,
            medicineName: medicine.name,
            selectedUnit: 'base',
            companyBatchNumber: autoBatchNumber,
            expiryDate: this.getDefaultExpiryDate(),
            quantity: 1,
            bonusQuantity: 0,
            price: medicine.defaultPurchasePrice || 0,
            salePrice: medicine.defaultSalePrice || 0,
            unitId: null,
            storageLocation: this.purchaseForm.get('storageLocation')?.value || ''
        });

        // Open modal automatically
        this.itemModalVisible = true;

        // Auto focus & select text on Quantity input inside modal
        setTimeout(() => {
            this.focusAndSelectQtyInput();
        }, 150);
    }

    buildUnitOptions(medicine: Medicine) {
        this.unitOptions = [
            {
                label: `${this.baseUnitName} (أساسية)`,
                value: 'base',
                purchasePrice: medicine.defaultPurchasePrice || 0,
                salePrice: medicine.defaultSalePrice || 0
            }
        ];
        if (medicine.medicineUnits && medicine.medicineUnits.length > 0) {
            medicine.medicineUnits.forEach(u => {
                this.unitOptions.push({
                    label: `${u.name} (${u.conversionFactor} ${this.baseUnitName})`,
                    value: u.id,
                    purchasePrice: u.defaultPurchasePrice,
                    salePrice: u.defaultSalePrice
                });
            });
        }
    }

    onUnitSelect(event: any) {
        const unitVal = event.value;
        const option = this.unitOptions.find(o => o.value === unitVal);
        if (option) {
            this.inlineItemForm.patchValue({
                price: option.purchasePrice,
                salePrice: option.salePrice,
                unitId: option.value === 'base' ? null : option.value
            });
        }
    }

    confirmModalItem() {
        if (this.inlineItemForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال الحقول المطلوبة' });
            return;
        }

        const val = this.inlineItemForm.getRawValue();
        const selectedUnitVal = val.selectedUnit;
        const option = this.unitOptions.find(o => o.value === selectedUnitVal);

        let finalName = val.medicineName;
        if (option && selectedUnitVal !== 'base') {
            const unitName = option.label.split(' ')[0];
            finalName = `${val.medicineName} (${unitName})`;
        }

        const detail = {
            medicineId: val.medicineId,
            medicineName: finalName,
            companyBatchNumber: val.companyBatchNumber,
            expiryDate: val.expiryDate,
            quantity: val.quantity,
            purchaseUnitId: val.unitId,
            bonusQuantity: val.bonusQuantity,
            purchasePrice: val.price,
            salePrice: val.salePrice,
            total: val.quantity * val.price,
            storageLocation: val.storageLocation || this.purchaseForm.get('storageLocation')?.value || null
        };

        if (this.editingIndex !== null) {
            this.details.at(this.editingIndex).patchValue(detail);
            this.messageService.add({ severity: 'success', summary: 'تم التعديل', detail: `تم تعديل ${val.medicineName}` });
            this.editingIndex = null;
        } else {
            this.addDetailToForm(detail);
            this.messageService.add({ severity: 'success', summary: 'تمت الإضافة', detail: `تم إضافة ${val.medicineName} إلى الفاتورة` });
        }

        this.closeItemModal();
    }

    closeItemModal() {
        this.itemModalVisible = false;
        this.editingIndex = null;
        this.selectedMedicine = null;
        this.searchQueryText = null;

        this.inlineItemForm.reset({
            quantity: 1,
            bonusQuantity: 0,
            price: 0,
            salePrice: 0,
            selectedUnit: 'base',
            expiryDate: this.getDefaultExpiryDate(),
            storageLocation: ''
        });

        // Focus back to Medicine Search AutoComplete automatically
        setTimeout(() => {
            this.focusMedicineSearch();
        }, 150);
    }

    editItem(index: number) {
        const item = this.details.at(index).value;
        this.editingIndex = index;

        this.inventoryService.getMedicineById(item.medicineId).subscribe(medicine => {
            this.selectedMedicine = medicine;
            this.baseUnitName = medicine.baseUnitName || 'حبة';
            this.buildUnitOptions(medicine);

            this.inlineItemForm.patchValue({
                medicineId: item.medicineId,
                medicineName: item.medicineName,
                companyBatchNumber: item.companyBatchNumber,
                expiryDate: item.expiryDate ? new Date(item.expiryDate) : null,
                quantity: item.quantity,
                bonusQuantity: item.bonusQuantity || 0,
                price: item.purchasePrice,
                salePrice: item.salePrice,
                selectedUnit: item.purchaseUnitId || 'base',
                unitId: item.purchaseUnitId,
                storageLocation: item.storageLocation || ''
            });

            this.itemModalVisible = true;
            setTimeout(() => {
                this.focusAndSelectQtyInput();
            }, 150);
        });
    }

    removeItem(index: number) {
        this.details.removeAt(index);
        this.messageService.add({ severity: 'info', summary: 'حذف صنف', detail: 'تم حذف الصنف من الفاتورة' });
    }

    // Auto Focus Helpers
    focusAndSelectQtyInput() {
        if (this.qtyInputEl) {
            const inputNative = this.qtyInputEl.inputFocusEL?.nativeElement || this.qtyInputEl.el?.nativeElement?.querySelector('input');
            if (inputNative) {
                inputNative.focus();
                inputNative.select();
            }
        }
    }

    focusMedicineSearch() {
        if (this.medicineAutoComplete) {
            const inputElem = this.medicineAutoComplete.el?.nativeElement?.querySelector('input');
            if (inputElem) {
                inputElem.value = '';
                inputElem.focus();
                inputElem.select();
            }
        }
    }

    onInputFocus(event: any) {
        const target = event?.target || event?.originalEvent?.target;
        if (target && target.select) {
            target.select();
        }
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
        return this.calculateTotal();
    }

    saveDraft() {
        if (this.purchaseForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال الحقول المطلوبة (المورد والبيانات الأساسية)' });
            return;
        }

        if (this.details.length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن حفظ فاتورة بدون أصناف' });
            return;
        }

        this.saving = true;
        const formValue = this.purchaseForm.getRawValue();

        const items = (formValue.purchaseInvoiceDetails || []).map((d: any) => ({
            medicineId: d.medicineId,
            companyBatchNumber: d.companyBatchNumber,
            expiryDate: d.expiryDate,
            quantity: d.quantity,
            purchaseUnitId: d.purchaseUnitId,
            bonusQuantity: d.bonusQuantity,
            purchasePrice: d.purchasePrice,
            salePrice: d.salePrice,
            storageLocation: d.storageLocation || null,
            batchBarcode: d.batchBarcode
        }));

        const payload = {
            supplierId: formValue.supplierId,
            warehouseId: formValue.warehouseId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber || null,
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
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الفاتورة كمسودة بنجاح' });
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

        const items = (formValue.purchaseInvoiceDetails || []).map((d: any) => ({
            medicineId: d.medicineId,
            companyBatchNumber: d.companyBatchNumber,
            expiryDate: d.expiryDate,
            quantity: d.quantity,
            purchaseUnitId: d.purchaseUnitId,
            bonusQuantity: d.bonusQuantity,
            purchasePrice: d.purchasePrice,
            salePrice: d.salePrice,
            storageLocation: d.storageLocation || null
        }));

        const payload = {
            supplierId: formValue.supplierId,
            warehouseId: formValue.warehouseId,
            supplierInvoiceNumber: formValue.supplierInvoiceNumber || null,
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
                const invoiceId = (this.isEditMode && this.currentInvoiceId) ? this.currentInvoiceId : res?.id;

                if (!invoiceId) {
                    this.saving = false;
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'تعذر الحصول على رقم الفاتورة' });
                    return;
                }

                this.purchaseService.approve(invoiceId).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم التوريد والترحيل للمخازن بنجاح' });
                        this.router.navigate(['/purchases', invoiceId]);
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
}

