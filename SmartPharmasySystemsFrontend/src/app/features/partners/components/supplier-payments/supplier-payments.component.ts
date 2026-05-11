import { Component, OnInit, signal, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { ConfirmDialogModule } from 'primeng/confirmdialog';

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
    ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './supplier-payments.component.html',
  styleUrls: ['./supplier-payments.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
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

  paymentForm: FormGroup;

  filteredSuppliers: Supplier[] = [];
  selectedSupplier: Supplier | null = null;

  // Data: unpaid invoices from backend (credit approved invoices)
  unpaidInvoices = signal<(PurchaseInvoice & { displayText?: string; remainingAmount?: number })[]>([]);
  filteredInvoices: any[] = [];
  allocations = signal<UnpaidInvoiceAllocation[]>([]);
  selectedInvoiceId: number | null = null;

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
      purchaseInvoiceId: [null],
      notes: ['']
    });

    this.paymentForm.get('amount')?.valueChanges.subscribe(() => this.recomputeAllocations());
  }

  ngOnInit(): void {
    this.loadPayments();

    if (this.dialogMode) {
      if (this.autoOpenCreate) this.openNew();
      if (this.presetSupplierId) this.prefillSupplier(this.presetSupplierId);
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
        this.onSupplierSelect({ value: supplier });
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
    this.supplierService.getUnpaidInvoices(supplierId).subscribe({
      next: (invoices) => {
        const mapped = invoices.map(inv => ({
          ...inv,
          remainingAmount: inv.totalAmount - (inv.paidAmount || 0),
          displayText: `${inv.purchaseInvoiceNumber || inv.id} (المتبقي: ${inv.totalAmount - (inv.paidAmount || 0)})`
        })).filter(inv => inv.remainingAmount > 0).sort((a, b) => +new Date(a.purchaseDate) - +new Date(b.purchaseDate));

        this.unpaidInvoices.set(mapped);
      },
      error: () => {
        this.unpaidInvoices.set([]);
      }
    });
  }

  searchInvoices(event: any) {
    const query = event.query.toLowerCase();
    this.filteredInvoices = this.unpaidInvoices().filter(inv =>
      inv.displayText?.toLowerCase().includes(query) ||
      inv.purchaseInvoiceNumber?.toLowerCase().includes(query)
    );
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
    this.unpaidInvoices.set([]);
    this.allocations.set([]);

    if (this.selectedSupplier?.id) {
      this.loadUnpaidInvoices(this.selectedSupplier.id);
    }
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

  printPayment(payment: SupplierPayment) {
    const printContents = `
      <div style="direction: rtl; font-family: 'Tajawal', sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; border: 2px solid #333; border-radius: 10px;">
        <div style="text-align: center; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px;">
          <h1 style="margin: 0; color: #1e3a8a;">صيدلية الشفاء الذكية</h1>
          <h2 style="margin: 10px 0 0 0; color: #333;">سند صرف مالي</h2>
        </div>
        
        <div style="display: flex; justify-content: space-between; margin-bottom: 20px;">
          <div>
            <p><strong>رقم السند:</strong> #${payment.id}</p>
            <p><strong>التاريخ:</strong> ${new Date(payment.paymentDate).toLocaleDateString('ar-YE')}</p>
          </div>
          <div>
            <p><strong>المرجع/الفاتورة:</strong> ${payment.referenceNo || '---'}</p>
          </div>
        </div>

        <div style="background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin-bottom: 20px;">
          <h3 style="margin-top: 0;">يصرف للأخ / السادة: <span style="color: #1e3a8a;">${payment.supplierName}</span></h3>
          <p style="font-size: 1.2rem; margin-bottom: 5px;"><strong>مبلغ وقدره:</strong> ${payment.amount.toLocaleString()} ريال يمني</p>
          <p><strong>وذلك بياناً لـ:</strong> ${payment.notes || 'سداد دفعة من الحساب'}</p>
        </div>

        <div style="display: flex; justify-content: space-between; margin-top: 50px; text-align: center;">
          <div style="width: 30%;">
            <p><strong>المستلم</strong></p>
            <hr style="border-top: 1px dashed #999; margin-top: 40px;">
          </div>
          <div style="width: 30%;">
            <p><strong>المحاسب</strong></p>
            <hr style="border-top: 1px dashed #999; margin-top: 40px;">
          </div>
          <div style="width: 30%;">
            <p><strong>المدير المالي</strong></p>
            <hr style="border-top: 1px dashed #999; margin-top: 40px;">
          </div>
        </div>
      </div>
    `;

    const popupWin = window.open('', '_blank', 'top=0,left=0,height=600,width=800');
    if (popupWin) {
      popupWin.document.open();
      popupWin.document.write(`
        <html>
          <head>
            <title>طباعة سند صرف #${payment.id}</title>
            <link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;700&display=swap" rel="stylesheet">
            <style>
              body { margin: 0; padding: 20px; font-family: 'Tajawal', sans-serif; }
              @media print {
                body { padding: 0; }
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