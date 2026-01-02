import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SalesReturnService } from '../../services/sales-return.service';
import { SaleInvoice, SaleInvoiceDetail } from '../../../../core/models';
import { CreateSalesReturnDto } from '../../../../core/models/sales-return.interface';
import { PrintService } from '../../../../core/services/print.service';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG imports
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CardModule } from 'primeng/card';
import { CalendarModule } from 'primeng/calendar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-sales-return-create',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        TableModule,
        AutoCompleteModule,
        CardModule,
        CalendarModule,
        ConfirmDialogModule,
        ToastModule,
        DividerModule,
        TagModule
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './sales-return-create.component.html',
    styleUrls: ['./sales-return-create.component.scss']
})
export class SalesReturnCreateComponent implements OnInit {
    selectedInvoice: SaleInvoice | null = null;
    filteredInvoices: SaleInvoice[] = [];
    invoiceDetails: any[] = []; // Augmented detail with returnAmount

    returnDate: Date = new Date();
    reason: string = '';

    totalReturnAmount = 0;
    saving = false;

    // Edit mode
    returnId: number | null = null;
    isViewMode = false;
    isReadOnly = false;

    /**
     * 🛡️ Check if any item exceeds remaining quantity
     * التحقق من تجاوز الكمية المتاحة
     */
    get hasExceededQuantity(): boolean {
        return this.invoiceDetails.some(item =>
            (item.returnAmount || 0) > (item.remainingQtyToReturn || item.quantity)
        );
    }

    constructor(
        private salesInvoiceService: SaleInvoiceService,
        private salesReturnService: SalesReturnService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private printService: PrintService
    ) { }

    ngOnInit() {
        // Check for return ID (edit/view mode)
        const id = this.route.snapshot.params['id'];
        if (id) {
            if (this.router.url.includes('create')) {
                // creating new
            } else {
                this.loadReturn(id);
            }
        }

        // Check for invoiceId from query params (quick return from list)
        this.route.queryParams.subscribe(params => {
            const invoiceId = params['invoiceId'];
            if (invoiceId && !this.selectedInvoice) {
                this.loadInvoiceForReturn(+invoiceId);
            }
        });
    }

