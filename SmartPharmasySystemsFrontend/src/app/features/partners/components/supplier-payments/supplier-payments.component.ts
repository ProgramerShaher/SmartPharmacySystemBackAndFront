import { Component, OnInit, signal, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SupplierService } from '../../services/supplier.service';
import { Supplier, SupplierPayment, CreateSupplierPaymentDto, PurchaseInvoice } from '../../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { SettingsService } from '../../../../core/services/settings.service';
import { PharmacySettings } from '../../../../core/models/settings/pharmacy-settings.interface';
import { environment } from '../../../../../environments/environment';

type UnpaidInvoiceAllocation = {
  invoiceId: number;
  invoiceRef: string;
  invoiceDate: string;
  invoiceTotal: number;
  allocated: number;
  remaining: number;
};

@Component({
  selector: 'app-supplier-payments',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    AutoCompleteModule,
    InputNumberModule,
    CalendarModule,
    InputTextModule,
    ToastModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService, DatePipe],
  templateUrl: './supplier-payments.component.html',
  styleUrls: ['./supplier-payments.component.scss']
})
export class SupplierPaymentsComponent implements OnInit {
  @Input() dialogMode = false;
  @Input() autoOpenCreate = false;
  @Input() presetSupplierId?: number | null;
  @Output() closed = new EventEmitter<void>();

  payments = signal<SupplierPayment[]>([]);
  loading = signal(false);
  displayDialog = signal(false);
  saving = signal(false);
  loadingInvoices = signal(false);

  paymentForm: FormGroup;

  filteredSuppliers: Supplier[] = [];
  selectedSupplier: Supplier | null = null;

  pharmacySettings = signal<PharmacySettings>({
    id: 0,
    pharmacyName: 'صيدلية الشفاء العالمية',
    taxNumber: '300123456700003',
    phoneNumber: '011234567',
    address: 'الرياض، المملكة العربية السعودية',
    baseCurrency: 'ر.ي'
  });
  readonly serverUrl = environment.apiUrl.replace('/api', '');

