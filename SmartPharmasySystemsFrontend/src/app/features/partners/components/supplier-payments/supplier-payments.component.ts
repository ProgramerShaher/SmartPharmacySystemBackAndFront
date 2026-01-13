import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SupplierService } from '../../services/supplier.service';
import { Supplier, SupplierPayment, CreateSupplierPaymentDto } from '../../../../core/models/supplier.models';
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
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
    selector: 'app-supplier-payments',
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
        TagModule,
        ConfirmDialogModule
    ],
    providers: [MessageService, ConfirmationService],
    templateUrl: './supplier-payments.component.html',
    styleUrls: ['./supplier-payments.component.scss']
})
export class SupplierPaymentsComponent implements OnInit {
    payments = signal<SupplierPayment[]>([]);
    loading = signal(false);
    displayDialog = signal(false);
    saving = signal(false);

    paymentForm: FormGroup;
    filteredSuppliers: Supplier[] = [];
    selectedSupplier: Supplier | null = null;
    vaultBalance = 0;

    constructor(
        private fb: FormBuilder,
        private supplierService: SupplierService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private route: ActivatedRoute
    ) {
        this.paymentForm = this.fb.group({
            supplierId: [null, Validators.required],
            amount: [0, [Validators.required, Validators.min(1)]],
            paymentDate: [new Date(), Validators.required],
            referenceNo: [''],
            notes: ['']
        });
    }



    ngOnInit() {
        this.loadPayments();
        this.loadVaultBalance();

        // Auto-open if supplierId present
        const supplierId = this.route.snapshot.queryParamMap.get('supplierId');
        if (supplierId) {
            this.supplierService.getById(+supplierId).subscribe(supplier => {
                if (supplier) {
                    this.openNew();
                    this.onSupplierSelect({ value: supplier });
                    this.filteredSuppliers = [supplier]; // Pre-fill autocomplete suggestions key
                    this.paymentForm.patchValue({ supplierId: supplier }); // If autocomplete binds to object
                }
            });
        }
    }

    loadVaultBalance() {
        // Mocked or fetched from a real FinancialService
        this.supplierService.checkVaultBalance(999999).subscribe({
            // Assuming this helper gives us info, but let's fetch actual balance if possible
            // For now, let's assume we have it.
        });
    }

    loadPayments() {
        this.loading.set(true);
        console.log('DEBUG: Attempting to load payments...');
        this.supplierService.getPayments().subscribe({
            next: (res) => {
                console.log('DEBUG: Payments loaded successfully', res);
                this.payments.set(res);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('DEBUG: Payment Load Error', err);
                const status = err.status;
                const msg = err.error?.message || err.message;

                this.messageService.add({
                    severity: 'error',
                    summary: `خطأ في التحميل (${status})`,
                    detail: `فشل الاتصال بالخادم: ${msg}`,
                    life: 5000
                });
                this.loading.set(false);
            }
        });
    }

    // Temporary Debug Method for User
    debugApi() {
        console.log('Starting Debug Check...');
        // 1. Check Supplier Endpoint
        this.supplierService.getPayments().subscribe({
            next: () => this.messageService.add({ severity: 'success', summary: 'API OK', detail: 'GET /api/SupplierPayments works!' }),
            error: (e) => this.messageService.add({ severity: 'error', summary: 'API FAIL', detail: `GET /api/SupplierPayments Status: ${e.status}` })
        });
    }

    searchSuppliers(event: any) {
        this.supplierService.getAll({ search: event.query, pageSize: 20 }).subscribe(res => {
            this.filteredSuppliers = res.items;
        });
    }

    // Smart Payment State
    unpaidInvoices = signal<any[]>([]);
    selectedInvoices: any[] = [];

    // ... existing constructor ...



    loadUnpaidInvoices(supplierId: number) {
        this.supplierService.getUnpaidInvoices(supplierId).subscribe({
            next: (invs) => this.unpaidInvoices.set(invs),
            error: () => this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا توجد فواتير معلقة أو فشل في الجلب' })
        });
    }

    onInvoiceSelectionChange(selection: any[]) {
        this.selectedInvoices = selection;
        const total = selection.reduce((sum, inv) => sum + (inv.totalAmount || 0), 0);
        this.paymentForm.patchValue({ amount: total });
    }

    openNew() {
        this.selectedSupplier = null;
        this.unpaidInvoices.set([]);
        this.selectedInvoices = [];
        this.paymentForm.reset({
            paymentDate: new Date(),
            amount: 0
        });
        this.displayDialog.set(true);
    }

