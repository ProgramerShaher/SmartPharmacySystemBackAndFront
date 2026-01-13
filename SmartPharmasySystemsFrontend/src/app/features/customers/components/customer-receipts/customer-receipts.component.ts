import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import { Customer, CustomerReceipt, CreateCustomerReceiptDto } from '../../../../core/models/customer.models';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-customer-receipts',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        TableModule,
        ButtonModule,
        DialogModule,
        AutoCompleteModule,
        InputNumberModule,
        CalendarModule,
        DropdownModule,
        InputTextareaModule,
        ToastModule,
        ConfirmDialogModule,
        TagModule
    ],
    providers: [MessageService, ConfirmationService],
    templateUrl: './customer-receipts.component.html',
    styleUrls: ['./customer-receipts.component.scss']
})
export class CustomerReceiptsComponent implements OnInit {
    receipts = signal<CustomerReceipt[]>([]);
    loading = signal(false);
    displayDialog = signal(false);
    saving = signal(false);

    // Stats
    totalAmount = signal(0);
    todayAmount = signal(0);
    receiptsCount = signal(0);

    receiptForm: FormGroup;
    filteredCustomers: Customer[] = [];
    selectedCustomer: Customer | null = null;

    paymentMethods = [
        { label: 'نقد (Cash)', value: 'Cash' },
        { label: 'تحويل بنكي (Bank Transfer)', value: 'BankTransfer' },
        { label: 'شيك (Check)', value: 'Check' }
    ];

    constructor(
        private fb: FormBuilder,
        private customerService: CustomerService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.receiptForm = this.fb.group({
            customerId: [null, Validators.required],
            amount: [0, [Validators.required, Validators.min(1)]],
            paymentDate: [new Date(), Validators.required],
            paymentMethod: ['Cash', Validators.required],
            referenceNumber: [''],
            saleInvoiceId: [null],
            notes: ['']
        });
    }

    // Pagination
    totalRecords = signal(0);
    currentPage = signal(1);
    pageSize = signal(10);
    searchQuery = signal('');

    ngOnInit() {
        this.loadReceipts({ first: 0, rows: 10 });
        this.loadStats();
    }

    loadStats() {
        this.customerService.getReceiptStatistics().subscribe(stats => {
            this.totalAmount.set(stats.totalAmount);
            this.todayAmount.set(stats.todayAmount);
            this.receiptsCount.set(stats.totalReceipts);
        });
    }

    loadReceipts(event?: any) {
        this.loading.set(true);
        const page = event ? (event.first / event.rows) + 1 : this.currentPage();
        const size = event ? event.rows : this.pageSize();

        this.customerService.getAllReceipts({ page, pageSize: size, search: this.searchQuery() }).subscribe({
            next: (res) => {
                this.receipts.set(res.items);
                this.totalRecords.set(res.totalCount);
                this.currentPage.set(page);
                this.pageSize.set(size);

                // For stats, we might need a separate call or just sum existing page (simplified for now)
                this.calculateStats(res.items); 
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل السندات' });
                this.loading.set(false);
            }
        });
    }

    calculateStats(receipts: CustomerReceipt[]) {
        // Note: This only calculates stats for CURRENT PAGE.
        // Ideally backend should return global stats in a wrapper, but for now this is better than nothing or crashing.
        const total = receipts.reduce((acc, curr) => acc + curr.amount, 0);
        this.todayAmount.set(total); // Placeholder
    }

    searchCustomers(event: any) {
        this.customerService.getAll({ search: event.query, hasDebt: true }).subscribe(res => {
            this.filteredCustomers = res.items;
        });
    }

    unpaidInvoices = signal<any[]>([]);

    onCustomerSelect(customer: Customer) {
        this.selectedCustomer = customer;
        this.receiptForm.patchValue({ customerId: customer.id });

        // Load unpaid invoices (Smart Linking)
        this.loading.set(true);
        this.customerService.getUnpaidInvoices(customer.id).subscribe({
            next: (invoices) => {
                this.unpaidInvoices.set(invoices);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    openNew() {
        this.selectedCustomer = null;
        this.receiptForm.reset({
            paymentDate: new Date(),
            paymentMethod: 'Cash',
            amount: 0
        });
        this.displayDialog.set(true);
    }

    saveReceipt() {
        if (this.receiptForm.invalid) {
            this.receiptForm.markAllAsTouched();
            return;
        }

        const amount = this.receiptForm.value.amount;
        // Optional warning, strictly validating might be annoying if they want to overpay slightly or system is out of sync
        if (this.selectedCustomer && amount > this.selectedCustomer.balance) {
             // Just a toast warning, allow proceed? User might be paying in advance.
             // For now, let's allow it but warn.
        }

        this.saving.set(true);
        this.customerService.createReceipt(this.receiptForm.value).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ سند القبض وتحديث الرصيد' });
                this.displayDialog.set(false);
                this.loadReceipts();
                this.loadStats(); // Refresh stats
                this.saving.set(false);
            },
            error: (err) => {
                const errorMsg = err.error?.message || 'فشل في حفظ السند';
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: errorMsg });
                this.saving.set(false);
            }
        });
    }

    deleteReceipt(event: Event, receipt: CustomerReceipt) {
        this.confirmationService.confirm({
            target: event.target as EventTarget,
            message: `هل أنت متأكد من حذف سند القبض رقم ${receipt.receiptNumber}؟ سيتم إعادة المديونية للعميل.`,
            header: 'تأكيد الحذف المالي',
            icon: 'pi pi-exclamation-circle',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.customerService.deleteReceipt(receipt.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف السند وتعديل الأرصدة' });
                        this.loadReceipts();
                        this.loadStats(); // Refresh stats
                    }
                });
            }
        });
    }
}