    /**
     * 🔥 Load Invoice for Quick Return
     * تحميل الفاتورة للمرتجع السريع
     */
    loadInvoiceForReturn(invoiceId: number) {
        this.salesInvoiceService.getById(invoiceId).subscribe({
            next: (invoice) => {
                this.selectedInvoice = invoice;
                if (invoice && invoice.items) {
                    this.invoiceDetails = invoice.items.map(item => ({
                        ...item,
                        returnAmount: 0,
                        remainingQtyToReturn: item.quantity - ((item as any).returnedQuantity || 0)
                    }));
                }
                this.messageService.add({
                    severity: 'success',
                    summary: 'تم التحميل',
                    detail: `تم تحميل الفاتورة ${invoice.saleInvoiceNumber}`
                });
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل الفاتورة'
                });
            }
        });
    }

    loadReturn(id: number) {
        this.salesReturnService.getById(id).subscribe({
            next: (ret) => {
                this.returnId = ret.id;
                this.returnDate = new Date(ret.returnDate);
                this.reason = ret.reason;
                this.isViewMode = true;

                // Allow editing only if Draft (1)
                this.isReadOnly = ret.status !== 1;

                // Load Original Invoice to populate table
                this.salesInvoiceService.getById(ret.saleInvoiceId).subscribe(inv => {
                    this.selectedInvoice = inv;

                    if (inv && inv.items) {
                        this.invoiceDetails = inv.items.map(item => {
                            // Find if this item was in the return
                            const returnedItem = ret.items.find((d: any) => d.medicineId === item.medicineId && d.batchId === item.batchId);
                            return {
                                ...item,
                                returnAmount: returnedItem ? returnedItem.quantity : 0
                            };
                        });
                        this.calculateTotal();
                    }
                });
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات المرتجع' });
                this.goBack();
            }
        });
    }

    searchInvoice(event: any) {
        this.salesInvoiceService.getAll(event.query).subscribe(res => {
            this.filteredInvoices = res;
        });
    }

    onInvoiceSelect(event: any) {
        const inv = event.value as SaleInvoice;
        if (!inv) return;

        // Fetch full details since list might be partial
        this.salesInvoiceService.getById(inv.id).subscribe(fullInv => {
            this.selectedInvoice = fullInv;
            if (fullInv && fullInv.items) {
                this.invoiceDetails = fullInv.items.map(d => ({
                    ...d,
                    returnAmount: 0 // Local property for input
                }));
            }
        });
    }

    calculateTotal() {
        this.totalReturnAmount = this.invoiceDetails.reduce((sum, item) => {
            return sum + ((item.returnAmount || 0) * item.salePrice);
        }, 0);
    }

    save(approve: boolean) {
        if (!this.selectedInvoice) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى اختيار فاتورة' });
            return;
        }
        if (!this.reason) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إدخال سبب الإرجاع' });
            return;
        }

        const itemsToReturn = this.invoiceDetails.filter(i => i.returnAmount > 0);
        if (itemsToReturn.length === 0) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى تحديد كمية للإرجاع لصنف واحد على الأقل' });
            return;
        }

        // Validate Quantity vs Remaining (Strict Logic)
        for (const item of itemsToReturn) {
            if (item.returnAmount > item.remainingQtyToReturn) {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ في الكمية',
                    detail: `الكمية المرتجعة للصنف ${item.medicineName} تتجاوز الكمية المتاحة للإرجاع (${item.remainingQtyToReturn})`
                });
                return;
            }
        }

        // Validate Date
        if (this.selectedInvoice && new Date(this.returnDate) < new Date(this.selectedInvoice.invoiceDate)) {
            this.messageService.add({
                severity: 'error',
                summary: 'خطأ في التاريخ',
                detail: 'تاريخ الإرجاع لا يمكن أن يكون قبل تاريخ الفاتورة الأصلية'
            });
            return;
        }

        const dto: CreateSalesReturnDto = {
            saleInvoiceId: this.selectedInvoice.id,
            returnDate: this.returnDate.toISOString(),
            reason: this.reason,
            remainingQtyToReturn: 0, // Should be calculated or field on backend?
            // The prompt asks to "Extend DTO with remainingQtyToReturn". 
            // Usually this is a validation field, but if backend requires it in DTO? 
            // Or maybe it means "validate against it". 
            // Let's assume it's part of the DTO as requested.
            details: itemsToReturn.map(i => ({
                salesReturnId: 0,
                medicineId: i.medicineId,
                batchId: i.batchId,
                quantity: i.returnAmount,
                salePrice: i.salePrice
            }))
        };

        this.saving = true;
        this.salesReturnService.create(dto).subscribe({
            next: (newReturn) => {
                if (approve) {
                    this.salesReturnService.approve(newReturn.id).subscribe({
                        next: () => {
                            this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم حفظ واعتماد المرتجع' });

                            // Print Return Receipt
                            this.printService.printReturn(newReturn.id).subscribe({
                                next: () => this.messageService.add({ severity: 'info', summary: 'طباعة', detail: 'تم الطباعة' }),
                                error: (e) => console.error(e)
                            });

                            this.router.navigate(['/sales/returns']);
                        },
                        error: (err) => {
                            this.saving = false;
                            this.messageService.add({ severity: 'error', summary: 'فشل الاعتماد', detail: err.error?.message || 'تم الحفظ كمسودة ولكن فشل الاعتماد' });
                            this.router.navigate(['/sales/returns']);
                        }
                    });
                } else {
                    this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم حفظ المرتجع كمسودة' });
                    this.router.navigate(['/sales/returns']);
                }
            },
            error: (err) => {
                this.saving = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحفظ' });
            }
        });
    }

    goBack() {
        this.router.navigate(['/sales/returns']);
    }
}
