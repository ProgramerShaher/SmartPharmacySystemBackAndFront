import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SaleInvoice, CreateSaleInvoiceDto } from '../../../../core/models';
import { Medicine, MedicineBatch } from '../../../../core/models';
import { MedicineBatchResponseDto } from '../../../../core/models/medicine-batch.interface';
import { MessageService } from 'primeng/api';
// PrimeNG Imports
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CardModule } from 'primeng/card';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MedicineService } from '../../../inventory/services/medicine.service';
import { MedicineBatchService } from '../../../inventory/services/medicine-batch.service';
import { CustomerService } from '../../../customers/services/customer.service';
import { DropdownModule } from "primeng/dropdown";
import { BarcodeService } from '../../../../core/services/barcode.service';
import { BarcodeSimulatorComponent } from '../../../../shared/components/barcode-simulator/barcode-simulator.component';
import { TransactionType } from '../../../../core/models/barcode.interface';
import { HostListener } from '@angular/core';
import { finalize } from 'rxjs/operators';

interface InvoiceItem {
    medicineId: number;
    medicineName: string;
    batchId: number;
    batchNumber: string;
    quantity: number;
    salePrice: number;
    unitCost: number;
    total: number;
    profit: number;
    expiryDate?: Date;
    availableQuantity: number;
    saleUnitId?: number | null;
    unitName?: string;
}

import { ShiftService } from '../../../../core/services/shift.service';

@Component({
    selector: 'app-sale-invoice-create',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        TableModule,
        AutoCompleteModule,
        CardModule,
        CalendarModule,
        ToastModule,
        DividerModule,
        TagModule,
        TooltipModule,
        DialogModule,
        InputSwitchModule,
        DropdownModule,
        BarcodeSimulatorComponent
    ],
    templateUrl: './sale-invoice-create.component.html',
    styleUrls: ['./sale-invoice-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [MessageService]
})
export class SaleInvoiceCreateComponent implements OnInit {
    // 🚀 COCKPIT SIGNALS
    items = signal<InvoiceItem[]>([]);
    discount = signal<number>(0);

    // ⚡ REACTIVE TOTALS (0ms Latency)
    subtotal = computed(() => this.items().reduce((sum, item) => sum + item.total, 0));
    totalProfit = computed(() => this.items().reduce((sum, item) => sum + item.profit, 0));
    total = computed(() => Math.max(0, this.subtotal() - this.discount()));
    totalQuantity = computed(() => this.items().reduce((sum, item) => sum + item.quantity, 0));

    // 💡 INLINE LIVE TOTAL (Instant Calculation)
    get inlineLivePrice(): number {
        if (this.inlineBatch) {
            if (this.inlineUnit && this.inlineUnit.salePrice) {
                return this.inlineUnit.salePrice;
            }
            const unitFactor = this.inlineUnit ? this.inlineUnit.factor : 1;
            const basePrice = this.inlineBatch.retailPrice || this.inlineBatch.unitPurchasePrice || 0;
            return basePrice * unitFactor;
        }
        return 0;
    }

    get inlineLiveTotal(): number {
        if (this.inlineBatch && this.inlineQuantity) {
            return this.inlineLivePrice * this.inlineQuantity;
        }
        return 0;
    }

    get maxAllowedQuantityInline(): number {
        if (!this.inlineBatch) return 0;
        const unitFactor = this.inlineUnit ? this.inlineUnit.factor : 1;
        return Math.floor(this.inlineBatch.remainingQuantity / unitFactor);
    }

    // 🛫 OPERATIONAL STATE
    invoiceDate = new Date();
    selectedCustomer: any = null;
    isCashCustomer = false; // "Flying Customer" Mode
    flyingCustomerName: string = ''; // Name for flying customer
    paymentMethod: 'Cash' | 'Credit' = 'Cash';

    // 🔍 SEARCH ENGINES
    filteredMedicines: Medicine[] = [];
    customers: any[] = []; // All customers loaded from backend

    // 📦 BATCH CONTROL
    availableBatches: MedicineBatchResponseDto[] = [];

