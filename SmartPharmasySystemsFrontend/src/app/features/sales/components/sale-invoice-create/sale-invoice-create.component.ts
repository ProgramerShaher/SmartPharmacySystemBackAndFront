import { Component, OnInit, AfterViewInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SaleInvoice, CreateSaleInvoiceDto } from '../../../../core/models';
import { Medicine, MedicineBatch } from '../../../../core/models';
import { MedicineBatchResponseDto } from '../../../../core/models/medicine-batch.interface';
import { MessageService } from 'primeng/api';
import { PricelistService, PricelistSelectDto, PricelistItem } from '../../../inventory/services/pricelist.service';
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
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from "primeng/dropdown";
import { BarcodeService } from '../../../../core/services/barcode.service';
import { BarcodeSimulatorComponent } from '../../../../shared/components/barcode-simulator/barcode-simulator.component';
import { TransactionType } from '../../../../core/models/barcode.interface';
import { HostListener, ChangeDetectorRef, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';

interface InvoiceItem {
    medicineId: number;
    medicineName: string;
    barcode?: string;
    batchId: number;
    batchNumber: string;
    quantity: number;
    salePrice: number;
    unitCost: number;
    total: number;         // total BEFORE discount
    netTotal: number;      // total AFTER line discount
    profit: number;
    expiryDate?: Date;
    availableQuantity: number;
    saleUnitId?: number | null;
    unitName?: string;
    /** Manual discount applied by cashier (0-100%) */
    discountPercentage: number;
    /** Calculated discount amount for this line */
    discountAmount: number;
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
        ProgressSpinnerModule,
        BarcodeSimulatorComponent
    ],
    templateUrl: './sale-invoice-create.component.html',
    styleUrls: ['./sale-invoice-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [MessageService]
})
export class SaleInvoiceCreateComponent implements OnInit, AfterViewInit {
    // 🚀 COCKPIT SIGNALS
    items = signal<InvoiceItem[]>([]);
    discount = signal<number>(0);

    // 🏷️ PRICELIST
    /** Global pricelist discount from customer pricelist (e.g. 10%) */
    customerPricelistId = signal<number | null>(null);
    customerPricelistDiscount = signal<number>(0);
    activePricelistItems = signal<PricelistItem[]>([]); // To store item-specific overrides

    // ⚡ REACTIVE TOTALS (0ms Latency)
    subtotal = computed(() => this.items().reduce((sum, item) => sum + item.total, 0));
    totalDiscount = computed(() => this.items().reduce((sum, item) => sum + item.discountAmount, 0) + this.discount());
    totalProfit = computed(() => this.items().reduce((sum, item) => sum + item.profit, 0));
    total = computed(() => Math.max(0, this.subtotal() - this.totalDiscount()));
    totalQuantity = computed(() => this.items().reduce((sum, item) => sum + item.quantity, 0));

