import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesInvoiceService } from '../../services/sales-invoice.service';
import {
  SaleInvoice,
  SaleInvoiceDetail,
  DocumentStatus,
  User,
} from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { InvoiceItemDialogComponent } from '../../../../shared/components/invoice-item-dialog/invoice-item-dialog.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { AuthService } from '../../../auth/services/auth.service';
import { from, concatMap, toArray } from 'rxjs';

@Component({
  selector: 'app-sales-invoice-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    ConfirmDialogModule,
    CardModule,
    ToolbarModule,
    TagModule,
    DividerModule,
    InvoiceItemDialogComponent,
    ConfirmationDialogComponent,
  ],
  template: `
    <div class="flex flex-column h-full" dir="rtl">
      <!-- ERP Header Toolbar -->
      <p-toolbar styleClass="erp-toolbar">
        <div class="p-toolbar-group-start">
          <div class="flex align-items-center gap-4">
            <p-button
              icon="pi pi-arrow-right"
              rounded
              text
              styleClass="text-white hover:bg-white-alpha-20"
              (onClick)="backToList()"
            ></p-button>
            <div class="flex flex-column">
              <h2 class="m-0 text-3xl font-extrabold line-height-2">
                نقطة بيع الأدوية
              </h2>
              <div class="flex align-items-center gap-2 mt-1">
                <span class="status-badge" [ngClass]="status.toLowerCase()">{{
                  status
                }}</span>
                <span class="text-white-alpha-70 text-sm"
                  >| &nbsp; فواتير المبيعات - اليمن</span
                >
              </div>
            </div>
          </div>
        </div>
        <div class="p-toolbar-group-end flex gap-3">
          <p-button
            *ngIf="!isReadOnly"
            label="حفظ كمسودة"
            icon="pi pi-save"
            severity="warning"
            [loading]="saving"
            (onClick)="saveDraft()"
            styleClass="p-button-raised"
          ></p-button>
          <p-button
            *ngIf="!isReadOnly"
            label="اعتماد البيع"
            icon="pi pi-check"
            severity="success"
            [loading]="saving"
            (onClick)="approveInvoice()"
            styleClass="p-button-raised"
          ></p-button>
          <p-button
            *ngIf="isReadOnly"
            label="طباعة الفاتورة"
            icon="pi pi-print"
            severity="secondary"
            outlined
            styleClass="text-white border-white-alpha-30"
          ></p-button>
        </div>
      </p-toolbar>

      <div class="flex-grow-1 overflow-auto form-container">
        <form [formGroup]="invoiceForm" class="grid">
          <!-- Right Sidebar: Financial Summary -->
          <div class="col-12 lg:col-4 lg:order-2">
            <p-card
              header="الخلاصة المالية"
              styleClass="financial-summary h-full"
            >
              <div class="flex flex-column gap-2">
                <div class="summary-item">
                  <span class="label">إجمالي البنود</span>
                  <span class="value">{{ details.length }} أصناف</span>
                </div>
                <div class="summary-item">
                  <span class="label">إجمالي الربح التقديري</span>
                  <span class="value text-green-600 font-bold"
                    >{{ calculateTotalProfit() | number : '1.2-2' }} ريال
                    يمني</span
                  >
                </div>
                <div class="summary-item">
                  <span class="label">المبلغ الصافي</span>
                  <span class="value"
                    >{{ calculateTotal() | number : '1.2-2' }} ريال يمني</span
                  >
                </div>
                <div class="summary-item total">
                  <span class="label">إجمالي الفاتورة</span>
                  <span class="value"
                    >{{ calculateTotal() | number : '1.2-2' }} ريال يمني</span
                  >
                </div>
              </div>

              <div class="mt-5" *ngIf="!isReadOnly">
                <p-button
                  label="إتمام عملية البيع (F10)"
                  icon="pi pi-verified"
                  severity="success"
                  [disabled]="details.length === 0"
                  styleClass="w-full py-3 text-xl font-bold"
                  (onClick)="approveInvoice()"
                ></p-button>
                <p-divider></p-divider>
                <p-button
                  label="إلغاء كافة البنود"
                  icon="pi pi-refresh"
                  severity="danger"
                  text
                  styleClass="w-full"
                  (onClick)="details.clear()"
                ></p-button>
              </div>
            </p-card>
          </div>

          <!-- Main Content: Header Info & Details -->
          <div class="col-12 lg:col-8 lg:order-1">
            <!-- Invoice Info -->
            <p-card header="بيانات العميل والسداد" styleClass="mb-4">
              <div class="grid p-fluid">
                <div class="col-12 md:col-6">
                  <label class="font-semibold block mb-2 text-primary"
                    >اسم العميل</label
                  >
                  <input
                    pInputText
                    formControlName="customerName"
                    placeholder="أدخل اسم العميل (أو عميل نقدي)..."
                    [readonly]="isReadOnly"
                  />
                </div>
                <div class="col-12 md:col-6">
                  <label class="font-semibold block mb-2">رقم الهاتف</label>
                  <input
                    pInputText
                    formControlName="customerPhone"
                    placeholder="رقم الهاتف للتواصل..."
                    [readonly]="isReadOnly"
                  />
                </div>
                <div class="col-12 md:col-6">
                  <label class="font-semibold block mb-2">طريقة السداد</label>
                  <p-dropdown
                    [options]="paymentMethods"
                    formControlName="paymentMethod"
                    optionLabel="label"
                    optionValue="value"
                    [disabled]="isReadOnly"
                  ></p-dropdown>
                </div>
                <div class="col-12 md:col-6">
                  <label class="font-semibold block mb-2">تاريخ الفاتورة</label>
                  <p-calendar
                    formControlName="saleInvoiceDate"
                    [showIcon]="true"
                    appendTo="body"
                    [disabled]="isReadOnly"
                    dateFormat="dd/mm/yy"
                  ></p-calendar>
                </div>
              </div>
            </p-card>

            <!-- Items Grid -->
            <p-card header="أصناف البيع">
              <div class="flex justify-content-between align-items-center mb-4">
                <div class="flex align-items-center gap-2 text-secondary">
                  <i class="pi pi-shopping-cart font-bold"></i>
                  <span>قائمة الأدوية المبيعة</span>
                </div>
                <p-button
                  label="إضافة صنف مبيعات"
                  icon="pi pi-plus"
                  severity="primary"
                  [outlined]="true"
                  size="small"
                  (onClick)="openItemDialog()"
                  [disabled]="isReadOnly"
                ></p-button>
              </div>

              <p-table
                [value]="details.value"
                responsiveLayout="scroll"
                styleClass="p-datatable-gridlines p-datatable-sm shadow-1 border-round-lg overflow-hidden"
              >
                <ng-template pTemplate="header">
                  <tr>
                    <th class="text-right">اسم الدواء / الصنف</th>
                    <th class="text-center" style="width: 140px">رقم الدفعة</th>
                    <th class="text-center" style="width: 100px">الكمية</th>
                    <th class="text-right" style="width: 150px">
                      السعر (ريال يمني)
                    </th>
                    <th class="text-right" style="width: 150px">الإجمالي</th>
                    <th
                      class="text-center"
                      style="width: 100px"
                      *ngIf="!isReadOnly"
                    >
                      إجراءات
                    </th>
                  </tr>
                </ng-template>
                <ng-template pTemplate="body" let-item let-i="rowIndex">
                  <tr
                    class="hover:surface-50 cursor-pointer"
                    (click)="!isReadOnly && openItemDialog(item, i)"
                  >
                    <td class="font-bold text-primary">
                      {{ item.medicineName }}
                    </td>
                    <td
                      class="text-center font-mono py-2 bg-blue-50 text-blue-700 border-round"
                    >
                      {{ item.companyBatchNumber }}
                    </td>
                    <td class="text-center font-bold text-xl">
                      {{ item.quantity }}
                    </td>
                    <td class="text-right">
                      {{ item.salePrice | number : '1.2-2' }}
                    </td>
                    <td class="text-right font-bold text-indigo-700">
                      {{ item.totalLineAmount | number : '1.2-2' }}
                    </td>
                    <td
                      class="text-center"
                      *ngIf="!isReadOnly"
                      (click)="$event.stopPropagation()"
                    >
                      <div class="flex justify-content-center gap-1">
                        <p-button
                          icon="pi pi-pencil"
                          rounded
                          text
                          severity="warning"
                          (onClick)="openItemDialog(item, i)"
                        ></p-button>
                        <p-button
                          icon="pi pi-trash"
                          rounded
                          text
                          severity="danger"
                          (onClick)="removeItem(i)"
                        ></p-button>
                      </div>
                    </td>
                  </tr>
                </ng-template>
                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td
                      colspan="6"
                      class="text-center p-6 text-secondary bg-gray-50 border-round-bottom-lg"
                    >
                      <div class="flex flex-column align-items-center gap-3">
                        <i class="pi pi-calculator text-5xl opacity-20"></i>
                        <span class="text-xl"
                          >شاغرة.. يرجى البدء بإدراج أصناف المبيعات</span
                        >
                        <p-button
                          label="أضف أول صنف بيع"
                          icon="pi pi-plus"
                          text
                          (onClick)="openItemDialog()"
                          *ngIf="!isReadOnly"
                        ></p-button>
                      </div>
                    </td>
                  </tr>
                </ng-template>
                <ng-template pTemplate="footer" *ngIf="details.length > 0">
                  <tr>
                    <td
                      colspan="2"
                      class="text-left font-bold bg-surface-section text-xl"
                    >
                      إجمالي الفاتورة
                    </td>
                    <td
                      class="text-center bg-surface-section text-primary text-xl font-bold"
                    >
                      {{ calculateTotalQuantity() }}
                    </td>
                    <td class="bg-surface-section"></td>
                    <td
                      class="text-right font-extrabold text-primary bg-surface-section text-xl"
                    >
                      {{ calculateTotal() | number : '1.2-2' }}
                    </td>
                    <td *ngIf="!isReadOnly" class="bg-surface-section"></td>
                  </tr>
                </ng-template>
              </p-table>
            </p-card>
          </div>
        </form>
      </div>
    </div>

    <app-invoice-item-dialog
      #itemDialog
      invoiceType="Sales"
      (onSave)="onItemSaved($event)"
    ></app-invoice-item-dialog>

    <app-confirmation-dialog
      #confirmDialog
      [header]="'تأكيد فاتورة البيع'"
      [message]="'هل تريد اعتماد الفاتورة وترحيلها للحسابات؟'"
      [subMessage]="
        'عملية الاعتماد ستؤدي لخصم الكميات فوراً من المخزن وتسجيل الإيراد في النظام اليمني.'
      "
      [severity]="'success'"
      confirmLabel="نعم، اعتماد الآن"
      (accept)="onConfirmApprove()"
    >
    </app-confirmation-dialog>
  `,
  styleUrls: ['./sale-invoice-create.component.scss'],
  providers: [ConfirmationService],
})
export class SalesInvoiceCreateComponent implements OnInit {
  @ViewChild('itemDialog') itemDialog!: InvoiceItemDialogComponent;
  @ViewChild('confirmDialog') confirmDialog!: ConfirmationDialogComponent;