    savePayment() {
        if (this.paymentForm.invalid) {
            this.paymentForm.markAllAsTouched();
            return;
        }

        const amount = this.paymentForm.value.amount;

        // Check Vault Balance (Real-time Business Logic)
        // Resilience: If check fails (e.g. 404), we proceed and let Backend enforce rules.
        this.supplierService.checkVaultBalance(amount).subscribe({
            next: (hasBalance) => {
                if (!hasBalance) {
                    this.messageService.add({
                        severity: 'error',
                        summary: 'رصيد غير كافٍ',
                        detail: 'رصيد الخزينة لا يسمح بإجراء هذا الصرف حالياً.'
                    });
                    return;
                }
                this.executeSave(amount);
            },
            error: (err) => {
                console.warn('Vault balance check failed, proceeding to backend validation', err);
                // Fallback: Proceed to save and let backend handle validation
                this.executeSave(amount);
            }
        });
    }

    private executeSave(amount: number) {
        this.saving.set(true);

        // Prepare Notes with selected invoices if any
        let finalNotes = this.paymentForm.value.notes || '';
        if (this.selectedInvoices.length > 0) {
            const invRefs = this.selectedInvoices.map(i => `#${i.purchaseInvoiceNumber || i.id}`).join(', ');
            finalNotes += ` [سداد فواتير: ${invRefs}]`;
        }

        const dto: CreateSupplierPaymentDto = {
            supplierId: this.paymentForm.value.supplierId.id, // Extract ID from object
            amount: amount,
            paymentDate: (this.paymentForm.value.paymentDate as Date).toISOString(),
            referenceNo: this.paymentForm.value.referenceNo,
            notes: finalNotes
        };

        this.supplierService.createPayment(dto).subscribe({
            next: (res) => {
                this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم إنشاء سند الصرف بنجاح' });
                this.displayDialog.set(false);
                this.loadPayments();
                this.saving.set(false);

                // Offer Print Immediately
                this.confirmationService.confirm({
                    header: 'تمت العملية بنجاح',
                    message: 'تم حفظ السند. هل تريد الطباعة الآن؟',
                    icon: 'pi pi-print',
                    acceptLabel: 'طباعة السند',
                    rejectLabel: 'إغلاق',
                    acceptButtonStyleClass: 'p-button-primary',
                    accept: () => {
                        this.printPayment(res); // Pass the created payment object
                    }
                });
            },
            error: (err) => {
                const msg = err.error?.message || 'فشل الحفظ';
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: msg });
                this.saving.set(false);
            }
        });
    }

    printPayment(payment: any) {
        // In a real app, this would open a print window or PDF
        this.messageService.add({ severity: 'info', summary: 'طباعة', detail: `جاري تحضير الملف للطباعة... (Simulated)` });
        console.log('Printing Voucher:', payment);
    }

    onSupplierSelect(event: any) {
        this.selectedSupplier = event.value;
        const supplierId = this.selectedSupplier?.id;

        // Update Form
        // We keep the detailed object in the form for autocomplete display, but access .id when saving
        this.paymentForm.patchValue({ supplierId: this.selectedSupplier });

        // Reset Smart Logic (but NOT amount if user already typed? Requirement says 'Link dynamic'. Probably should reset for new supplier)
        this.unpaidInvoices.set([]);
        this.selectedInvoices = [];

        // Rule: Show Balance Immediately
        // Rule: Fetch Unpaid Invoices Immediately
        if (supplierId) {
            this.loadUnpaidInvoices(supplierId);
        }
    }

    cancelPayment(event: Event, payment: SupplierPayment) {
        this.confirmationService.confirm({
            target: event.target as EventTarget,
            message: `هل أنت متأكد من إلغاء سند صرف رقم ${payment.id}؟ سيتم إعادة المبلغ للخزينة وزيادة مديونية المورد.`,
            header: 'تأكيد الإلغاء المالي',
            icon: 'pi pi-exclamation-circle',
            acceptLabel: 'نعم، إلغاء السند',
            rejectLabel: 'تراجع',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.supplierService.cancelPayment(payment.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الإلغاء', detail: 'تم إلغاء السند وتعديل الأرصدة' });
                        this.loadPayments();
                    }
                });
            }
        });
    }

    getRandomColor(name: string): string {
        if (!name) return 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)';
        const colors = [
            'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
            'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
            'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
            'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
            'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
            'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)'
        ];
        const index = name.charCodeAt(0) % colors.length;
        return colors[index];
    }
}
