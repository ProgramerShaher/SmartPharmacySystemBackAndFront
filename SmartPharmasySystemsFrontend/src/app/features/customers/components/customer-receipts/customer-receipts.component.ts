import { Component, OnInit, signal, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import { Customer, CustomerReceipt, CreateCustomerReceiptDto } from '../../../../core/models/customer.models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SettingsService } from '../../../../core/services/settings.service';
import { PharmacySettings } from '../../../../core/models/settings/pharmacy-settings.interface';
import { environment } from '../../../../../environments/environment';

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
    styleUrls: ['../../../partners/components/supplier-payments/supplier-payments.component.scss']
})
export class CustomerReceiptsComponent implements OnInit {
    @Input() dialogMode = false;
    @Input() autoOpenCreate = false;
    @Input() presetCustomerId: number | null = null;
    @Output() closed = new EventEmitter<void>();

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
    selectedCustomerObj!: Customer | null;

    pharmacySettings = signal<PharmacySettings>({
        id: 0,
        pharmacyName: 'صيدلية الشفاء العالمية',
        taxNumber: '300123456700003',
        phoneNumber: '011234567',
        address: 'الرياض، المملكة العربية السعودية',
        baseCurrency: 'ر.ي'
    });
    readonly serverUrl = environment.apiUrl.replace('/api', '');

    paymentMethods = [
        { label: 'نقد (Cash)', value: 'Cash' },
        { label: 'تحويل بنكي (Bank Transfer)', value: 'BankTransfer' },
        { label: 'شيك (Check)', value: 'Check' }
    ];