    // ⚡ INLINE FORM STATE
    inlineMedicine: Medicine | null = null;
    inlineBatch: MedicineBatchResponseDto | null = null;
    inlineQuantity: number = 1;
    inlineUnitOptions: any[] = [];
    inlineUnit: any = null;
    editingItemIndex: number | null = null;

    // 💳 PAYMENT METHODS
    paymentMethods = [
        { label: 'نقدي', value: 1 },
        { label: 'آجل', value: 2 }
    ];
    selectedPaymentMethod = 1; // Default to Cash

    saving = false;
    isEditMode = false;
    invoiceId: number | null = null;

    constructor(
        private salesService: SaleInvoiceService,
        private messageService: MessageService,
        private medicineService: MedicineService,
        private medicineBatchService: MedicineBatchService,
        private customerService: CustomerService,
        private barcodeService: BarcodeService,
        private shiftService: ShiftService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
        // Check for open shift first
        this.shiftService.getCurrentShift().subscribe({
            next: (res) => {
                // If there's no shift, this will fall into error or return false if backend handles it
            },
            error: (err) => {
                // If API returns 400 Bad Request ("No open shift found")
                this.messageService.add({ severity: 'warn', summary: 'الوردية مغلقة', detail: 'يرجى فتح وردية جديدة للتمكن من البيع' });
                this.shiftService.requestShiftModal('open');
            }
        });

        this.loadCustomers(); // Load customers on init
        this.route.params.subscribe((params: any) => {
            if (params['id']) {
                this.isEditMode = true;
                this.invoiceId = +params['id'];
                this.loadInvoice(this.invoiceId);
            } else {
                // Default to Flying Customer for new invoices for speed
                this.isCashCustomer = true;
                this.toggleCashCustomer();
            }
        });

        // Handle Quick Sale
        this.route.queryParams.subscribe(params => {
            if (params['quickSaleMedicineId']) {
                const medicineId = +params['quickSaleMedicineId'];
                // Load medicine and open modal automatically
                this.medicineService.getById(medicineId).subscribe({
                    next: (medicine) => {
                        if (medicine) {
                            this.onMedicineSelectInline(medicine);
                        }
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'لم يتم العثور على الدواء المطلوب للبيع السريع' });
                    }
                });
            }
        });
    }

    // 🔍 BARCODE SCANNER ENGINE
    private barcodeBuffer = '';
    private lastKeyTime = 0;
    simulatorVisible = false;
    readonly transactionType = TransactionType.Sale;

    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        const currentTime = new Date().getTime();

        // If typing is very fast (< 30ms between keys), it's likely a scanner
        if (currentTime - this.lastKeyTime > 50) {
            this.barcodeBuffer = '';
        }

        if (event.key === 'Enter') {
            if (this.barcodeBuffer.length > 3) {
                this.processScannedBarcode(this.barcodeBuffer);
                this.barcodeBuffer = '';
                event.preventDefault();
            }
        } else if (event.key.length === 1) {
            this.barcodeBuffer += event.key;
        }

        this.lastKeyTime = currentTime;
    }

    processScannedBarcode(barcode: string) {
        this.messageService.add({ severity: 'info', summary: 'جاري البحث', detail: `تم مسح الباركود: ${barcode}` });

        this.barcodeService.processBarcode({
            barcode: barcode,
            transactionType: TransactionType.Sale
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
        // بدلاً من إضافة الصنف مباشرة للجدول، نضعه في حقول الإدخال بالأعلى ونحدد حقل الكمية ليقوم المستخدم بإدخالها

        // 1. تجهيز الدواء
        this.inlineMedicine = {
            id: data.medicineId,
            name: data.tradeName,
        } as any;

        // 2. تجهيز الوحدة الأساسية
        this.inlineUnitOptions = [
            { label: 'أساسية', value: null, factor: 1, salePrice: data.salePrice, name: 'أساسية' }
        ];
        this.inlineUnit = this.inlineUnitOptions[0];

        // 3. تجهيز الدفعة
        this.inlineBatch = {
            id: data.batchId,
            companyBatchNumber: data.batchNumber,
            retailPrice: data.salePrice,
            unitPurchasePrice: data.movingAverageCost,
            remainingQuantity: data.availableQuantity,
            expiryDate: data.expiryDate ? new Date(data.expiryDate).toISOString() : '',
            medicineId: data.medicineId,
            quantity: 0,
            soldQuantity: 0,
            status: 'Active',
            isDeleted: false,
            entryDate: '',
            isSellable: true,
            medicineName: data.tradeName,
            isExpired: false,
            isExpiringSoon: false,
            daysUntilExpiry: 0
        } as any;

        this.availableBatches = this.inlineBatch ? [this.inlineBatch] : [];

        // تجهيز الكمية لتكون 1 مبدئياً
        this.inlineQuantity = 1;
        this.editingItemIndex = null;

        this.messageService.add({ severity: 'success', summary: 'تم استدعاء الصنف', detail: `تم إدراج ${data.tradeName} أدخل الكمية` });

        // 4. الانتقال التلقائي لحقل الكمية وتظليل النص
        setTimeout(() => {
            const qtyInput = document.getElementById('inlineQty');
            if (qtyInput) {
                qtyInput.focus();
                (qtyInput as HTMLInputElement).select();
            }
        }, 100);
    }

    selectText(event: any) {
        if (event && event.originalEvent && event.originalEvent.target) {
            event.originalEvent.target.select();
        } else if (event && event.target) {
            event.target.select();
        }
    }

    toggleSimulator() {
        this.simulatorVisible = !this.simulatorVisible;
    }

    // 🔄 INTELLIGENT CUSTOMER TOGGLE
    toggleCashCustomer() {
        if (this.isCashCustomer) {
            this.selectedCustomer = null;
            this.paymentMethod = 'Cash';
        } else {
            this.paymentMethod = 'Credit'; // Default to credit if selecting a specific customer, can be changed logic
        }
    }

    // 💼 LOAD CUSTOMERS
    loadCustomers() {
        // Load all customers from backend
        this.customerService.getAll({ pageSize: 100 }).subscribe({
            next: (result) => {
                this.customers = result.items.map(c => ({
                    id: c.id,
                    name: c.name,
                    phone: c.phoneNumber || ''
                }));
            },
            error: () => {
                this.messageService.add({ severity: 'warn', summary: 'تحذير', detail: 'فشل تحميل قائمة العملاء' });
                // Fallback to empty list
                this.customers = [];
            }
        });
    }

    searchCustomer(event: any) {
        // Mock search for now or implement real service call
        // this.customerService.search(event.query)...
    }

    loadInvoice(id: number) {
        this.saving = true;
        this.salesService.getById(id).subscribe({
            next: (invoice: SaleInvoice) => {
                this.invoiceDate = new Date(invoice.invoiceDate);
                const method: any = invoice.paymentMethod;
                this.paymentMethod = (method === 2 || method === 'Credit') ? 'Credit' : 'Cash';

                if (invoice.customerId) {
                    this.selectedCustomer = { id: invoice.customerId, name: invoice.customerName };
                    this.isCashCustomer = false;
                } else {
                    this.isCashCustomer = true;
                }

                const mappedItems: InvoiceItem[] = (invoice.items || []).map(d => ({
                    medicineId: d.medicineId,
                    medicineName: d.medicineName || 'Unknown',
                    batchId: d.batchId || 0,
                    batchNumber: d.companyBatchNumber || '',
                    quantity: d.quantity,
                    salePrice: d.salePrice,
                    unitCost: 0, // Need fetch for profit calc if strict
                    total: d.quantity * d.salePrice,
                    profit: 0,
                    availableQuantity: 9999, // Fallback
                    saleUnitId: d.saleUnitId,
                    unitName: d.saleUnitId ? 'وحدة' : 'أساسية'
                }));
                this.items.set(mappedItems);
                this.saving = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Could not load invoice' });
                this.router.navigate(['/sales']);
            }
        });
    }

    // 💊 FAST SEARCH
    searchMedicine(event: any) {
        this.medicineService.getAll({ search: event.query, pageSize: 20 }).subscribe({
            next: (res) => this.filteredMedicines = res.items || [],
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Search failed' })
        });
    }

    updateItemQuantity(item: InvoiceItem, qty: number) {
        if (qty > item.availableQuantity) {
            this.messageService.add({ severity: 'warn', summary: 'Stock Limit', detail: `Only ${item.availableQuantity} available` });
            qty = item.availableQuantity;
        }

        item.quantity = qty;
        item.total = qty * item.salePrice;
        // Recalculate Profit: (Sale Price - Cost) * Qty
        item.profit = (item.salePrice - item.unitCost) * qty;

        this.items.set([...this.items()]); // Trigger Signal Update
    }

    removeItem(index: number) {
        this.items.update(current => current.filter((_, i) => i !== index));
    }

    // 💾 TRANSACTION ENGINE
    saveDraft() {
        this.submitInvoice(false);
    }

    approveInvoice() {
        this.submitInvoice(true);
    }

    private submitInvoice(approve: boolean) {
        if (this.items().length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'تحذير', detail: 'أضف صنف واحد على الأقل' });
            return;
        }

        if (!this.isCashCustomer && !this.selectedCustomer) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'اختر عميل أو فعّل وضع العميل الطيار' });
            return;
        }

        // Before saving, ensure shift is open
        this.shiftService.getCurrentShift().subscribe({
            next: () => {
                this.executeSubmitInvoice(approve);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'الوردية مغلقة', detail: 'لا يمكنك حفظ مبيعات بدون فتح وردية.' });
                this.shiftService.requestShiftModal('open');
            }
        });
    }

    private executeSubmitInvoice(approve: boolean) {
        this.saving = true;

        // If Flying Customer with a name - do NOT save to database, just pass the name to the invoice
        if (this.isCashCustomer && this.flyingCustomerName && this.flyingCustomerName.trim()) {
            this.createInvoiceWithCustomer(approve, null, this.flyingCustomerName.trim());
        } else if (this.isCashCustomer) {
            // Cash customer without name
            this.createInvoiceWithCustomer(approve, null, 'زبون نقدي');
        } else {
            // Selected customer from dropdown
            this.createInvoiceWithCustomer(approve, this.selectedCustomer?.id || null, this.selectedCustomer?.name || '');
        }
    }

    private createInvoiceWithCustomer(approve: boolean, customerId: number | null, customerName: string) {
        const payload: CreateSaleInvoiceDto = {
            invoiceDate: this.invoiceDate.toISOString(),
            paymentMethod: this.paymentMethod === 'Cash' ? 1 : 2,
            customerId: customerId,
            customerName: customerName,
            details: this.items().map(item => ({
                medicineId: item.medicineId,
                batchId: item.batchId,
                quantity: item.quantity,
                salePrice: item.salePrice,
                saleUnitId: item.saleUnitId
            })),
            notes: approve ? 'تم الاعتماد من نقطة البيع' : 'مسودة من نقطة البيع'
        };

        const request = this.isEditMode && this.invoiceId
            ? this.salesService.update(this.invoiceId, payload as any)
            : this.salesService.create(payload);

        request.subscribe({
            next: (invoice: any) => {
                if (approve) {
                    this.salesService.approve(invoice.id || this.invoiceId).subscribe({
                        next: () => {
                            this.saving = false;
                            this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم اعتماد الفاتورة وترحيل المخزون' });
                            this.router.navigate(['/sales']);
                        },
                        error: (err) => this.handleError(err)
                    });
                } else {
                    this.saving = false;
                    this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم حفظ المسودة بنجاح' });
                    this.router.navigate(['/sales']);
                }
            },
            error: (err) => this.handleError(err)
        });
    }

    private handleError(err: any) {
        this.saving = false;
        console.error(err);
        this.messageService.add({
            severity: 'error',
            summary: 'System Error',
            detail: err.error?.message || 'Transaction failed. Please check connection.'
        });
    }

    getBatchStatus(expiryDate?: Date | string): string {
        if (!expiryDate) return '';
        const now = new Date();
        const dateToCheck = new Date(expiryDate);
        const diffTime = dateToCheck.getTime() - now.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays < 30) return 'critical';
        if (diffDays < 90) return 'warning';
        return 'good';
    }

    goBack() {
        this.router.navigate(['/sales']);
    }

    clearList() {
        if (this.items().length > 0) {
            this.items.set([]);
            this.discount.set(0);
            this.messageService.add({ severity: 'info', summary: 'تم التفريغ', detail: 'تم تفريغ قائمة الأصناف' });
        }
    }

    // 🎭 INLINE FORM METHODS
    onMedicineSelectInline(medicine: Medicine) {
        this.inlineMedicine = medicine;
        this.inlineBatch = null;
        this.inlineQuantity = 1;

        const baseName = medicine.baseUnitName || 'حبة';
        this.inlineUnitOptions = [
            { label: `${baseName} (أساسية)`, value: null, factor: 1, salePrice: medicine.defaultSalePrice || 0, name: baseName }
        ];

        if (medicine.medicineUnits && medicine.medicineUnits.length > 0) {
            medicine.medicineUnits.forEach(u => {
                this.inlineUnitOptions.push({
                    label: `${u.name} (x${u.conversionFactor})`,
                    value: u.id,
                    factor: u.conversionFactor,
                    salePrice: u.defaultSalePrice || (medicine.defaultSalePrice * u.conversionFactor),
                    name: u.name
                });
            });
        }
        this.inlineUnit = this.inlineUnitOptions[0];

        // Load batches for selected medicine (FEFO order from Backend)
        this.medicineService.getFefoBatches(medicine.id).subscribe({
            next: (batches) => {
                this.availableBatches = batches
                    .filter(b => b.remainingQuantity > 0 && (b.isSellable ?? true));

                // Auto-select first batch (FEFO)
                if (this.availableBatches.length > 0) {
                    this.inlineBatch = this.availableBatches[0];
                } else {
                    this.messageService.add({ severity: 'warn', summary: 'نفاذ المخزون', detail: 'لا توجد دفعات متاحة لهذا الصنف' });
                }
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الدفعات' })
        });
    }

    onBatchSelectInline() {
        this.inlineQuantity = 1; // Reset quantity when batch changes
    }

    addItemInline() {
        if (!this.inlineMedicine || !this.inlineBatch || !this.inlineQuantity) {
            return;
        }

        // Selected Unit Info
        const unitFactor = this.inlineUnit ? this.inlineUnit.factor : 1;
        const saleUnitId = this.inlineUnit ? this.inlineUnit.value : null;
        const unitName = this.inlineUnit ? this.inlineUnit.name : 'أساسية';

        // Price calculations
        const baseSalePrice = this.inlineBatch.retailPrice || this.inlineBatch.unitPurchasePrice || 0;
        let salePrice = baseSalePrice * unitFactor;

        if (this.inlineUnit && this.inlineUnit.salePrice) {
            salePrice = this.inlineUnit.salePrice;
        }

        const baseUnitCost = this.inlineBatch.unitPurchasePrice || 0;
        const unitCost = baseUnitCost * unitFactor;

        // Validation against stock
        const requestedBaseUnits = this.inlineQuantity * unitFactor;
        if (requestedBaseUnits > this.inlineBatch.remainingQuantity) {
            this.messageService.add({ severity: 'error', summary: 'رصيد غير كاف', detail: `الكمية المتوفرة ${this.inlineBatch.remainingQuantity} وحدة أساسية فقط.` });
            return;
        }

        if (this.editingItemIndex !== null) {
            // Update existing item
            const item = this.items()[this.editingItemIndex];

            // Check if we changed batch and it conflicts with another existing item (other than the one being edited)
            const conflictingItemIndex = this.items().findIndex((i, idx) => i.batchId === this.inlineBatch!.id && idx !== this.editingItemIndex);

            if (conflictingItemIndex !== -1) {
                // Merge into the conflicting item and remove the current one
                const conflictingItem = this.items()[conflictingItemIndex];
                this.updateItemQuantity(conflictingItem, conflictingItem.quantity + this.inlineQuantity);
                this.removeItem(this.editingItemIndex);
                this.messageService.add({ severity: 'success', summary: 'تم الدمج', detail: 'تم دمج الكمية مع الدفعة الموجودة' });
            } else {
                // Update properties in place
                item.medicineId = this.inlineMedicine.id;
                item.medicineName = `${this.inlineMedicine.name} (${unitName})`;
                item.batchId = this.inlineBatch.id;
                item.batchNumber = this.inlineBatch.companyBatchNumber;
                item.quantity = this.inlineQuantity;
                item.salePrice = salePrice;
                item.unitCost = unitCost;
                item.total = this.inlineQuantity * salePrice;
                item.profit = (salePrice - unitCost) * this.inlineQuantity;
                item.expiryDate = new Date(this.inlineBatch.expiryDate);
                item.availableQuantity = Math.floor(this.inlineBatch.remainingQuantity / unitFactor);
                item.saleUnitId = saleUnitId;
                item.unitName = unitName;

                this.items.set([...this.items()]); // Trigger update
                this.messageService.add({ severity: 'success', summary: 'تم التعديل', detail: 'تم تعديل الصنف بنجاح' });
            }
        } else {
            // Add new item
            const existingItem = this.items().find(i => i.batchId === this.inlineBatch!.id);
            if (existingItem) {
                this.updateItemQuantity(existingItem, existingItem.quantity + this.inlineQuantity);
                this.messageService.add({ severity: 'success', summary: 'تم التحديث', detail: 'تم زيادة الكمية' });
            } else {
                const newItem: InvoiceItem = {
                    medicineId: this.inlineMedicine.id,
                    medicineName: `${this.inlineMedicine.name} (${unitName})`,
                    batchId: this.inlineBatch.id,
                    batchNumber: this.inlineBatch.companyBatchNumber,
                    quantity: this.inlineQuantity,
                    salePrice: salePrice,
                    unitCost: unitCost,
                    total: this.inlineQuantity * salePrice,
                    profit: (salePrice - unitCost) * this.inlineQuantity,
                    expiryDate: new Date(this.inlineBatch.expiryDate),
                    availableQuantity: Math.floor(this.inlineBatch.remainingQuantity / unitFactor),
                    saleUnitId: saleUnitId,
                    unitName: unitName
                };

                this.items.update(current => [...current, newItem]);
                this.messageService.add({ severity: 'success', summary: 'تمت الإضافة', detail: 'تم إضافة الصنف بنجاح' });
            }
        }

        this.resetInlineForm();
    }

    resetInlineForm() {
        this.inlineMedicine = null;
        this.inlineBatch = null;
        this.inlineQuantity = 1;
        this.availableBatches = [];
        this.inlineUnitOptions = [];
        this.inlineUnit = null;
        this.editingItemIndex = null;
    }

    editItemInline(item: InvoiceItem, index: number) {
        this.editingItemIndex = index;

        // Hydrate Medicine
        this.inlineMedicine = {
            id: item.medicineId,
            name: item.medicineName.split(' (')[0], // Extract base name without unit
        } as any;

        // Setup options manually to restore state quickly without making HTTP call if not needed
        this.inlineUnitOptions = [
            { label: `${item.unitName}`, value: item.saleUnitId, factor: 1, salePrice: item.salePrice, name: item.unitName }
        ];
        this.inlineUnit = this.inlineUnitOptions[0];

        // Hydrate Batch
        this.inlineBatch = {
            id: item.batchId,
            companyBatchNumber: item.batchNumber,
            retailPrice: item.salePrice,
            unitPurchasePrice: item.unitCost,
            remainingQuantity: item.availableQuantity, // Simplified for editing view
            expiryDate: item.expiryDate ? item.expiryDate.toISOString() : '',
            medicineId: item.medicineId,
            quantity: 0,
            soldQuantity: 0,
            status: 'Active',
            isDeleted: false,
            entryDate: '',
            isSellable: true,
            medicineName: item.medicineName,
            isExpired: false,
            isExpiringSoon: false,
            daysUntilExpiry: 0
        } as MedicineBatchResponseDto;

        this.availableBatches = [this.inlineBatch];
        this.inlineQuantity = item.quantity;
    }
}
