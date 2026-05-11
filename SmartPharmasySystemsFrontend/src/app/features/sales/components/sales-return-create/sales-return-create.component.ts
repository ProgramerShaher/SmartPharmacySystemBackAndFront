import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SalesReturnService } from '../../services/sales-return.service';
import { SaleInvoice, SalesReturn, DocumentStatus } from '../../../../core/models';
import { MessageService } from 'primeng/api';
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
import { InputTextareaModule } from 'primeng/inputtextarea';
import { SearchIcon } from 'primeng/icons/search';

interface ReturnItem {
    id: number; // SaleInvoiceDetailId
    medicineName: string;
    batchNumber: string;
    originalQuantity: number;
    returnedQuantity: number;
    remainingQtyToReturn: number;
    returnQuantity: number;
    salePrice: number;
    totalReturnAmount: number;
}

@Component({
    selector: 'app-sales-return-create',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
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
        InputTextareaModule
    ],
    templateUrl: './sales-return-create.component.html',
    styleUrls: ['./sales-return-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [MessageService]
})
export class SalesReturnCreateComponent implements OnInit {
    // 🎯 Signals
    returnItems = signal<ReturnItem[]>([]);

    // 💰 Computed total
    totalReturnAmount = computed(() =>
        this.returnItems().reduce((sum, item) => sum + item.totalReturnAmount, 0)
    );

    // 📋 State
    selectedInvoice: SaleInvoice | null = null;
    filteredInvoices: SaleInvoice[] = [];
    returnDate = new Date();
    reason = '';
    saving = false;

    // 🔍 Search
    invoiceSearchQuery = '';

    constructor(
        private salesService: SaleInvoiceService,
        private returnsService: SalesReturnService,
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        // Check for invoiceId in query params (quick return from list)
        const invoiceId = this.route.snapshot.queryParams['invoiceId'];
        if (invoiceId) {
            this.loadInvoiceForReturn(+invoiceId);
        }
    }

