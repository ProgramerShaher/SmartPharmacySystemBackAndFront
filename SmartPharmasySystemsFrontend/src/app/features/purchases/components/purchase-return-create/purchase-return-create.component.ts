import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PurchaseReturnService, CreatePurchaseReturnDto } from '../../services/purchase-return.service';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { PurchaseInvoice } from '../../../../core/models/purchase-invoice.interface';
import { PurchaseInvoiceDetail } from '../../../../core/models/purchase-invoice-detail.interface';
import { DocumentStatus } from '../../../../core/models/stock-movement.enums';

// PrimeNG
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CalendarModule } from 'primeng/calendar';
import { TableModule } from 'primeng/table';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface ExtendedDetail extends PurchaseInvoiceDetail {
    returnQty: number;
    maxReturnQty: number;
    batchStatus?: string; // e.g. 'Sold', 'Available'
    batchSoldQty?: number;
    batchRemainingQty?: number;
    loadingBatch?: boolean;
    errorBatch?: boolean;
}

@Component({
    selector: 'app-purchase-return-create',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        AutoCompleteModule,
        CalendarModule,
        TableModule,
        InputNumberModule,
        ToastModule,
        ConfirmDialogModule,
        TooltipModule,
        TagModule,
        DividerModule,
        DialogModule
    ],
    providers: [MessageService, ConfirmationService],
    templateUrl: './purchase-return-create.component.html',
    styleUrls: ['./purchase-return-create.component.scss']
})
export class PurchaseReturnCreateComponent implements OnInit, OnChanges {
    // 🪟 Modal Inputs / Outputs
    @Input() visible = false;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Input() invoiceId: number | null = null;
    @Output() onSaved = new EventEmitter<any>();

    isRouted = false;
    shortcutsHelpVisible = false;

    returnForm: FormGroup;
    selectedInvoice: PurchaseInvoice | null = null;
    filteredInvoices: PurchaseInvoice[] = [];
    details: ExtendedDetail[] = [];
    totalReturnAmount = 0;
    saving = false;

    constructor(
        private fb: FormBuilder,
        private purchaseReturnService: PurchaseReturnService,
        private purchaseInvoiceService: PurchaseInvoiceService,
        private inventoryService: InventoryService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.returnForm = this.fb.group({
            invoice: [null, Validators.required],
            returnDate: [new Date(), Validators.required],
            reason: ['', Validators.required]
        });
    }

