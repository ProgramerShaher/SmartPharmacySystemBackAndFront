import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SalesReturnService } from '../../services/sales-return.service';
import { SaleInvoice, DocumentStatus } from '../../../../core/models';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';

export interface ReturnItem {
    id: number; // SaleInvoiceDetailId
    medicineId: number;
    batchId: number;
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
        CalendarModule,
        ToastModule,
        DividerModule,
        TagModule,
        TooltipModule,
        DialogModule
    ],
    templateUrl: './sales-return-create.component.html',
    styleUrls: ['./sales-return-create.component.scss'],
    providers: [MessageService]
})
export class SalesReturnCreateComponent implements OnInit, OnChanges {
    // 🪟 Modal Inputs / Outputs
    @Input() visible = false;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Input() invoiceId: number | null = null;
    @Output() onSaved = new EventEmitter<any>();

    isRouted = false;
    shortcutsHelpVisible = false;

    // 🎯 Signals
    returnItems = signal<ReturnItem[]>([]);

    // 💰 Computed total
    totalReturnAmount = computed(() =>
        this.returnItems().reduce((sum, item) => sum + item.totalReturnAmount, 0)
    );

    totalReturnQuantity = computed(() =>
        this.returnItems().reduce((sum, item) => sum + (item.returnQuantity || 0), 0)
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
        const qInvoiceId = this.route.snapshot.queryParams['invoiceId'];
        const isCreateRoute = this.router.url.includes('/sales/returns/create');

        if (isCreateRoute) {
            this.isRouted = true;
            this.visible = true;
        }

        if (qInvoiceId) {
            this.loadInvoiceForReturn(+qInvoiceId);
        } else if (this.invoiceId) {
            this.loadInvoiceForReturn(this.invoiceId);
        }
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['invoiceId'] && this.invoiceId) {
            this.loadInvoiceForReturn(this.invoiceId);
        }
        if (changes['visible'] && this.visible) {
            setTimeout(() => {
                this.focusFirstInput();
            }, 250);
        }
    }

    focusFirstInput() {
        const searchInput = document.getElementById('salesReturnInvoiceSearch') as HTMLInputElement;
        if (searchInput) {
            searchInput.focus();
        }
    }

    // ⌨️ Keyboard Hotkeys
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
            if (!this.saving && this.selectedInvoice && !this.hasExceededQuantity && this.totalReturnAmount() > 0) {
                this.approveReturn();
            }
            return;
        }
        if (event.key === 'F3') {
            event.preventDefault();
            if (!this.saving && this.selectedInvoice && this.totalReturnAmount() > 0) {
                this.saveDraft();
            }
            return;
        }
        if (event.key === 'F4') {
            event.preventDefault();
            this.returnAllItems();
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

    /**
     * 🔍 Search invoices
     */
    searchInvoice(event: any) {
        const query = (event?.query ?? '').toString().trim();

        this.salesService.getAll(query).subscribe({
            next: (invoices) => {
                this.filteredInvoices = invoices.filter(
                    inv => this.isApprovedInvoice(inv)
                );
            },
            error: () => {
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
        if (status === 'Approved') return true;
        const statusNum = Number(status);
        return statusNum === Number(DocumentStatus.Approved);
    }

    /**
     * 📋 Invoice selected
     */
    onInvoiceSelect(invoice: SaleInvoice) {
        if (!invoice) return;
        this.invoiceSearchQuery = invoice.saleInvoiceNumber || '';
        this.loadInvoiceForReturn(invoice.id);
    }

    /**
     * 📦 Load invoice for return
     */
    loadInvoiceForReturn(invoiceId: number) {
        this.salesService.getById(invoiceId).subscribe({
            next: (invoice) => {
                if (!this.isApprovedInvoice(invoice)) {
                    this.messageService.add({
                        severity: 'warn',
                        summary: 'تنبيه',
                        detail: 'يمكن إرجاع الفواتير المعتمدة فقط'
                    });
                    return;
                }

                this.selectedInvoice = invoice;
                this.invoiceSearchQuery = invoice?.saleInvoiceNumber || '';

                const items: ReturnItem[] = (invoice.items || []).map(item => {
                    const remainingQty = item.remainingQtyToReturn !== undefined ? item.remainingQtyToReturn : item.quantity;
                    return {
                        id: item.id,
                        medicineId: item.medicineId,
                        batchId: item.batchId,
                        medicineName: item.medicineName || 'Unknown',
                        batchNumber: item.companyBatchNumber || '',
                        originalQuantity: item.quantity,
                        returnedQuantity: item.quantity - remainingQty,
                        remainingQtyToReturn: remainingQty,
                        returnQuantity: 0,
                        salePrice: item.salePrice,
                        totalReturnAmount: 0
                    };
                });

                this.returnItems.set(items);

                setTimeout(() => {
                    this.focusRowQuantity(0);
                }, 200);
            },
            error: () => {
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

        item.totalReturnAmount = (item.returnQuantity || 0) * item.salePrice;
        this.returnItems.set([...this.returnItems()]);
    }

    /**
     * ⚡ Return all items
     */
    returnAllItems() {
        const updated = this.returnItems().map(item => {
            const qty = item.remainingQtyToReturn;
            return {
                ...item,
                returnQuantity: qty,
                totalReturnAmount: qty * item.salePrice
            };
        });
        this.returnItems.set(updated);
        this.messageService.add({
            severity: 'info',
            summary: 'إرجاع الكل',
            detail: 'تم تحديد كامل الكميات المتاحة للإرجاع'
        });
    }

    /**
     * 🧹 Reset quantities
     */
    resetQuantities() {
        const updated = this.returnItems().map(item => ({
            ...item,
            returnQuantity: 0,
            totalReturnAmount: 0
        }));
        this.returnItems.set(updated);
    }

    /**
     * ⌨️ Keyboard Navigation between Table Rows
     */
    onQtyKeydown(index: number, event: KeyboardEvent) {
        if (event.key === 'ArrowDown' || event.key === 'Enter') {
            event.preventDefault();
            if (index < this.returnItems().length - 1) {
                this.focusRowQuantity(index + 1);
            } else {
                const btn = document.getElementById('salesReturnApproveBtn');
                if (btn) btn.focus();
            }
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            if (index > 0) {
                this.focusRowQuantity(index - 1);
            } else {
                const reasonInput = document.getElementById('salesReturnReason') as HTMLInputElement;
                if (reasonInput) reasonInput.focus();
            }
        } else if (event.key === 'Tab' && !event.shiftKey) {
            if (index < this.returnItems().length - 1) {
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
            const el = document.getElementById(`return_qty_${index}`) as HTMLInputElement;
            if (el) {
                el.focus();
                el.select();
            }
        }, 50);
    }

    get hasExceededQuantity(): boolean {
        return this.returnItems().some(item =>
            item.returnQuantity > item.remainingQtyToReturn
        );
    }

    saveDraft() {
        this.saveReturn(false);
    }

    approveReturn() {
        this.saveReturn(true);
    }

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
            const rInput = document.getElementById('salesReturnReason');
            if (rInput) rInput.focus();
            return;
        }

        this.saving = true;

        const payload: any = {
            saleInvoiceId: this.selectedInvoice.id,
            returnDate: this.returnDate.toISOString(),
            reason: this.reason,
            details: itemsToReturn.map(item => ({
                salesReturnId: 0,
                medicineId: item.medicineId,
                batchId: item.batchId,
                quantity: item.returnQuantity,
                salePrice: item.salePrice
            }))
        };

        this.returnsService.create(payload).subscribe({
            next: (returnDoc) => {
                if (approve) {
                    this.returnsService.approve(returnDoc.id).subscribe({
                        next: () => {
                            this.saving = false;
                            this.messageService.add({
                                severity: 'success',
                                summary: 'نجاح',
                                detail: 'تم اعتماد المرتجع بنجاح'
                            });
                            this.onSaved.emit(returnDoc);
                            setTimeout(() => this.close(), 500);
                        },
                        error: (err) => this.handleError(err)
                    });
                } else {
                    this.saving = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجاح',
                        detail: 'تم حفظ المرتجع كمسودة'
                    });
                    this.onSaved.emit(returnDoc);
                    setTimeout(() => this.close(), 500);
                }
            },
            error: (err) => this.handleError(err)
        });
    }

    private handleError(err: any) {
        this.saving = false;
        this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'حدث خطأ غير متوقع'
        });
    }

    close() {
        this.visible = false;
        this.visibleChange.emit(false);
        if (this.isRouted) {
            this.router.navigate(['/sales/returns']);
        }
    }
}