    constructor(
        private fb: FormBuilder,
        private customerService: CustomerService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private settingsService: SettingsService
    ) {
        this.receiptForm = this.fb.group({
            customerId: [null, Validators.required],
            amount: [0, [Validators.required, Validators.min(1)]],
            receiptDate: [new Date(), Validators.required],
            paymentMethod: ['Cash', Validators.required],
            referenceNo: [''],
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
        this.settingsService.getSettings().subscribe({
            next: (settings) => {
                if (settings) {
                    this.pharmacySettings.set(settings);
                }
            }
        });

        if (this.presetCustomerId) {
            this.customerService.getById(this.presetCustomerId).subscribe(c => {
                this.filteredCustomers = [c];
                this.onCustomerSelect(c);
            });
        }
        if (this.autoOpenCreate) {
            setTimeout(() => this.openNew(), 100);
        }
    }

    closeModal() {
        this.displayDialog.set(false);
        this.closed.emit();
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

    onSearchChange(val: string) {
        this.searchQuery.set(val);
        this.loadReceipts({ first: 0, rows: this.pageSize() });
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
    filteredInvoices: any[] = [];
    selectedInvoiceId: number | null = null;
    loadingInvoices = signal(false);

    onCustomerSelect(event: any) {
        const customer: Customer = event.value || event;
        this.selectedCustomer = customer;
        this.selectedCustomerObj = customer;
        this.receiptForm.patchValue({ customerId: customer.id });

        // Reset invoice selection state
        this.unpaidInvoices.set([]);
        this.filteredInvoices = [];
        this.selectedInvoiceId = null;
        this.receiptForm.patchValue({ saleInvoiceId: null });

        // Load unpaid invoices (Smart Linking)
        this.loadingInvoices.set(true);
        this.customerService.getUnpaidInvoices(customer.id).subscribe({
            next: (invoices) => {
                const mapped = invoices.map(inv => ({
                    ...inv,
                    remainingAmount: inv.totalAmount - (inv.paidAmount || 0),
                    displayText: `فاتورة #${inv.saleInvoiceNumber || inv.id} — المتبقي: ${(inv.totalAmount - (inv.paidAmount || 0)).toLocaleString()} ر.ي`
                })).filter(inv => (inv.remainingAmount || 0) > 0)
                    .sort((a, b) => +new Date(a.invoiceDate) - +new Date(b.invoiceDate));

                this.unpaidInvoices.set(mapped);
                this.filteredInvoices = [...mapped];
                this.loadingInvoices.set(false);
            },
            error: () => {
                this.unpaidInvoices.set([]);
                this.filteredInvoices = [];
                this.loadingInvoices.set(false);
            }
        });
    }

    searchInvoices(event: any) {
        const query = (event.query || '').toLowerCase().trim();
        if (!query) {
            this.filteredInvoices = [...this.unpaidInvoices()];
        } else {
            this.filteredInvoices = this.unpaidInvoices().filter(inv =>
                inv.displayText?.toLowerCase().includes(query) ||
                (inv.saleInvoiceNumber || '').toLowerCase().includes(query) ||
                String(inv.id).includes(query)
            );
        }
    }

    onInvoiceSelect(event: any) {
        const inv = event.value;
        this.selectedInvoiceId = inv.id;
        this.receiptForm.patchValue({
            amount: inv.remainingAmount,
            saleInvoiceId: inv.id
        });
    }

    openNew() {
        this.selectedCustomer = null;
        this.selectedCustomerObj = null;
        this.receiptForm.reset({
            receiptDate: new Date(),
            paymentMethod: 'Cash',
            amount: 0
        });
        this.displayDialog.set(true);
    }

    saveReceipt() {
        if (this.receiptForm.invalid || !this.receiptForm.value.customerId?.id) {
            this.receiptForm.markAllAsTouched();
            return;
        }

        const amount = Number(this.receiptForm.value.amount || 0);

        this.saving.set(true);

        const formVal = this.receiptForm.value;
        const customerId = formVal.customerId.id;
        
        const invoiceObj = formVal.saleInvoiceId;
        const invoiceId = (typeof invoiceObj === 'object' && invoiceObj !== null) ? invoiceObj.id : invoiceObj;
        
        let refNo = formVal.referenceNo;
        if (!refNo && invoiceObj && typeof invoiceObj === 'object') {
            refNo = invoiceObj.saleInvoiceNumber || String(invoiceObj.id);
        }

        const dto: CreateCustomerReceiptDto = {
            customerId,
            amount,
            receiptDate: (formVal.receiptDate as Date).toISOString(),
            paymentMethod: formVal.paymentMethod,
            referenceNo: refNo,
            saleInvoiceId: invoiceId,
            notes: formVal.notes || 'سند قبض'
        };

        this.customerService.createReceipt(dto).subscribe({
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
            message: `هل أنت متأكد من حذف سند القبض رقم ${receipt.id}؟ سيتم إعادة المديونية للعميل.`,
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

    convertNumberToWords(num: number): string {
        if (num === 0) return 'صفر';
        const ones = ['', 'واحد', 'اثنان', 'ثلاثة', 'أربعة', 'خمسة', 'ستة', 'سبعة', 'ثمانية', 'تسعة', 'عشرة', 'أحد عشر', 'اثنا عشر', 'ثلاثة عشر', 'أربعة عشر', 'خمسة عشر', 'ستة عشر', 'سبعة عشر', 'ثمانية عشر', 'تسعة عشر'];
        const tens = ['', 'عشرة', 'عشرون', 'ثلاثون', 'أربعون', 'خمسون', 'ستون', 'سبعون', 'ثمانون', 'تسعون'];
        const hundreds = ['', 'مائة', 'مائتان', 'ثلاثمائة', 'أربعمائة', 'خمسمائة', 'ستمائة', 'سبعمائة', 'ثمانمائة', 'تسعمائة'];

        const parse = (n: number): string => {
            if (n < 20) return ones[n];
            if (n < 100) {
                const ten = Math.floor(n / 10);
                const unit = n % 10;
                return (unit !== 0 ? ones[unit] + ' و' : '') + tens[ten];
            }
            if (n < 1000) {
                const hundred = Math.floor(n / 100);
                const rem = n % 100;
                return hundreds[hundred] + (rem !== 0 ? ' و' + parse(rem) : '');
            }
            if (n < 2000) {
                return 'ألف' + (n % 1000 !== 0 ? ' و' + parse(n % 1000) : '');
            }
            if (n < 3000) {
                return 'ألفان' + (n % 1000 !== 0 ? ' و' + parse(n % 1000) : '');
            }
            if (n < 10000) {
                const thousand = Math.floor(n / 1000);
                const rem = n % 1000;
                return ones[thousand] + ' آلاف' + (rem !== 0 ? ' و' + parse(rem) : '');
            }
            if (n < 1000000) {
                const thousand = Math.floor(n / 1000);
                const rem = n % 1000;
                return parse(thousand) + ' ألف' + (rem !== 0 ? ' و' + parse(rem) : '');
            }
            const million = Math.floor(n / 1000000);
            const rem = n % 1000000;
            return parse(million) + ' مليون' + (rem !== 0 ? ' و' + parse(rem) : '');
        };

        return parse(Math.floor(num));
    }

    printReceipt(receipt: CustomerReceipt) {
        const dateObj = new Date(receipt.receiptDate);
        const formattedDate = dateObj.toLocaleDateString('ar-YE');
        const formattedTime = dateObj.toLocaleTimeString('ar-YE', { hour: '2-digit', minute: '2-digit' });

        let translatedMethod = 'نقداً (كاش)';
        if (receipt.paymentMethod === 'BankTransfer') translatedMethod = 'تحويل بنكي';
        else if (receipt.paymentMethod === 'Check') translatedMethod = 'شيك بنكي';

        const amountWords = 'فقط ' + this.convertNumberToWords(receipt.amount) + ' ريال يمني لا غير.';

        const logoMarkup = this.pharmacySettings().logoUrl
            ? `<div class="pharmacy-logo"><img src="${this.serverUrl}${this.pharmacySettings().logoUrl}" alt="${this.pharmacySettings().pharmacyName}"></div>`
            : '';

        const printContents = `
            <div class="voucher-container receipt-voucher" dir="rtl">
                <!-- Header -->
                <div class="voucher-header">
                    <div class="pharmacy-info">
                        ${logoMarkup}
                        <h2>${this.pharmacySettings().pharmacyName}</h2>
                        <p>الرقم الضريبي: ${this.pharmacySettings().taxNumber || '---'}</p>
                        <p>الهاتف: ${this.pharmacySettings().phoneNumber || this.pharmacySettings().mobileNumber || '---'}</p>
                        <p>العنوان: ${this.pharmacySettings().address || '---'}</p>
                    </div>
                    <div class="voucher-title">
                        <h1>سند قبض مالي</h1>
                        <span class="status-badge">مُرَحّل</span>
                    </div>
                    <div class="meta-data">
                        <p><b>رقم السند:</b> RV-${receipt.id}</p>
                        <p><b>التاريخ:</b> ${formattedDate}</p>
                        <p><b>الوقت:</b> ${formattedTime}</p>
                    </div>
                </div>

                <!-- Body -->
                <div class="voucher-body">
                    <div class="amount-box">
                        <div>
                            <span class="field-label">المبلغ رقماً:</span>
                            <span class="amount-num">${receipt.amount.toLocaleString()} ر.ي</span>
                        </div>
                        <div>
                            <span class="field-label">طريقة الدفع:</span>
                            <span class="field-value">${translatedMethod}</span>
                        </div>
                    </div>

                    <div class="row-grid">
                        <div class="field-group">
                            <span class="field-label">استلمنا من العميل/ة:</span>
                            <span class="field-value">${receipt.customerName}</span>
                        </div>
                        <div class="field-group">
                            <span class="field-label">حساب مرجعي:</span>
                            <span class="field-value">CUST-${receipt.customerId || receipt.id}</span>
                        </div>
                    </div>

                    <div class="field-group" style="margin-bottom: 15px;">
                        <span class="field-label">مبلغ وقدره كتابةً:</span>
                        <span class="field-value">${amountWords}</span>
                    </div>

                    <div class="row-grid">
                        <div class="field-group">
                            <span class="field-label">رقم العملية/المرجع:</span>
                            <span class="field-value">${receipt.referenceNo || '---'}</span>
                        </div>
                        <div class="field-group">
                            <span class="field-label">مركز التكلفة:</span>
                            <span class="field-value">المبيعات الآجلة - عملاء</span>
                        </div>
                    </div>

                    <!-- Details Table -->
                    <table class="details-table">
                        <thead>
                            <tr>
                                <th style="width: 70%;">البيان / الغرض من القبض</th>
                                <th style="width: 30%;">المبلغ</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>${receipt.notes || 'تسوية حساب العميل المالي وسداد مستحقات'}</td>
                                <td>${receipt.amount.toLocaleString()} ر.ي</td>
                            </tr>
                        </tbody>
                    </table>

                    <div class="field-group">
                        <span class="field-label">ملاحظات النظام:</span>
                        <span class="field-value">تمت التسوية آلياً وتحديث رصيد العميل بالنظام.</span>
                    </div>
                </div>

                <!-- Footer Signatures -->
                <div class="voucher-footer">
                    <div class="signature-block">
                        <div class="signature-title">المستلم (أمين الصندوق)</div>
                        <div class="signature-line">أمين الصندوق</div>
                    </div>
                    <div class="signature-block">
                        <div class="signature-title">المُسلّم (العميل/المندوب)</div>
                        <div class="signature-line"></div>
                    </div>
                    <div class="signature-block">
                        <div class="signature-title">الاعتماد (المدير المالي)</div>
                        <div class="signature-line"></div>
                    </div>
                </div>
            </div>
        `;

        const popupWin = window.open('', '_blank', 'top=0,left=0,height=700,width=850');
        if (popupWin) {
            popupWin.document.open();
            popupWin.document.write(`
                <html>
                    <head>
                        <title>طباعة سند قبض #${receipt.id}</title>
                        <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700&display=swap" rel="stylesheet">
                        <style>
                            :root {
                                --primary-color: #008080;
                                --secondary-color: #2e7d32;
                                --dark-color: #333333;
                                --light-bg: #f8f9fa;
                                --border-color: #cccccc;
                            }
                            body {
                                font-family: 'Cairo', sans-serif;
                                background-color: #ffffff;
                                color: var(--dark-color);
                                margin: 0;
                                padding: 20px;
                                font-size: 14px;
                                direction: rtl;
                            }
                            .voucher-container {
                                background: #ffffff;
                                max-width: 800px;
                                margin: 0 auto;
                                padding: 30px;
                                border: 2px solid var(--border-color);
                                border-radius: 8px;
                                position: relative;
                            }
                            .receipt-voucher { border-top: 8px solid var(--secondary-color); }
                            .voucher-header {
                                display: flex;
                                justify-content: space-between;
                                align-items: center;
                                border-bottom: 2px dashed var(--border-color);
                                padding-bottom: 15px;
                                margin-bottom: 20px;
                            }
                            .pharmacy-info h2 {
                                margin: 0 0 5px 0;
                                color: var(--dark-color);
                                font-size: 18px;
                            }
                            .pharmacy-logo {
                                width: 64px;
                                height: 64px;
                                margin-bottom: 8px;
                                border-radius: 14px;
                                overflow: hidden;
                                border: 1px solid #ddd;
                                background: #fff;
                            }
                            .pharmacy-logo img {
                                width: 100%;
                                height: 100%;
                                object-fit: cover;
                                display: block;
                            }
                            .pharmacy-info p {
                                margin: 2px 0;
                                font-size: 11px;
                                color: #666;
                            }
                            .voucher-title {
                                text-align: center;
                            }
                            .voucher-title h1 {
                                margin: 0;
                                font-size: 22px;
                                padding: 5px 25px;
                                border: 2px solid var(--secondary-color);
                                border-radius: 4px;
                                background-color: var(--light-bg);
                                color: var(--secondary-color);
                            }
                            .status-badge {
                                display: inline-block;
                                padding: 2px 10px;
                                background: #e2f0d9;
                                color: var(--secondary-color);
                                border: 1px solid rgba(46,125,50,0.3);
                                border-radius: 12px;
                                font-size: 11px;
                                font-weight: bold;
                                margin-top: 5px;
                            }
                            .meta-data p {
                                margin: 4px 0;
                                font-size: 12px;
                            }
                            .voucher-body {
                                margin-bottom: 20px;
                            }
                            .row-grid {
                                display: grid;
                                grid-template-columns: 1fr 1fr;
                                gap: 15px;
                                margin-bottom: 15px;
                            }
                            .field-group {
                                display: flex;
                                align-items: center;
                                border-bottom: 1px dashed #bbb;
                                padding-bottom: 5px;
                            }
                            .field-label {
                                font-weight: 600;
                                min-width: 120px;
                                color: #444;
                            }
                            .field-value {
                                flex-grow: 1;
                                font-weight: 400;
                            }
                            .amount-box {
                                background-color: var(--light-bg);
                                border: 1px solid var(--border-color);
                                padding: 10px 15px;
                                border-radius: 4px;
                                display: flex;
                                justify-content: space-between;
                                align-items: center;
                                margin-bottom: 15px;
                            }
                            .amount-num {
                                font-size: 18px;
                                font-weight: 700;
                                color: var(--secondary-color);
                            }
                            .details-table {
                                width: 100%;
                                border-collapse: collapse;
                                margin: 20px 0;
                            }
                            .details-table th, .details-table td {
                                border: 1px solid var(--border-color);
                                padding: 10px;
                                text-align: right;
                            }
                            .details-table th {
                                background-color: var(--light-bg);
                                font-weight: 600;
                            }
                            .voucher-footer {
                                margin-top: 40px;
                                display: flex;
                                justify-content: space-between;
                                text-align: center;
                            }
                            .signature-block {
                                width: 30%;
                            }
                            .signature-line {
                                margin-top: 35px;
                                border-top: 1px solid #999;
                                font-size: 12px;
                                color: #555;
                            }
                            @media print {
                                body { padding: 0; }
                                .voucher-container { border: 1px solid #000; }
                            }
                        </style>
                    </head>
                    <body onload="window.print();window.close()">
                        ${printContents}
                    </body>
                </html>
            `);
            popupWin.document.close();
        }
    }
}