  invoiceForm: FormGroup;
  saving = false;
  isEditMode = false;
  currentInvoiceId: number | null = null;
  editingIndex: number | null = null;
  status: DocumentStatus = DocumentStatus.DRAFT;

  DocumentStatus = DocumentStatus; // Expose to template
  currentUser: User | null = null;

  paymentMethods = [
    { label: 'نقد (Cash)', value: 'Cash' },
    { label: 'آجل (On Credit)', value: 'Credit' },
  ];

  constructor(
    private fb: FormBuilder,
    private salesService: SalesInvoiceService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private authService: AuthService
  ) {
    this.invoiceForm = this.fb.group({
      saleInvoiceNumber: [''],
      customerName: [''],
      customerPhone: [''],
      saleInvoiceDate: [new Date(), Validators.required],
      paymentMethod: ['Cash', Validators.required],
      notes: [''],
      saleInvoiceDetails: this.fb.array([]),
    });
  }

  ngOnInit() {
    this.authService
      .getCurrentUser()
      .subscribe((user) => (this.currentUser = user));

    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.currentInvoiceId = +id;
      this.loadInvoice(id);
    }
  }

  get isReadOnly() {
    return this.status !== DocumentStatus.DRAFT;
  }

  get details() {
    return this.invoiceForm.get('saleInvoiceDetails') as FormArray;
  }

  loadInvoice(id: number) {
    this.salesService.getById(id).subscribe((data) => {
      if (data.status !== DocumentStatus.DRAFT) {
        this.router.navigate(['/sales-invoices', id]);
        return;
      }
      this.invoiceForm.patchValue({
        ...data,
        saleInvoiceDate: new Date(data.saleInvoiceDate),
      });
      this.status = data.status || DocumentStatus.DRAFT;
      this.details.clear();
      data.saleInvoiceDetails?.forEach((d) => this.addDetailToForm(d));
    });
  }

  addDetailToForm(detail: any) {
    const group = this.fb.group({
      id: [detail.id || 0],
      medicineId: [detail.medicineId, Validators.required],
      medicineName: [detail.medicineName],
      batchId: [detail.batchId, Validators.required],
      companyBatchNumber: [detail.companyBatchNumber],
      quantity: [detail.quantity, [Validators.required, Validators.min(1)]],
      salePrice: [detail.salePrice, Validators.required],
      totalLineAmount: [
        detail.totalLineAmount || detail.quantity * detail.salePrice,
      ],
      unitCost: [detail.unitCost || 0],
    });
    this.details.push(group);
  }

  openItemDialog(item?: any, index?: number) {
    this.editingIndex = index !== undefined ? index : null;
    const dialogData = item
      ? {
        medicineId: item.medicineId,
        medicineName: item.medicineName,
        batchId: item.batchId,
        companyBatchNumber: item.companyBatchNumber,
        quantity: item.quantity,
        price: item.salePrice,
        unitCost: item.unitCost,
      }
      : null;
    this.itemDialog.show(dialogData);
  }

  onItemSaved(itemData: any) {
    const detail = {
      medicineId: itemData.medicineId,
      medicineName: itemData.medicineName,
      batchId: itemData.batchId,
      companyBatchNumber: itemData.companyBatchNumber,
      quantity: itemData.quantity,
      salePrice: itemData.price,
      totalLineAmount: itemData.quantity * itemData.price,
      unitCost: itemData.unitCost || 0,
    };

    if (this.editingIndex !== null) {
      this.details.at(this.editingIndex).patchValue(detail);
    } else {
      this.addDetailToForm(detail);
    }
  }

  removeItem(index: number) {
    this.details.removeAt(index);
  }

  calculateTotal() {
    const total = this.details.controls.reduce(
      (acc, ctrl) => acc + (ctrl.get('totalLineAmount')?.value || 0),
      0
    );
    return Number(total.toFixed(2));
  }

  calculateTotalQuantity() {
    return this.details.controls.reduce(
      (acc, ctrl) => acc + (ctrl.get('quantity')?.value || 0),
      0
    );
  }

  calculateTotalProfit() {
    const profit = this.details.controls.reduce((acc, ctrl) => {
      const sale = ctrl.get('totalLineAmount')?.value || 0;
      const cost =
        (ctrl.get('unitCost')?.value || 0) * (ctrl.get('quantity')?.value || 0);
      return acc + (sale - cost);
    }, 0);
    return Number(profit.toFixed(2));
  }

  logInvoiceError(error: any): void {
    console.error('--- ERP SALES ERROR LOG ---');
    console.error('Timestamp:', new Date().toISOString());
    console.error('Status:', error.status);
    console.error('Message:', error.message);
    console.error('Backend Detail:', error.error);
    console.error('---------------------------');
  }

  getInvoiceError(detail: any): string {
    if (!detail) return 'بيانات غير صالحة';
    if (detail.quantity <= 0) return 'الكمية يجب أن تزيد عن صفر';
    if (!detail.batchId) return 'يجب اختيار الدفعة المتاحة';
    return 'خطأ في معالجة البند';
  }

  saveDraft() {
    if (this.details.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يجب إضافة صنف واحد على الأقل',
      });
      return;
    }

    this.saving = true;
    const formData = this.invoiceForm.getRawValue();
    const headerPayload = {
      saleInvoiceDate: formData.saleInvoiceDate,
      customerName: formData.customerName,
      paymentMethod: formData.paymentMethod,
      notes: formData.notes,
      createdBy: this.currentUser?.id || 1,
    };

    const headerObs =
      this.isEditMode && this.currentInvoiceId
        ? this.salesService.update(this.currentInvoiceId, headerPayload)
        : this.salesService.create(headerPayload);

    headerObs.subscribe({
      next: (invoice) => {
        this.currentInvoiceId = invoice.id;
        // Save details one by one
        const detailsPayload = formData.saleInvoiceDetails.map((d: any) => ({
          ...d,
          saleInvoiceId: invoice.id,
        }));

        from(detailsPayload)
          .pipe(
            concatMap((detail: any) => {
              if (detail.id > 0) {
                return this.salesService.updateDetail(detail.id, detail);
              } else {
                return this.salesService.createDetail(detail);
              }
            }),
            toArray()
          )
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'نجاح',
                detail: 'تم حفظ المسودة بنجاح',
              });
              this.router.navigate(['/sales-invoices']);
            },
            error: (err) => {
              this.logInvoiceError(err);
              this.messageService.add({
                severity: 'error',
                summary: 'خطأ',
                detail: 'تم حفظ الرأس وفشل حفظ بعض البنود',
              });
              this.saving = false;
            },
          });
      },
      error: (err) => {
        this.logInvoiceError(err);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'فشل الحفظ',
        });
        this.saving = false;
      },
    });
  }

  approveInvoice() {
    if (this.details.length === 0) return;
    this.confirmDialog.show();
  }

  onConfirmApprove() {
    this.saving = true;
    const formData = this.invoiceForm.getRawValue();
    const headerPayload = {
      saleInvoiceDate: formData.saleInvoiceDate,
      customerName: formData.customerName,
      paymentMethod: formData.paymentMethod,
      notes: formData.notes,
      createdBy: this.currentUser?.id || 1,
    };

    const headerObs =
      this.isEditMode && this.currentInvoiceId
        ? this.salesService.update(this.currentInvoiceId, headerPayload)
        : this.salesService.create(headerPayload);

    headerObs.subscribe({
      next: (invoice) => {
        this.currentInvoiceId = invoice.id;
        const detailsPayload = formData.saleInvoiceDetails.map((d: any) => ({
          ...d,
          saleInvoiceId: invoice.id,
        }));

        from(detailsPayload)
          .pipe(
            concatMap((detail: any) => {
              if (detail.id > 0) {
                return this.salesService.updateDetail(detail.id, detail);
              } else {
                return this.salesService.createDetail(detail);
              }
            }),
            toArray()
          )
          .subscribe({
            next: () => {
              this.salesService.approve(invoice.id).subscribe({
                next: () => {
                  this.messageService.add({
                    severity: 'success',
                    summary: 'نجاح',
                    detail: 'تم الاعتماد النهائي',
                  });
                  this.router.navigate(['/sales-invoices', invoice.id]);
                },
                error: (err) => {
                  this.logInvoiceError(err);
                  this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ في الاعتماد',
                    detail: err.error?.message,
                  });
                  this.saving = false;
                },
              });
            },
            error: (err) => {
              this.logInvoiceError(err);
              this.saving = false;
            },
          });
      },
      error: (err) => {
        this.logInvoiceError(err);
        this.saving = false;
      },
    });
  }

  backToList() {
    this.router.navigate(['/sales-invoices']);
  }
}