    ngOnInit() {
        const isCreateRoute = this.router.url.includes('/purchases/returns/create');
        if (isCreateRoute) {
            this.isRouted = true;
            this.visible = true;
        }

        this.route.queryParams.subscribe(params => {
            const id = params['invoiceId'];
            if (id) {
                this.loadInvoiceById(Number(id));
            }
        });

        if (this.invoiceId) {
            this.loadInvoiceById(this.invoiceId);
        }
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['invoiceId'] && this.invoiceId) {
            this.loadInvoiceById(this.invoiceId);
        }
        if (changes['visible'] && this.visible) {
            setTimeout(() => {
                this.focusFirstInput();
            }, 250);
        }
    }

    focusFirstInput() {
        const searchInput = document.getElementById('purchaseReturnInvoiceSearch') as HTMLInputElement;
        if (searchInput) {
            searchInput.focus();
        }
    }

    // ⌨️ Hotkeys
    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        if (!this.visible) return;

        if (event.key === 'F1') {
            event.preventDefault();
            this.shortcutsHelpVisible = !this.shortcutsHelpVisible;
            return;
        }
        if (event.key === 'F2' || (event.ctrlKey && event.key === 'Enter')) {
            event.preventDefault();
            if (!this.saving && this.selectedInvoice && this.totalReturnAmount > 0) {
                this.saveReturn(true);
            }
            return;
        }
        if (event.key === 'F3') {
            event.preventDefault();
            if (!this.saving && this.selectedInvoice && this.totalReturnAmount > 0) {
                this.saveReturn(false);
            }
            return;
        }
        if (event.key === 'F4') {
            event.preventDefault();
            this.returnAllAvailable();
            return;
        }
        if (event.key === 'F5') {
            event.preventDefault();
            this.resetQuantities();
            return;
        }
        if (event.key === 'Escape') {
            if (this.shortcutsHelpVisible) {
                this.shortcutsHelpVisible = false;
                event.preventDefault();
                return;
            }
            event.preventDefault();
            this.close();
            return;
        }
    }

    loadInvoiceById(id: number) {
        this.purchaseInvoiceService.getById(id).subscribe({
            next: (fullInvoice) => {
                this.selectedInvoice = fullInvoice;
                this.returnForm.patchValue({ invoice: fullInvoice });
                this.initializeDetails(fullInvoice.items || []);
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تفاصيل الفاتورة' })
        });
    }

    searchInvoices(event: any) {
        const query: any = {
            search: event.query,
            status: DocumentStatus.Approved
        };
        this.purchaseInvoiceService.getAll(query).subscribe({
            next: (data) => this.filteredInvoices = data,
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل البحث' })
        });
    }

    onInvoiceSelect(event: any) {
        const invoice = event.value as PurchaseInvoice;
        if (!invoice) return;

        this.purchaseInvoiceService.getById(invoice.id).subscribe({
            next: (fullInvoice) => {
                this.selectedInvoice = fullInvoice;
                this.initializeDetails(fullInvoice.items || []);
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تفاصيل الفاتورة' })
        });
    }

    initializeDetails(items: PurchaseInvoiceDetail[]) {
        this.details = items.map(i => ({
            ...i,
            returnQty: 0,
            maxReturnQty: 0,
            loadingBatch: true,
            batchSoldQty: 0,
            batchRemainingQty: 0
        }));

        this.calculateTotal();

        const batchChecks = this.details.map(detail => {
            return this.inventoryService.getBatchById(detail.batchId).pipe(
                catchError(() => of(null))
            );
        });

        forkJoin(batchChecks).subscribe(results => {
            results.forEach((batch, index) => {
                const detail = this.details[index];
                detail.loadingBatch = false;

                if (batch) {
                    detail.batchRemainingQty = batch.remainingQuantity;
                    detail.batchSoldQty = batch.soldQuantity || 0;

                    if (detail.batchSoldQty > 0) {
                        detail.maxReturnQty = 0;
                        detail.batchStatus = 'Sold';
                    } else {
                        detail.maxReturnQty = Math.min(detail.quantity, batch.remainingQuantity);
                        detail.batchStatus = 'Available';
                    }
                } else {
                    detail.errorBatch = true;
                    detail.maxReturnQty = 0;
                }
            });

            setTimeout(() => {
                this.focusRowQuantity(0);
            }, 200);
        });
    }

    calculateTotal() {
        this.totalReturnAmount = this.details.reduce((sum, item) => sum + ((item.returnQty || 0) * item.purchasePrice), 0);
    }

    get totalReturnPieces(): number {
        return this.details.reduce((sum, item) => sum + (item.returnQty || 0), 0);
    }

    /**
     * ⚡ Return all available
     */
    returnAllAvailable() {
        this.details.forEach(detail => {
            if (detail.maxReturnQty > 0) {
                detail.returnQty = detail.maxReturnQty;
            }
        });
        this.calculateTotal();
        this.messageService.add({
            severity: 'info',
            summary: 'إرجاع المتاح',
            detail: 'تم تحديد كامل الكميات المتاحة القابلة للإرجاع'
        });
    }

    /**
     * 🧹 Reset quantities
     */
    resetQuantities() {
        this.details.forEach(detail => {
            detail.returnQty = 0;
        });
        this.calculateTotal();
    }

    /**
     * ⌨️ Row navigation
     */
    onQtyKeydown(index: number, event: KeyboardEvent) {
        if (event.key === 'ArrowDown' || event.key === 'Enter') {
            event.preventDefault();
            if (index < this.details.length - 1) {
                this.focusRowQuantity(index + 1);
            } else {
                const btn = document.getElementById('purchaseReturnApproveBtn');
                if (btn) btn.focus();
            }
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            if (index > 0) {
                this.focusRowQuantity(index - 1);
            } else {
                const reasonInput = document.getElementById('purchaseReturnReason') as HTMLInputElement;
                if (reasonInput) reasonInput.focus();
            }
        } else if (event.key === 'Tab' && !event.shiftKey) {
            if (index < this.details.length - 1) {
                event.preventDefault();
                this.focusRowQuantity(index + 1);
            }
        } else if (event.key === 'Tab' && event.shiftKey) {
            if (index > 0) {
                event.preventDefault();
                this.focusRowQuantity(index - 1);
            }
        }
    }

    focusRowQuantity(index: number) {
        setTimeout(() => {
            const el = document.getElementById(`p_return_qty_${index}`) as HTMLInputElement;
            if (el) {
                el.focus();
                el.select();
            }
        }, 50);
    }

    saveReturn(approve: boolean) {
        if (this.returnForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال البيانات المطلوبة (السبب والتاريخ)' });
            const rInput = document.getElementById('purchaseReturnReason');
            if (rInput) rInput.focus();
            return;
        }

        const itemsToReturn = this.details.filter(d => (d.returnQty || 0) > 0);
        if (itemsToReturn.length === 0) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى تحديد كميات للإرجاع لصنف واحد على الأقل' });
            return;
        }

        const dto: CreatePurchaseReturnDto = {
            purchaseInvoiceId: this.selectedInvoice!.id,
            supplierId: this.selectedInvoice!.supplierId,
            returnDate: this.returnForm.get('returnDate')?.value.toISOString(),
            reason: this.returnForm.get('reason')?.value,
            details: itemsToReturn.map(d => ({
                medicineId: d.medicineId,
                batchId: d.batchId,
                quantity: d.returnQty,
                purchasePrice: d.purchasePrice
            }))
        };

        this.saving = true;
        this.purchaseReturnService.create(dto).subscribe({
            next: (ret) => {
                if (approve) {
                    this.purchaseReturnService.approve(ret.id).subscribe({
                        next: () => {
                            this.saving = false;
                            this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم حفظ واعتماد المردود بنجاح' });
                            this.onSaved.emit(ret);
                            setTimeout(() => this.close(), 500);
                        },
                        error: (err) => {
                            this.saving = false;
                            this.messageService.add({ severity: 'error', summary: 'خطأ الاعتماد', detail: err.error?.message || 'تم الحفظ ولكن فشل الاعتماد' });
                        }
                    });
                } else {
                    this.saving = false;
                    this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم حفظ المردود كمسودة' });
                    this.onSaved.emit(ret);
                    setTimeout(() => this.close(), 500);
                }
            },
            error: (err) => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل حفظ المردود' });
            }
        });
    }

    close() {
        this.visible = false;
        this.visibleChange.emit(false);
        if (this.isRouted) {
            this.router.navigate(['/purchases/returns']);
        }
    }
}