    /**
     * 🔍 Search invoices
     */
    searchInvoice(event: any) {
        const query = (event?.query ?? '').toString().trim();

        // SaleInvoiceService.getAll expects an object (search/page/...) not a raw string
        this.salesService.getAll(query).subscribe({
            next: (invoices) => {
                // Only show approved invoices
                this.filteredInvoices = invoices.filter(
                    inv => this.isApprovedInvoice(inv)
                );
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل البحث عن الفواتير'
                });
            }
        });
    }

    private isApprovedInvoice(invoice: SaleInvoice): boolean {
        const status: any = (invoice as any)?.status;

        // Backend might return numeric enum (2) or string ("Approved")
        if (status === 'Approved') return true;

        const statusNum = Number(status);
        return statusNum === Number(DocumentStatus.Approved);
    }

    /**
     * 📋 Invoice selected
     */
    onInvoiceSelect(invoice: SaleInvoice) {
        this.invoiceSearchQuery = invoice?.saleInvoiceNumber || '';
        this.loadInvoiceForReturn(invoice.id);
    }

    /**
     * 📦 Load invoice for return
     */
    loadInvoiceForReturn(invoiceId: number) {
        this.salesService.getById(invoiceId).subscribe({
            next: (invoice) => {
                if (invoice.status !== DocumentStatus.Approved) {
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'تنبيه',
                        detail: 'يمكن إرجاع الفواتير المعتمدة فقط'
                    });
                    return;
                }

                this.selectedInvoice = invoice;
                this.invoiceSearchQuery = invoice?.saleInvoiceNumber || '';

                // Map items to return items
                const items: ReturnItem[] = invoice.items.map(item => {
                    // Use backend provided remainingQtyToReturn directly as per requirement
                    // If backend sends 0 or undefined, we fallback to 0 to prevent issues
                    const remainingQty = item.remainingQtyToReturn !== undefined ? item.remainingQtyToReturn : 0;

                    return {
                        id: item.id,
                        medicineName: item.medicineName || 'Unknown',
                        batchNumber: item.companyBatchNumber || '', // Correct property
                        originalQuantity: item.quantity,
                        returnedQuantity: item.quantity - remainingQty, // Infer returned qty if needed for display, or 0 if not tracking
                        remainingQtyToReturn: remainingQty,
                        returnQuantity: 0,
                        salePrice: item.salePrice,
                        totalReturnAmount: 0
                    };
                });

                this.returnItems.set(items);
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل بيانات الفاتورة'
                });
            }
        });
    }

    /**
     * 🔢 Calculate return amount for item
     */
    calculateReturnAmount(item: ReturnItem) {
        if (item.returnQuantity > item.remainingQtyToReturn) {
            this.messageService.add({
                severity: 'error',
                summary: 'خطأ',
                detail: `الكمية المتاحة للإرجاع: ${item.remainingQtyToReturn} فقط`
            });
            item.returnQuantity = item.remainingQtyToReturn;
        }

        item.totalReturnAmount = item.returnQuantity * item.salePrice;

        // Trigger signal update
        this.returnItems.set([...this.returnItems()]);
    }

    /**
     * ✅ Check if has exceeded quantity
     */
    get hasExceededQuantity(): boolean {
        return this.returnItems().some(item =>
            item.returnQuantity > item.remainingQtyToReturn
        );
    }

    /**
     * 💾 Save as draft
     */
    saveDraft() {
        this.saveReturn(false);
    }

    /**
     * ✅ Approve return
     */
    approveReturn() {
        this.saveReturn(true);
    }

    /**
     * 💾 Save return
     */
    private saveReturn(approve: boolean) {
        if (!this.selectedInvoice) {
            this.messageService.add({
                severity: 'warn',
                summary: 'تنبيه',
                detail: 'يجب اختيار فاتورة أولاً'
            });
            return;
        }

        const itemsToReturn = this.returnItems().filter(item => item.returnQuantity > 0);

        if (itemsToReturn.length === 0) {
            this.messageService.add({
                severity: 'warn',
                summary: 'تنبيه',
                detail: 'يجب إدخال كمية مرتجع لصنف واحد على الأقل'
            });
            return;
        }

        if (this.hasExceededQuantity) {
            this.messageService.add({
                severity: 'error',
                summary: 'خطأ',
                detail: 'بعض الأصناف تتجاوز الكمية المتاحة للإرجاع'
            });
            return;
        }

        if (!this.reason.trim()) {
            this.messageService.add({
                severity: 'warn',
                summary: 'تنبيه',
                detail: 'يجب إدخال سبب الإرجاع'
            });
            return;
        }

        this.saving = true;

        const payload: any = {
            saleInvoiceId: this.selectedInvoice.id,
            returnDate: this.returnDate.toISOString(),
            reason: this.reason,
            items: itemsToReturn.map(item => ({
                saleInvoiceDetailId: item.id,
                quantity: item.returnQuantity,
                returnPrice: item.salePrice
            }))
        };

        const action$ = this.returnsService.create(payload);

        action$.subscribe({
            next: (returnDoc) => {
                if (approve) {
                    this.returnsService.approve(returnDoc.id).subscribe({
                        next: () => {
                            this.messageService.add({
                                severity: 'success',
                                summary: 'نجاح',
                                detail: 'تم اعتماد المرتجع بنجاح'
                            });
                            this.router.navigate(['/sales/returns']);
                        },
                        error: (err) => this.handleError(err)
                    });
                } else {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجاح',
                        detail: 'تم حفظ المرتجع كمسودة'
                    });
                    this.router.navigate(['/sales/returns']);
                }
            },
            error: (err) => this.handleError(err)
        });
    }

    /**
     * ⚠️ Handle errors
     */
    private handleError(err: any) {
        this.saving = false;
        this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'حدث خطأ غير متوقع'
        });
    }

    /**
     * 🔙 Go back
     */
    goBack() {
        this.router.navigate(['/sales/returns']);
    }
}