    get inlineLivePrice(): number {
        if (this.inlineBatch) {
            const unitFactor = this.inlineUnit ? this.inlineUnit.factor : 1;
            const basePrice = (this.inlineBatch.retailPrice !== undefined && this.inlineBatch.retailPrice !== null && this.inlineBatch.retailPrice !== 0)
                ? this.inlineBatch.retailPrice
                : ((this.inlineUnit && this.inlineUnit.salePrice) ? (this.inlineUnit.salePrice / unitFactor) : 0);
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
        if (this.availableBatches && this.availableBatches.length > 0) {
            const totalRemaining = this.availableBatches.reduce((sum, b) => sum + (b.remainingQuantity || 0), 0);
            return Math.floor(totalRemaining / unitFactor);
        }
        return 99999;
    }

    get currentBatchAvailableQuantity(): number {
        if (!this.inlineBatch) return 0;
        const unitFactor = this.inlineUnit ? this.inlineUnit.factor : 1;
        return Math.floor((this.inlineBatch.remainingQuantity || 0) / unitFactor);
    }

    // 🛫 OPERATIONAL STATE
    invoiceDate = new Date();
    selectedCustomer: any = null;
    isCashCustomer = true; // "Flying Customer" Mode
    flyingCustomerName: string = ''; // Name for flying customer
    paymentMethod: 'Cash' | 'Credit' = 'Cash';
    selectedMedicineBarcode: string | null = null;

    customerTypeOptions = [
        { label: 'عميل نقدي (طيار) ✈️', value: 'cash' },
        { label: 'عميل مسجل 👤', value: 'registered' }
    ];
    selectedCustomerType: 'cash' | 'registered' = 'cash';

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

    // 💰 DRAWER & DIALOG STATES
    drawerLedgerVisible = false;
    drawerLedger: any = null;
    loadingDrawer = false;
    shortcutsHelpVisible = false;
    itemModalVisible = false;
    searchQueryText = '';

    // 🎯 VIEWCHILD REFERENCES
    @ViewChild('medicineAutoComplete') medicineAutoComplete: any;
    @ViewChild('qtyInputEl') qtyInputEl: any;

    constructor(
        private salesService: SaleInvoiceService,
        private messageService: MessageService,
        private medicineService: MedicineService,
        private medicineBatchService: MedicineBatchService,
        private customerService: CustomerService,
        private barcodeService: BarcodeService,
        private shiftService: ShiftService,
        private pricelistService: PricelistService,
        private router: Router,
        private route: ActivatedRoute,
        private cdr: ChangeDetectorRef
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

        // Preload product and customer lookup indexes asynchronously in background
        this.medicineService.loadLookupIndex().subscribe();
        this.customerService.loadLookupIndex().subscribe();

        this.loadCustomers(); // Load customers on init

        this.route.params.subscribe((params: any) => {
            if (params['id']) {
                this.isEditMode = true;
                this.invoiceId = +params['id'];
                this.loadInvoice(this.invoiceId);
            } else {
                // Default to Flying Customer for new invoices for speed
                this.isCashCustomer = true;
                this.selectedCustomerType = 'cash';
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

    ngAfterViewInit() {
        setTimeout(() => {
            this.focusSearchInput();
        }, 300);
    }

    onCustomerTypeChange(type: 'cash' | 'registered') {
        this.selectedCustomerType = type;
        this.isCashCustomer = (type === 'cash');
        this.toggleCashCustomer();
    }

    focusNext(elementId: string, event?: Event) {
        if (event) {
            event.preventDefault();
        }
        const el = document.getElementById(elementId);
        if (el) {
            el.focus();
            if (el instanceof HTMLInputElement) {
                el.select();
            }
        }
    }

    getStatusClass(): string {
        return this.isEditMode ? 'status-draft' : 'status-completed';
    }

    getStatusLabel(): string {
        return this.isEditMode ? 'تعديل فاتورة' : 'فاتورة جديدة';
    }

    // 🔍 BARCODE SCANNER & HOTKEY ENGINE
    private barcodeBuffer = '';
    private lastKeyTime = 0;
    simulatorVisible = false;
    readonly transactionType = TransactionType.Sale;

    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        // 1. Hotkeys Handling
        if (event.key === 'F1') {
            event.preventDefault();
            this.shortcutsHelpVisible = !this.shortcutsHelpVisible;
            return;
        }
        if (event.key === 'F2') {
            event.preventDefault();
            if (this.items().length > 0 && !this.saving) {
                this.approveInvoice();
            }
            return;
        }
        if (event.key === 'F3') {
            event.preventDefault();
            if (!this.saving) {
                this.saveDraft();
            }
            return;
        }
        if (event.key === 'F4') {
            event.preventDefault();
            this.isCashCustomer = !this.isCashCustomer;
            this.toggleCashCustomer();
            return;
        }
        if (event.key === 'Escape') {
            if (this.shortcutsHelpVisible) {
                this.shortcutsHelpVisible = false;
                event.preventDefault();
                return;
            }
            if (this.itemModalVisible) {
                this.closeItemModal();
                event.preventDefault();
                return;
            }
            if (this.drawerLedgerVisible) {
                this.drawerLedgerVisible = false;
                event.preventDefault();
                return;
            }
            this.goBack();
            return;
        }

        // 2. Barcode scanner buffer handling
        const currentTime = new Date().getTime();
        if (currentTime - this.lastKeyTime > 50) {
            this.barcodeBuffer = '';
        }

        if (event.key === 'Enter') {
            if (this.barcodeBuffer.length > 3) {
                this.processScannedBarcode(this.barcodeBuffer);
                this.barcodeBuffer = '';
                event.preventDefault();
            }
        } else if (event.key.length === 1 && !event.ctrlKey && !event.altKey && !event.metaKey) {
            this.barcodeBuffer += event.key;
        }

        this.lastKeyTime = currentTime;
    }

    processScannedBarcode(barcode: string) {
        if (!barcode) return;
        const localMed = this.medicineService.getByBarcodeLocal(barcode);
        if (localMed) {
            this.onMedicineSelectInline(localMed);
            return;
        }

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
        if (!data.availableQuantity || data.availableQuantity <= 0) {
            this.itemModalVisible = false;
            this.messageService.add({
                severity: 'warn',
                summary: 'لا توجد دفعة متوفرة',
                detail: `عفواً، لا توجد أي دفعة متوفرة بالمخزون لدواء (${data.tradeName})`
            });
            setTimeout(() => this.focusSearchInput(), 100);
            return;
        }

        // 1. تجهيز الدواء
        this.selectedMedicineBarcode = data.barcode || data.defaultBarcode || null;
        this.inlineMedicine = {
            id: data.medicineId,
            name: data.tradeName,
            defaultBarcode: this.selectedMedicineBarcode
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

        this.itemModalVisible = true;
        setTimeout(() => this.focusQtyInput(), 100);
    }

    selectText(event: any) {
        if (event && event.originalEvent && event.originalEvent.target) {
            event.originalEvent.target.select();
        } else if (event && event.target) {
            event.target.select();
        }
    }

    onInputFocus(event: any) {
        if (event && event.originalEvent && event.originalEvent.target) {
            event.originalEvent.target.select();
        } else if (event && event.target && typeof event.target.select === 'function') {
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
            this.selectedPaymentMethod = 1;
        } else {
            this.paymentMethod = 'Cash'; // Default to cash as requested
            this.selectedPaymentMethod = 1;
        }
    }

    // 💼 LOAD CUSTOMERS
    loadCustomers() {
        this.customerService.loadLookupIndex().subscribe({
            next: (items) => {
                this.customers = items.map(c => ({
                    id: c.id,
                    name: c.name,
                    phone: c.phoneNumber || '',
                    pricelistId: (c as any).pricelistId || null,
                    pricelistDiscountPercentage: (c as any).pricelistDiscountPercentage || 0
                }));
            },
            error: () => {
                this.messageService.add({ severity: 'warn', summary: 'تحذير', detail: 'فشل تحميل قائمة العملاء' });
                this.customers = [];
            }
        });
    }

    /** Called when cashier selects a customer from dropdown - loads their pricelist */
    onCustomerChange(customer: any) {
        if (customer && customer.pricelistId) {
            this.customerPricelistId.set(customer.pricelistId);
            this.customerPricelistDiscount.set(customer.pricelistDiscountPercentage || 0);

            // Fetch the full pricelist to get item overrides
            this.pricelistService.getById(customer.pricelistId).subscribe({
                next: (pricelist) => {
                    this.activePricelistItems.set(pricelist.items || []);

                    if (customer.pricelistDiscountPercentage > 0 || (pricelist.items && pricelist.items.length > 0)) {
                        this.messageService.add({
                            severity: 'info',
                            summary: 'قائمة أسعار مخصصة',
                            detail: `تم تطبيق قائمة أسعار العميل (${pricelist.name}) بنجاح`
                        });
                        // Recalculate all existing items using the newly fetched pricelist
                        this.recalculateAllItems();
                    }
                },
                error: () => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل جلب تفاصيل قائمة أسعار العميل' });
                }
            });
        } else {
            this.customerPricelistId.set(null);
            this.customerPricelistDiscount.set(0);
            this.activePricelistItems.set([]);
            this.recalculateAllItems(); // Remove discounts if customer is changed back to standard
        }
    }

    /** Recalculate all items based on current active pricelist (global + overrides) */
    recalculateAllItems() {
        const updated = this.items().map(item => {
            const override = this.activePricelistItems().find(p => p.medicineId === item.medicineId);

            let finalDiscountPct = this.customerPricelistDiscount(); // default global

            if (override) {
                if (override.fixedPrice !== undefined && override.fixedPrice !== null) {
                    // Calculate equivalent discount percentage for the fixed price
                    if (item.salePrice > 0 && override.fixedPrice < item.salePrice) {
                        finalDiscountPct = ((item.salePrice - override.fixedPrice) / item.salePrice) * 100;
                    } else {
                        finalDiscountPct = 0; // If fixed price is higher or equal, 0 discount (or handle differently)
                    }
                } else if (override.discountPercentage !== undefined && override.discountPercentage !== null) {
                    finalDiscountPct = override.discountPercentage;
                }
            }

            const discountAmount = Math.round((item.total * finalDiscountPct / 100) * 100) / 100;
            return { ...item, discountPercentage: Math.round(finalDiscountPct * 100) / 100, discountAmount, netTotal: item.total - discountAmount };
        });
        this.items.set(updated);
    }

    /** Update discount for a specific line item */
    updateItemDiscount(index: number, discountPct: number) {
        const current = this.items();
        if (current[index]) {
            const item = current[index];
            item.discountPercentage = discountPct;
            item.discountAmount = Math.round((item.total * discountPct / 100) * 100) / 100;
            item.netTotal = item.total - item.discountAmount;
            this.items.set([...current]);
        }
    }


    searchCustomer(event: any) {
        if (!event || !event.query) return;
        const localResults = this.customerService.searchLocal(event.query);
        this.customers = localResults.map(c => ({
            id: c.id,
            name: c.name,
            phone: c.phoneNumber || '',
            pricelistId: (c as any).pricelistId || null,
            pricelistDiscountPercentage: (c as any).pricelistDiscountPercentage || 0
        }));
    }

    loadInvoice(id: number) {
        this.saving = true;
        this.salesService.getById(id).subscribe({
            next: (invoice: SaleInvoice) => {
                this.invoiceDate = new Date(invoice.invoiceDate);
                const method: any = invoice.paymentMethod;
                this.paymentMethod = (method === 2 || method === 'Credit') ? 'Credit' : 'Cash';
                this.selectedPaymentMethod = (method === 2 || method === 'Credit') ? 2 : 1;

                if (invoice.customerId) {
                    this.selectedCustomer = { id: invoice.customerId, name: invoice.customerName };
                    this.isCashCustomer = false;
                } else {
                    this.isCashCustomer = true;
                }

                const mappedItems: InvoiceItem[] = (invoice.items || []).map(d => ({
                    medicineId: d.medicineId,
                    medicineName: d.medicineName || 'Unknown',
                    barcode: (d as any).barcode || (d as any).medicineBarcode || (d as any).defaultBarcode || '',
                    batchId: d.batchId || 0,
                    batchNumber: d.companyBatchNumber || '',
                    quantity: d.quantity,
                    salePrice: d.salePrice,
                    unitCost: 0,
                    total: d.quantity * d.salePrice,
                    netTotal: d.quantity * d.salePrice,
                    profit: 0,
                    availableQuantity: 9999,
                    saleUnitId: d.saleUnitId,
                    unitName: d.saleUnitId ? 'وحدة' : 'أساسية',
                    discountPercentage: (d as any).discountPercentage || 0,
                    discountAmount: (d as any).discountAmount || 0
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

    // 💊 FAST INSTANT SEARCH
    searchMedicine(event: any) {
        const query = event ? (event.query || '') : '';
        this.filteredMedicines = this.medicineService.searchLocal(query, 20);
    }


    updateItemQuantity(item: InvoiceItem, qty: number) {
        if (qty > item.availableQuantity) {
            this.messageService.add({ severity: 'info', summary: 'تنبيه مخزون', detail: `الكمية المطلوبة أكبر من المتوفر في الدفعة الحالية (${item.availableQuantity})، سيتم السحب من الدفعات الأخرى تلقائياً.` });
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

        // Refresh time to prevent stale timestamps if the page was left open for a while
        if (!this.isEditMode) {
            const now = new Date();
            if (this.invoiceDate.toDateString() === now.toDateString()) {
                this.invoiceDate = now;
            }
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

    private formatToLocalISO(date: Date | string | null | undefined): string {
        if (!date) {
            date = new Date();
        }
        const d = date instanceof Date ? date : new Date(date);
        if (isNaN(d.getTime())) {
            return new Date().toISOString();
        }
        const pad = (n: number) => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
    }

    private createInvoiceWithCustomer(approve: boolean, customerId: number | null, customerName: string) {
        const formattedDate = this.formatToLocalISO(this.invoiceDate);
        const payload: any = {
            id: this.isEditMode ? (this.invoiceId || 0) : 0,
            Id: this.isEditMode ? (this.invoiceId || 0) : 0,
            invoiceDate: formattedDate,
            saleInvoiceDate: formattedDate,
            paymentMethod: this.selectedPaymentMethod,
            customerId: customerId,
            customerName: customerName,
            details: this.items().map(item => ({
                medicineId: item.medicineId,
                batchId: item.batchId,
                quantity: item.quantity,
                salePrice: item.salePrice,
                saleUnitId: item.saleUnitId,
                discountPercentage: item.discountPercentage || 0
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
                            this.resetFormAfterSave();
                        },
                        error: (err) => this.handleError(err)
                    });
                } else {
                    this.saving = false;
                    this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم حفظ المسودة بنجاح' });
                    this.resetFormAfterSave();
                }
            },
            error: (err) => this.handleError(err)
        });
    }

    private resetFormAfterSave() {
        this.items.set([]);
        this.discount.set(0);
        this.customerPricelistId.set(null);
        this.customerPricelistDiscount.set(0);
        this.activePricelistItems.set([]);

        this.isCashCustomer = true;
        this.toggleCashCustomer();
        this.flyingCustomerName = '';
        this.selectedCustomer = null;

        this.inlineMedicine = null;
        this.inlineBatch = null;
        this.inlineQuantity = 1;
        this.availableBatches = [];
        this.inlineUnitOptions = [];
        this.inlineUnit = null;
        this.editingItemIndex = null;

        if (this.isEditMode) {
            this.router.navigate(['/sales/create']).then(() => {
                this.isEditMode = false;
                this.invoiceId = null;
            });
        }
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

    // 🎭 INLINE FORM & MODAL CONTROL METHODS
    focusQtyInput() {
        if (this.qtyInputEl) {
            const inputEl = this.qtyInputEl.el?.nativeElement?.querySelector('input');
            if (inputEl) {
                inputEl.focus();
                inputEl.select();
            }
        }
    }

    focusSearchInput() {
        if (this.medicineAutoComplete) {
            const inputEl = this.medicineAutoComplete.el?.nativeElement?.querySelector('input');
            if (inputEl) {
                inputEl.focus();
            }
        }
    }

    closeItemModal() {
        this.itemModalVisible = false;
        this.resetInlineForm();
        setTimeout(() => this.focusSearchInput(), 100);
    }

    confirmModalItem() {
        if (!this.inlineMedicine || !this.inlineBatch || !this.inlineQuantity) return;
        this.addItemInline();
        this.itemModalVisible = false;
        this.searchQueryText = '';
        setTimeout(() => this.focusSearchInput(), 100);
    }

    onMedicineSelectInline(medicine: Medicine) {
        this.inlineMedicine = medicine;
        this.selectedMedicineBarcode = medicine.defaultBarcode || medicine.barcode || null;
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
                    // Open modal automatically for smooth quantity confirmation
                    this.itemModalVisible = true;
                    setTimeout(() => this.focusQtyInput(), 100);
                } else {
                    // DO NOT open modal! Show warning toast notification
                    this.itemModalVisible = false;
                    this.resetInlineForm();
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'لا توجد دفعة متوفرة',
                        detail: `عفواً، لا توجد أي دفعة متوفرة بالمخزون لدواء (${medicine.name})`
                    });
                    setTimeout(() => this.focusSearchInput(), 100);
                }
            },
            error: () => {
                this.itemModalVisible = false;
                this.resetInlineForm();
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات الصنف' });
                setTimeout(() => this.focusSearchInput(), 100);
            }
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

        // Validation against stock
        let requestedBaseUnits = this.inlineQuantity * unitFactor;
        const totalAvailableUnits = this.availableBatches.reduce((sum, b) => sum + (b.remainingQuantity || 0), 0);

        if (this.availableBatches.length > 1 && requestedBaseUnits > totalAvailableUnits) {
            this.messageService.add({ severity: 'error', summary: 'رصيد غير كاف', detail: `إجمالي الكمية المتوفرة للصنف ${Math.floor(totalAvailableUnits / unitFactor)} وحدة فقط.` });
            return;
        }

        if (this.editingItemIndex !== null) {
            // Update existing item (Single Batch editing logic)
            const item = this.items()[this.editingItemIndex];

            // Price calculations for edit
            const baseSalePrice = this.inlineBatch.retailPrice > 0 ? this.inlineBatch.retailPrice : ((this.inlineUnit && this.inlineUnit.salePrice) ? (this.inlineUnit.salePrice / unitFactor) : 0);
            const salePrice = baseSalePrice * unitFactor;
            const baseUnitCost = this.inlineBatch.unitPurchasePrice || 0;
            const unitCost = baseUnitCost * unitFactor;

            const conflictingItemIndex = this.items().findIndex((i, idx) => i.batchId === this.inlineBatch!.id && idx !== this.editingItemIndex);

            if (conflictingItemIndex !== -1) {
                const conflictingItem = this.items()[conflictingItemIndex];
                this.updateItemQuantity(conflictingItem, conflictingItem.quantity + this.inlineQuantity);
                this.removeItem(this.editingItemIndex);
                this.messageService.add({ severity: 'success', summary: 'تم الدمج', detail: 'تم دمج الكمية مع الدفعة الموجودة' });
            } else {
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

                this.items.set([...this.items()]);
                this.messageService.add({ severity: 'success', summary: 'تم التعديل', detail: 'تم تعديل الصنف بنجاح' });
            }
        } else {
            // Add new item with FEFO Auto-Splitting
            let startIndex = this.availableBatches.findIndex(b => b.id === this.inlineBatch!.id);
            if (startIndex === -1) startIndex = 0; // Fallback to first if somehow missing

            let remainingUnitsToFulfill = requestedBaseUnits;
            let currentItems = [...this.items()];
            let addedCount = 0;

            for (let i = startIndex; i < this.availableBatches.length && remainingUnitsToFulfill > 0; i++) {
                const batch = this.availableBatches[i];
                if (batch.remainingQuantity <= 0) continue;

                const takeBaseUnits = Math.min(remainingUnitsToFulfill, batch.remainingQuantity);
                const takeUnitQty = takeBaseUnits / unitFactor;

                const baseSalePrice = batch.retailPrice > 0 ? batch.retailPrice : ((this.inlineUnit && this.inlineUnit.salePrice) ? (this.inlineUnit.salePrice / unitFactor) : 0);
                const salePrice = baseSalePrice * unitFactor;
                const baseUnitCost = batch.unitPurchasePrice || 0;
                const unitCost = baseUnitCost * unitFactor;

                const itemBarcode = this.selectedMedicineBarcode || this.inlineMedicine?.defaultBarcode || (this.inlineMedicine as any)?.barcode || (batch as any)?.barcode || '';

                const existingItemIndex = currentItems.findIndex(item => item.batchId === batch.id && item.saleUnitId === saleUnitId);

                if (existingItemIndex !== -1) {
                    const existing = currentItems[existingItemIndex];
                    existing.quantity += takeUnitQty;
                    existing.total = existing.quantity * existing.salePrice;
                    existing.profit = (existing.salePrice - existing.unitCost) * existing.quantity;
                    if (!existing.barcode) existing.barcode = itemBarcode;
                } else {
                    currentItems.push({
                        medicineId: this.inlineMedicine.id,
                        medicineName: `${this.inlineMedicine.name} (${unitName})`,
                        barcode: itemBarcode,
                        batchId: batch.id,
                        batchNumber: batch.companyBatchNumber,
                        quantity: takeUnitQty,
                        salePrice: salePrice,
                        unitCost: unitCost,
                        total: takeUnitQty * salePrice,
                        netTotal: takeUnitQty * salePrice,
                        profit: (salePrice - unitCost) * takeUnitQty,
                        expiryDate: batch.expiryDate ? new Date(batch.expiryDate) : undefined,
                        availableQuantity: Math.floor(batch.remainingQuantity / unitFactor),
                        saleUnitId: saleUnitId,
                        unitName: unitName,
                        discountPercentage: 0,
                        discountAmount: 0
                    });
                }

                addedCount++;
                remainingUnitsToFulfill -= takeBaseUnits;
            }

            if (remainingUnitsToFulfill > 0) {
                this.messageService.add({ severity: 'warn', summary: 'نقص في المخزون', detail: 'تم سحب الكمية المتوفرة ولم يتم توفية كامل طلبك' });
            } else if (addedCount > 1) {
                this.messageService.add({ severity: 'success', summary: 'سحب آلي متعدد', detail: `تم سحب الكمية وتقسيمها من ${addedCount} دفعات بناءً على FEFO (الأقرب انتهاءً).` });
            } else {
                this.messageService.add({ severity: 'success', summary: 'تمت الإضافة', detail: 'تم إضافة الصنف للفاتورة' });
            }

            this.items.set(currentItems);
            this.recalculateAllItems(); // Apply active pricelists immediately to new items
        }

        this.resetInlineForm();
    }

    resetInlineForm() {
        this.inlineMedicine = null;
        this.selectedMedicineBarcode = null;
        this.inlineBatch = null;
        this.inlineQuantity = 1;
        this.availableBatches = [];
        this.inlineUnitOptions = [];
        this.inlineUnit = null;
        this.editingItemIndex = null;
        this.searchQueryText = '';
    }

    editItemInline(item: InvoiceItem, index: number) {
        this.editingItemIndex = index;
        this.selectedMedicineBarcode = item.barcode || null;

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

        // Open modal for editing item
        this.itemModalVisible = true;
        setTimeout(() => this.focusQtyInput(), 100);
    }

    // 💰 DRAWER LOGIC
    openDrawerLedger() {
        this.drawerLedgerVisible = true;
        this.loadingDrawer = true;
        this.cdr.markForCheck(); // Trigger UI update for loading spinner

        // Fetch from backend (we added my-drawer-ledger endpoint)
        this.shiftService.getMyDrawerLedger().subscribe({
            next: (ledger) => {
                this.drawerLedger = ledger;
                this.loadingDrawer = false;
                this.cdr.markForCheck(); // Trigger UI update for loaded data
            },
            error: (err) => {
                this.loadingDrawer = false;
                this.drawerLedgerVisible = false;
                this.cdr.markForCheck(); // Trigger UI update for error
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'لم يتم العثور على درج (قد تكون الوردية مغلقة)' });
            }
        });
    }

    closeShiftFromDrawer(transferToSafe: boolean = true) {
        this.shiftService.transferIntent = transferToSafe;
        this.shiftService.requestShiftModal('close');
        this.drawerLedgerVisible = false;
    }
}
