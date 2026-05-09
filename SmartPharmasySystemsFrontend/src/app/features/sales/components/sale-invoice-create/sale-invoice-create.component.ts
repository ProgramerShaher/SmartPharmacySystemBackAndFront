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
}

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

    // 💡 MODAL LIVE TOTAL (Instant Calculation) - Using getter for non-signal properties
    get modalLiveTotal(): number {
        if (this.selectedBatchForModal && this.modalQuantity) {
            return this.selectedBatchForModal.retailPrice * this.modalQuantity;
        }
        return 0;
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
    batchDialogVisible = false;
    selectedMedicine: Medicine | null = null;
    availableBatches: MedicineBatchResponseDto[] = [];

    // 🎭 MODAL STATE (New)
    addItemDialogVisible = false;
    selectedMedicineForModal: Medicine | null = null;
    selectedBatchForModal: MedicineBatchResponseDto | null = null;
    modalQuantity: number = 1;

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
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
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
        // Check if item already exists in cart with same batch
        const existingItem = this.items().find(i => i.batchId === data.batchId);

        if (existingItem) {
            this.updateItemQuantity(existingItem, existingItem.quantity + 1);
            this.messageService.add({ severity: 'success', summary: 'تحديث الكمية', detail: `تم زيادة كمية ${data.tradeName}` });
        } else {
            const newItem: InvoiceItem = {
                medicineId: data.medicineId,
                medicineName: data.tradeName,
                batchId: data.batchId,
                batchNumber: data.batchNumber,
                quantity: 1,
                salePrice: data.salePrice,
                unitCost: data.movingAverageCost, // Best estimate for profit if batch cost not clear
                total: data.salePrice,
                profit: data.salePrice - data.movingAverageCost,
                expiryDate: new Date(data.expiryDate),
                availableQuantity: data.availableQuantity
            };

            this.items.update(current => [...current, newItem]);
            this.messageService.add({ severity: 'success', summary: 'إضافة صنف', detail: `تم إضافة ${data.tradeName} للفاتورة` });
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
                    availableQuantity: 9999 // Fallback
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

    // 💊 FAST SEARCH & BATCH SELECTION
    searchMedicine(event: any) {
        this.medicineService.getAll({ search: event.query, pageSize: 20 }).subscribe({
            next: (res) => this.filteredMedicines = res.items || [],
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Search failed' })
        });
    }

    onMedicineSelect(medicine: Medicine) {
        this.selectedMedicine = medicine;
        this.medicineBatchService.getAvailableByMedicineId(medicine.id).subscribe({
            next: (batches) => {
                this.availableBatches = batches
                    .filter(b => b.remainingQuantity > 0 && (b.isSellable ?? true))
                    // Optional: Sort by expiry date (FEFO) - usually backend does this but safely ensure here
                    .sort((a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime());

                if (this.availableBatches.length > 0) {
                    this.batchDialogVisible = true;
                } else {
                    this.messageService.add({ severity: 'warn', summary: 'Out of Stock', detail: 'No batches available for this medicine' });
                }
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Could not load batches' })
        });
    }

    selectBatch(batch: MedicineBatchResponseDto) {
        if (!this.selectedMedicine) return;

        // Check if item already exists in cart with same batch
        const existingItem = this.items().find(i => i.batchId === batch.id);
        if (existingItem) {
            this.messageService.add({ severity: 'info', summary: 'Item Updated', detail: 'Incremented quantity' });
            this.updateItemQuantity(existingItem, existingItem.quantity + 1);
            this.batchDialogVisible = false;
            this.selectedMedicine = null;
            return;
        }

        const newItem: InvoiceItem = {
            medicineId: this.selectedMedicine.id,
            medicineName: this.selectedMedicine.name,
            batchId: batch.id,
            batchNumber: batch.companyBatchNumber,
            quantity: 1,
            salePrice: batch.retailPrice,
            unitCost: batch.unitPurchasePrice,
            total: batch.retailPrice,
            profit: (batch.unitPurchasePrice > 0) ? (batch.retailPrice - batch.unitPurchasePrice) * 1 : 0, // Initial Profit
            expiryDate: new Date(batch.expiryDate),
            availableQuantity: batch.remainingQuantity
        };

        this.items.update(current => [...current, newItem]);
        this.batchDialogVisible = false;
        this.selectedMedicine = null;
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

        this.saving = true;

        // If Flying Customer with a name - create customer first
        if (this.isCashCustomer && this.flyingCustomerName && this.flyingCustomerName.trim()) {
            this.customerService.create({
                name: this.flyingCustomerName.trim(),
                phoneNumber: '',
                address: '',
                notes: 'عميل طيار - تم إنشاؤه تلقائياً'
            }).subscribe({
                next: (newCustomer) => {
                    // Now create invoice with the new customer
                    this.createInvoiceWithCustomer(approve, newCustomer.id, newCustomer.name);
                },
                error: (err) => {
                    console.warn('Could not create customer, proceeding with name only:', err);
                    // Fallback: create invoice without customer ID
                    this.createInvoiceWithCustomer(approve, null, this.flyingCustomerName.trim());
                }
            });
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
                salePrice: item.salePrice
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

    // 🎭 MODAL METHODS
    openAddItemDialog() {
        this.addItemDialogVisible = true;
        this.selectedMedicineForModal = null;
        this.selectedBatchForModal = null;
        this.modalQuantity = 1;
        this.availableBatches = [];
    }

    closeAddItemDialog() {
        this.addItemDialogVisible = false;
        this.selectedMedicineForModal = null;
        this.selectedBatchForModal = null;
        this.modalQuantity = 1;
        this.availableBatches = [];
    }

    onMedicineSelectInModal(medicine: Medicine) {
        this.selectedMedicineForModal = medicine;
        this.selectedBatchForModal = null;
        this.modalQuantity = 1;

        // Load batches for selected medicine (FEFO order)
        this.medicineBatchService.getAvailableByMedicineId(medicine.id).subscribe({
            next: (batches) => {
                this.availableBatches = batches
                    // Double check filtering just in case, but backend should handle it
                    .filter(b => b.remainingQuantity > 0 && (b.isSellable ?? true))
                    .sort((a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime());

                // Auto-select first batch (FEFO)
                if (this.availableBatches.length > 0) {
                    this.selectedBatchForModal = this.availableBatches[0];
                } else {
                    this.messageService.add({ severity: 'warn', summary: 'نفاذ المخزون', detail: 'لا توجد دفعات متاحة لهذا الصنف' });
                }
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الدفعات' })
        });
    }

    selectBatchForModal(batch: MedicineBatchResponseDto) {
        this.selectedBatchForModal = batch;
        this.modalQuantity = 1; // Reset quantity
    }

    addItemFromModal() {
        if (!this.selectedMedicineForModal || !this.selectedBatchForModal || !this.modalQuantity) {
            return;
        }

        // Get price with fallback
        const salePrice = this.selectedBatchForModal.retailPrice || this.selectedBatchForModal.unitPurchasePrice || 0;
        const unitCost = this.selectedBatchForModal.unitPurchasePrice || 0;

        // Check if item already exists
        const existingItem = this.items().find(i => i.batchId === this.selectedBatchForModal!.id);
        if (existingItem) {
            this.updateItemQuantity(existingItem, existingItem.quantity + this.modalQuantity);
            this.messageService.add({ severity: 'success', summary: 'تم التحديث', detail: 'تم زيادة الكمية' });
        } else {
            const newItem: InvoiceItem = {
                medicineId: this.selectedMedicineForModal.id,
                medicineName: this.selectedMedicineForModal.name,
                batchId: this.selectedBatchForModal.id,
                batchNumber: this.selectedBatchForModal.companyBatchNumber,
                quantity: this.modalQuantity,
                salePrice: salePrice,
                unitCost: unitCost,
                total: this.modalQuantity * salePrice,
                profit: (salePrice - unitCost) * this.modalQuantity,
                expiryDate: new Date(this.selectedBatchForModal.expiryDate),
                availableQuantity: this.selectedBatchForModal.remainingQuantity
            };

            this.items.update(current => [...current, newItem]);
            this.messageService.add({ severity: 'success', summary: 'تمت الإضافة', detail: 'تم إضافة الصنف بنجاح' });
        }

        this.closeAddItemDialog();
    }

    // 💡 UPDATE MODAL TOTAL (For manual trigger if needed)
    updateModalTotal() {
        // The computed signal handles this automatically
        // This method is here for explicit calls if needed
    }

    // 🖊️ EDIT ITEM IN MODAL
    editingItemIndex: number | null = null;

    editItemInModal(item: InvoiceItem, index: number) {
        this.editingItemIndex = index;

        // Set the medicine
        this.selectedMedicineForModal = {
            id: item.medicineId,
            name: item.medicineName,
            // ... other props if needed for display
        } as any;

        // Set the batch (Hydrate from item)
        this.selectedBatchForModal = {
            id: item.batchId,
            companyBatchNumber: item.batchNumber,
            retailPrice: item.salePrice,
            unitPurchasePrice: item.unitCost,
            remainingQuantity: item.availableQuantity, // Use available not current
            expiryDate: item.expiryDate ? item.expiryDate.toISOString() : '',
            medicineId: item.medicineId,
            quantity: 0, // Not important for this context
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

        // Reload actual batches to allow switching?
        // Ideally yes, but for now we just load the current one into context

        // Set quantity
        this.modalQuantity = item.quantity;

        // Open dialog
        this.addItemDialogVisible = true;
    }
}