  // Unpaid invoices for the selected supplier
  unpaidInvoices = signal<(PurchaseInvoice & { displayText?: string; remainingAmount?: number })[]>([]);
  filteredInvoices: any[] = [];
  allocations = signal<UnpaidInvoiceAllocation[]>([]);
  selectedInvoiceId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private settingsService: SettingsService
  ) {
    this.paymentForm = this.fb.group({
      supplierId: [null, Validators.required],
      amount: [0, [Validators.required, Validators.min(1)]],
      paymentDate: [new Date(), Validators.required],
      referenceNo: [''],
      purchaseInvoiceId: [null],
      notes: ['']
    });

    this.paymentForm.get('amount')?.valueChanges.subscribe(() => this.recomputeAllocations());
  }

  ngOnInit(): void {
    this.loadPayments();
    this.settingsService.getSettings().subscribe({
      next: (settings) => {
        if (settings) {
          this.pharmacySettings.set(settings);
        }
      }
    });

    if (this.dialogMode) {
      if (this.autoOpenCreate) this.openNew();
      // Delay prefill slightly so dialog is rendered first
      if (this.presetSupplierId) {
        setTimeout(() => this.prefillSupplier(this.presetSupplierId!), 100);
      }
      return;
    }

    const supplierId = this.route.snapshot.queryParamMap.get('supplierId');
    if (supplierId) {
      this.openNew();
      this.prefillSupplier(+supplierId);
    }
  }

  private prefillSupplier(supplierId: number): void {
    this.supplierService.getById(supplierId).subscribe({
      next: supplier => {
        this.filteredSuppliers = [supplier];
        // Set the form value to the supplier object (autocomplete expects full object)
        this.paymentForm.patchValue({ supplierId: supplier });
        this.selectedSupplier = supplier;
        // Load unpaid invoices for this supplier
        this.loadUnpaidInvoices(supplier.id);
        this.cdr.markForCheck();
      }
    });
  }

  loadPayments() {
    this.loading.set(true);
    this.supplierService.getPayments().subscribe({
      next: (res) => {
        this.payments.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل سندات الصرف' });
        this.loading.set(false);
      }
    });
  }

  searchSuppliers(event: any) {
    this.supplierService.getAll({ search: event.query, pageSize: 20 }).subscribe(res => {
      this.filteredSuppliers = res.items;
    });
  }

  private loadUnpaidInvoices(supplierId: number) {
    this.loadingInvoices.set(true);
    this.supplierService.getUnpaidInvoices(supplierId).subscribe({
      next: (invoices) => {
        const mapped = invoices.map(inv => ({
          ...inv,
          remainingAmount: inv.totalAmount - (inv.paidAmount || 0),
          displayText: `#${inv.purchaseInvoiceNumber || inv.id} — متبقي: ${(inv.totalAmount - (inv.paidAmount || 0)).toLocaleString()} ر.ي`
        })).filter(inv => (inv.remainingAmount || 0) > 0)
          .sort((a, b) => +new Date(a.purchaseDate) - +new Date(b.purchaseDate));

        this.unpaidInvoices.set(mapped);
        // Initialize filteredInvoices so dropdown shows all on open
        this.filteredInvoices = [...mapped];
        this.loadingInvoices.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.unpaidInvoices.set([]);
        this.filteredInvoices = [];
        this.loadingInvoices.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  searchInvoices(event: any) {
    const query = (event.query || '').toLowerCase().trim();
    // If empty query (dropdown opened), show all
    if (!query) {
      this.filteredInvoices = [...this.unpaidInvoices()];
    } else {
      this.filteredInvoices = this.unpaidInvoices().filter(inv =>
        inv.displayText?.toLowerCase().includes(query) ||
        (inv.purchaseInvoiceNumber || '').toLowerCase().includes(query) ||
        String(inv.id).includes(query)
      );
    }
  }

  onInvoiceSelect(event: any) {
    const inv = event.value;
    this.selectedInvoiceId = inv.id;
    this.paymentForm.patchValue({
      amount: inv.remainingAmount,
      purchaseInvoiceId: inv.id,
      referenceNo: inv.purchaseInvoiceNumber || String(inv.id)
    });
  }

  onSupplierSelect(event: any) {
    this.selectedSupplier = event.value;
    this.paymentForm.patchValue({ supplierId: this.selectedSupplier });
    // Reset invoice selection state
    this.unpaidInvoices.set([]);
    this.filteredInvoices = [];
    this.allocations.set([]);
    this.selectedInvoiceId = null;
    this.paymentForm.patchValue({ referenceNo: '', purchaseInvoiceId: null });

    if (this.selectedSupplier?.id) {
      this.loadUnpaidInvoices(this.selectedSupplier.id);
    }
    this.cdr.markForCheck();
  }

  selectedInvoices: PurchaseInvoice[] = [];

  onInvoiceSelectionChange(event: any) {
    // Legacy support if needed
  }

  openNew() {
    this.selectedSupplier = null;
    this.unpaidInvoices.set([]);
    this.selectedInvoiceId = null;
    this.filteredInvoices = [];
    this.paymentForm.reset({ paymentDate: new Date(), amount: 0, supplierId: null, referenceNo: '', purchaseInvoiceId: null, notes: '' });
    this.displayDialog.set(true);
  }

  closeModal() {
    this.displayDialog.set(false);
    if (this.dialogMode) this.closed.emit();
  }

  private recomputeAllocations() {
    const amountRaw = this.paymentForm.get('amount')?.value;
    const amount = Number(amountRaw || 0);
    if (!this.unpaidInvoices().length) {
      this.allocations.set([]);
      return;
    }

    if (!Number.isFinite(amount) || amount <= 0) {
      this.allocations.set(
        this.unpaidInvoices().map(inv => ({
          invoiceId: inv.id,
          invoiceRef: inv.purchaseInvoiceNumber || String(inv.id),
          invoiceDate: inv.purchaseDate,
          invoiceTotal: inv.totalAmount,
          allocated: 0,
          remaining: inv.totalAmount
        }))
      );
      return;
    }

    let remainingPayment = amount;

    const next = this.unpaidInvoices().map(inv => {
      const invoiceTotal = Number(inv.totalAmount || 0);
      const allocated = Math.min(invoiceTotal, remainingPayment);
      const invoiceRemaining = invoiceTotal - allocated;
      remainingPayment -= allocated;

      return {
        invoiceId: inv.id,
        invoiceRef: inv.purchaseInvoiceNumber || String(inv.id),
        invoiceDate: inv.purchaseDate,
        invoiceTotal,
        allocated,
        remaining: invoiceRemaining
      };
    }).filter(row => row.remaining > 0);

    this.allocations.set(next);
  }

  get totalUnpaidDebtFull(): number {
    // "الرصيد المستحق كامل" = sum of unpaid invoices totals
    return this.unpaidInvoices().reduce((s, inv) => s + (inv.totalAmount || 0), 0);
  }

  get totalRemainingAfterEntry(): number {
    return this.allocations().reduce((s, a) => s + a.remaining, 0);
  }

  savePayment() {
    if (this.paymentForm.invalid || !this.paymentForm.value.supplierId?.id) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const amount = Number(this.paymentForm.value.amount || 0);

    this.supplierService.checkVaultBalance(amount).subscribe({
      next: (hasBalance) => {
        if (!hasBalance) {
          this.messageService.add({ severity: 'error', summary: 'رصيد غير كاف', detail: 'رصيد الخزينة لا يكفي.' });
          return;
        }
        this.executeSave(amount);
      },
      error: () => this.executeSave(amount)
    });
  }

  private executeSave(amount: number) {
    this.saving.set(true);

    const finalNotesBase = this.paymentForm.value.notes || '';
    const refValue = this.paymentForm.value.referenceNo;
    const finalRefNo = typeof refValue === 'object' && refValue ? (refValue.purchaseInvoiceNumber || String(refValue.id)) : refValue;
    const invoiceId = this.paymentForm.value.purchaseInvoiceId || (typeof refValue === 'object' && refValue ? refValue.id : null);

    const dto: CreateSupplierPaymentDto = {
      supplierId: this.paymentForm.value.supplierId.id,
      amount,
      paymentDate: (this.paymentForm.value.paymentDate as Date).toISOString(),
      referenceNo: finalRefNo,
      purchaseInvoiceId: invoiceId,
      notes: finalNotesBase
        ? finalNotesBase
        : `سند صرف لمورد`
    };

    this.supplierService.createPayment(dto).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم إنشاء سند الصرف بنجاح' });
        this.saving.set(false);
        this.displayDialog.set(false);
        this.loadPayments();
        if (this.dialogMode) this.closed.emit();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حفظ سند الصرف' });
        this.saving.set(false);
      }
    });
  }

  cancelPayment(event: Event, payment: SupplierPayment) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل تريد إلغاء السند رقم ${payment.id}؟`,
      header: 'تأكيد الإلغاء',
      icon: 'pi pi-exclamation-circle',
      acceptLabel: 'إلغاء السند',
      rejectLabel: 'تراجع',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.supplierService.cancelPayment(payment.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إلغاء السند' });
            this.loadPayments();
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

  printPayment(payment: SupplierPayment) {
    const dateObj = new Date(payment.paymentDate);
    const formattedDate = dateObj.toLocaleDateString('ar-YE');
    const formattedTime = dateObj.toLocaleTimeString('ar-YE', { hour: '2-digit', minute: '2-digit' });

    const translatedMethod = payment.referenceNo ? 'حوالة / حساب بنكي' : 'نقداً (كاش)';
    const amountWords = 'فقط ' + this.convertNumberToWords(payment.amount) + ' ريال يمني لا غير.';

    const logoMarkup = this.pharmacySettings().logoUrl
      ? `<div class="pharmacy-logo"><img src="${this.serverUrl}${this.pharmacySettings().logoUrl}" alt="${this.pharmacySettings().pharmacyName}"></div>`
      : '';

    const printContents = `
      <div class="voucher-container payment-voucher" dir="rtl">
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
                <h1>سند صرف مالي</h1>
                <span class="status-badge">مُرَحّل</span>
            </div>
            <div class="meta-data">
                <p><b>رقم السند:</b> PV-${payment.id}</p>
                <p><b>التاريخ:</b> ${formattedDate}</p>
                <p><b>الوقت:</b> ${formattedTime}</p>
            </div>
        </div>

        <!-- Body -->
        <div class="voucher-body">
            <div class="amount-box">
                <div>
                    <span class="field-label">المبلغ رقماً:</span>
                    <span class="amount-num">${payment.amount.toLocaleString()} ر.ي</span>
                </div>
                <div>
                    <span class="field-label">طريقة الصرف:</span>
                    <span class="field-value">${translatedMethod}</span>
                </div>
            </div>

            <div class="row-grid">
                <div class="field-group">
                    <span class="field-label">إلى السيد/الشركة:</span>
                    <span class="field-value">${payment.supplierName}</span>
                </div>
                <div class="field-group">
                    <span class="field-label">حساب مرجعي:</span>
                    <span class="field-value">SUP-${payment.supplierId || payment.id}</span>
                </div>
            </div>

            <div class="field-group" style="margin-bottom: 15px;">
                <span class="field-label">مبلغ وقدره كتابةً:</span>
                <span class="field-value">${amountWords}</span>
            </div>

            <div class="row-grid">
                <div class="field-group">
                    <span class="field-label">رقم العملية/المرجع:</span>
                    <span class="field-value">${payment.referenceNo || '---'}</span>
                </div>
                <div class="field-group">
                    <span class="field-label">تاريخ الاستحقاق:</span>
                    <span class="field-value">${payment.referenceNo ? formattedDate : 'فوري'}</span>
                </div>
            </div>

            <!-- Details Table -->
            <table class="details-table">
                <thead>
                    <tr>
                        <th style="width: 70%;">البيان / الغرض من الصرف</th>
                        <th style="width: 30%;">المبلغ</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>${payment.notes || 'سداد مستحقات وفواتير المورد الآجلة'}</td>
                        <td>${payment.amount.toLocaleString()} ر.ي</td>
                    </tr>
                </tbody>
            </table>

            <div class="field-group">
                <span class="field-label">ملاحظات النظام:</span>
                <span class="field-value">تم خصم القيمة من رصيد المورد المعتمد وتخفيض مديونيته بنجاح.</span>
            </div>
        </div>

        <!-- Footer Signatures -->
        <div class="voucher-footer">
            <div class="signature-block">
                <div class="signature-title">المحاسب المالي</div>
                <div class="signature-line">أمين الصندوق</div>
            </div>
            <div class="signature-block">
                <div class="signature-title">المستلم (المندوب/الشركة)</div>
                <div class="signature-line"></div>
            </div>
            <div class="signature-block">
                <div class="signature-title">الاعتماد (مدير الصيدلية)</div>
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
            <title>طباعة سند صرف #${payment.id}</title>
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
              .payment-voucher { border-top: 8px solid var(--primary-color); }
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
                border: 2px solid var(--primary-color);
                border-radius: 4px;
                background-color: var(--light-bg);
                color: var(--primary-color);
              }
              .status-badge {
                display: inline-block;
                padding: 2px 10px;
                background: #e0f2fe;
                color: var(--primary-color);
                border: 1px solid rgba(0,128,128,0.3);
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
                color: var(--primary-color);
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

