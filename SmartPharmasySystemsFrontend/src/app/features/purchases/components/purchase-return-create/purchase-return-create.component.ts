import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PurchaseReturnService, CreatePurchaseReturnDto } from '../../services/purchase-return.service';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { PurchaseInvoice } from '../../../../core/models/purchase-invoice.interface';
import { PurchaseInvoiceDetail } from '../../../../core/models/purchase-invoice-detail.interface';
import { PurchaseReturn } from '../../../../core/models/purchase-return.interface';
import { DocumentStatus } from '../../../../core/models/stock-movement.enums';

// PrimeNG
import { CardModule } from 'primeng/card';
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
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface ExtendedDetail extends PurchaseInvoiceDetail {
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
        CardModule,
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
        DividerModule
    ],
    providers: [MessageService, ConfirmationService],
    templateUrl: './purchase-return-create.component.html',
    styleUrls: ['./purchase-return-create.component.scss']
})
export class PurchaseReturnCreateComponent implements OnInit {
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
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.returnForm = this.fb.group({
            invoice: [null, Validators.required],
            returnDate: [new Date(), Validators.required],
            reason: ['', Validators.required]
        });
    }

    ngOnInit() { }

    goBack() {
        this.router.navigate(['/purchases/returns']);
    }

    searchInvoices(event: any) {
        // Filter ONLY approved invoices
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

        // Fetch Full Details
        this.purchaseInvoiceService.getById(invoice.id).subscribe({
            next: (fullInvoice) => {
                this.selectedInvoice = fullInvoice;
                this.initializeDetails(fullInvoice.items || []);
            },
            error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تفاصيل الفاتورة' })
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

        // Check Batch Status for each item
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
                    detail.batchSoldQty = batch.soldQuantity || 0; // Check your Batch Interface for exact property

                    // Strict Logic: Cannot return if ANY sold
                    if (detail.batchSoldQty > 0) {
                        detail.maxReturnQty = 0;
                        detail.batchStatus = 'Sold';
                    } else {
                        // Max return is min(Purchased, Remaining)
                        // Typically Remaining should be == Purchased if Sold==0
                        // But maybe Damage/Expiry reduced it.
                        detail.maxReturnQty = Math.min(detail.quantity, batch.remainingQuantity);
                        detail.batchStatus = 'Available';

                        if (batch.remainingQuantity < detail.quantity) {
                            // This implies stock loss (Damage/Expiry) if no sales.
                            // Still, we can only return what is left.
                        }
                    }
                } else {
                    detail.errorBatch = true;
                    detail.maxReturnQty = 0;
                }
            });
        });
    }

    calculateTotal() {
        this.totalReturnAmount = this.details.reduce((sum, item) => sum + (item.returnQty * item.purchasePrice), 0);
    }

    saveReturn(approve: boolean) {
        if (this.returnForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال البيانات المطلوبة' });
            return;
        }

        const itemsToReturn = this.details.filter(d => d.returnQty > 0);
        if (itemsToReturn.length === 0) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى تحديد كميات للإرجاع' });
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
                            this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم حفظ واعتماد المرتجع' });
                            this.goBack();
                        },
                        error: (err) => {
                            this.saving = false;
                            this.messageService.add({ severity: 'error', summary: 'خطأ الاعتماد', detail: err.error?.message || 'تم الحفظ ولكن فشل الاعتماد' });
                        }
                    });
                } else {
                    this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم حفظ المرتجع كمسودة' });
                    this.goBack();
                }
            },
            error: (err) => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحفظ' });
            }
        });
    }
}
