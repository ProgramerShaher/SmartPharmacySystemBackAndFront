import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import {
  PurchaseInvoice,
  InventoryMovement,
  DocumentStatus,
} from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SaleInvoiceActionsDialogComponent } from '../../../sales/components/sale-invoice-actions-dialog/sale-invoice-actions-dialog.component';
import { StockCardLiteComponent } from '../../../../shared/components/stock-card-lite/stock-card-lite.component';

@Component({
  selector: 'app-purchase-invoice-details',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    CardModule,
    DividerModule,
    ConfirmDialogModule,
    TooltipModule,
    ProgressSpinnerModule,
    SaleInvoiceActionsDialogComponent,
    StockCardLiteComponent,
  ],
  template: `
    <div class="purchase-details-wrapper" dir="rtl" *ngIf="invoice">
      <!-- Sleek Top Nav -->
      <div class="nav-header">
        <div class="back-link" (click)="backToList()">
          <i class="pi pi-arrow-right"></i>
          <span>العودة لقائمة التوريدات</span>
        </div>
        <div class="flex align-items-center gap-3">
          <span class="text-600 text-sm font-bold">رقم السند الرقمي:</span>
          <span class="font-mono font-black text-indigo-700"
            >#{{ invoice.purchaseInvoiceNumber || invoice.id }}</span
          >
        </div>
      </div>

      <!-- Hero Summary Section -->
      <div class="hero-summary">
        <div class="title-area">
          <h1>سند توريد مخزني</h1>
          <p class="subtitle">
            تحرير وتدقيق بيانات الأصناف الموردة للمركز الرئيسي
          </p>

          <div class="flex gap-4 mt-5">
            <div class="flex flex-column">
              <span class="text-xs font-bold uppercase opacity-50 mb-1"
                >تاريخ الحركات</span
              >
              <span
                class="font-bold border-1 border-white-alpha-20 px-3 py-1 border-round-md"
              >
                {{ invoice.purchaseDate | date : 'dd MMM yyyy' }}
              </span>
            </div>
            <div class="flex flex-column">
              <span class="text-xs font-bold uppercase opacity-50 mb-1"
                >الحالة التشغيلية</span
              >
              <span
                class="font-bold border-1 border-white-alpha-20 px-3 py-1 border-round-md"
              >
                {{ getStatusLabel(invoice.status) }}
              </span>
            </div>
          </div>
        </div>

        <div class="amount-display">
          <span class="label uppercase font-black">إجمالي قيمة الفاتورة</span>
          <div class="flex align-items-baseline">
            <span class="value">{{
              invoice.totalAmount | number : '1.0-0'
            }}</span>
            <span class="currency">ريال يمني</span>
          </div>
        </div>
      </div>

      <div class="details-body">
        <!-- Main Detail Area -->
        <div class="document-card">
          <div
            class="flex align-items-center justify-content-between mb-5 border-bottom-1 border-gray-100 pb-4"
          >
            <div class="flex align-items-center gap-3">
              <i class="pi pi-shopping-bag text-indigo-600 text-2xl"></i>
              <h2 class="m-0 font-black text-900">الأصناف الملحقة بالسند</h2>
            </div>
            <span class="text-600 font-bold"
              >{{ invoice.purchaseInvoiceDetails?.length || 0 }} صنف مدخل</span
            >
          </div>

          <p-table
            [value]="invoice.purchaseInvoiceDetails || []"
            dataKey="id"
            styleClass="framed-table"
          >
            <ng-template pTemplate="header">
              <tr>
                <th style="width: 3rem"></th>
                <th>اسم الصنف الدوائي</th>
                <th class="text-center">التشغيلة</th>
                <th class="text-center">الصلاحية</th>
                <th class="text-center">الكمية</th>
                <th class="text-right">سعر الوحدة</th>
                <th class="text-right">الإجمالي</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-detail let-expanded="expanded">
              <tr [pRowToggler]="detail" class="cursor-pointer">
                <td>
                  <i
                    class="pi"
                    [ngClass]="expanded ? 'pi-chevron-down' : 'pi-chevron-left'"
                    style="color: #94a3b8"
                  ></i>
                </td>
                <td>
                  <div class="flex flex-column">
                    <span class="font-black text-indigo-900 text-lg">{{
                      detail.medicineName
                    }}</span>
                    <small class="text-500"
                      >كود الصنف: {{ detail.medicineId }}</small
                    >
                  </div>
                </td>
                <td class="text-center">
                  <span class="batch-pill">{{
                    detail.companyBatchNumber
                  }}</span>
                </td>
                <td class="text-center">
                  <span class="expiry-tag">
                    <i class="pi pi-calendar"></i>
                    {{ detail.expiryDate | date : 'MM/yyyy' }}
                  </span>
                </td>
                <td class="text-center font-black text-xl">
                  {{ detail.quantity }}
                </td>
                <td class="text-right font-bold text-600">
                  {{ detail.purchasePrice | number : '1.2-2' }}
                </td>
                <td class="text-right font-black text-indigo-700 text-lg">
                  {{ detail.total | number : '1.2-2' }}
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="rowexpansion" let-detail>
              <tr>
                <td colspan="7" class="p-0">
                  <div class="expansion-container">
                    <div class="expansion-header">
                      <i class="pi pi-chart-line"></i>
                      <span>سجل حركة المخزن اللحظي</span>
                    </div>
                    <div class="bg-white p-4 border-round-xl shadow-1">
                      <app-stock-card-lite
                        [batchId]="detail.batchId"
                      ></app-stock-card-lite>
                    </div>
                  </div>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <!-- Meta Sidebar -->
        <div class="meta-rail">
          <div class="meta-card">
            <span class="meta-title">معلومات المورد</span>
            <div class="flex align-items-center gap-3 mb-4">
              <div
                class="w-3rem h-3rem border-circle bg-indigo-50 flex align-items-center justify-content-center text-indigo-600 text-xl font-bold font-mono"
              >
                {{ invoice.supplierName?.substring(0, 1) }}
              </div>
              <div class="flex flex-column">
                <span class="font-black text-900">{{
                  invoice.supplierName || 'غير محدد'
                }}</span>
                <span class="text-xs font-bold text-500">مورد معتمد</span>
              </div>
            </div>
            <div class="info-item">
              <span class="label">رقم مرجع المورد:</span>
              <span class="value text-indigo-600"
                >#{{ invoice.supplierInvoiceNumber || '---' }}</span
              >
            </div>
          </div>

          <div class="meta-card">
            <span class="meta-title">الجانب المالي</span>
            <div class="info-item">
              <span class="label">نظام السداد:</span>
              <span
                class="value font-bold"
                [ngClass]="
                  invoice.paymentMethod === 'Cash' ||
                  invoice.paymentMethod === 'نقدي'
                    ? 'text-green-600'
                    : 'text-orange-600'
                "
              >
                {{
                  invoice.paymentMethod === 'Cash' ||
                  invoice.paymentMethod === 'نقدي'
                    ? 'دفع نقدي'
                    : 'دفع آجل'
                }}
              </span>
            </div>
            <div class="info-item">
              <span class="label">صافي الضريبة:</span>
              <span class="value">00.00 ر.ي</span>
            </div>
          </div>

          <div class="meta-card">
            <span class="meta-title">مسار التدقيق</span>
            <div class="info-item">
              <span class="label">المحرر:</span>
              <span class="value">{{
                invoice.createdBy || 'أدمن النظام'
              }}</span>
            </div>
            <div class="info-item">
              <span class="label">وقت التحرير:</span>
              <span class="value text-xs">{{
                invoice.purchaseDate | date : 'HH:mm'
              }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Floating Action Bar -->
      <div class="sticky-actions" *ngIf="invoice">
        <div class="status-section">
          <div
            class="status-indicator"
            [ngClass]="getStatusClass(invoice.status)"
          ></div>
          <span class="status-text">{{ getStatusLabel(invoice.status) }}</span>
        </div>
        <div class="action-btns">
          <p-button
            *ngIf="invoice.status === DocumentStatus.DRAFT"
            label="تعديل الفاتورة"
            icon="pi pi-pencil"
            severity="warning"
            outlined
            (onClick)="editInvoice()"
          ></p-button>
          <p-button
            *ngIf="invoice.status === DocumentStatus.DRAFT"
            label="حذف المسودة"
            icon="pi pi-trash"
            severity="danger"
            outlined
            (onClick)="deleteInvoice()"
          ></p-button>
          <p-button
            *ngIf="invoice.status === DocumentStatus.DRAFT"
            label="اعتماد التوريد والمراجعة"
            icon="pi pi-verified"
            severity="success"
            (onClick)="approveInvoice()"
          ></p-button>

          <p-button
            *ngIf="invoice.status === DocumentStatus.APPROVED"
            label="إرجاع أصناف"
            icon="pi pi-replay"
            severity="warning"
            (onClick)="returnItems()"
          ></p-button>
          <p-button
            *ngIf="invoice.status === DocumentStatus.APPROVED"
            label="إلغاء السند والترحيل"
            icon="pi pi-history"
            severity="danger"
            (onClick)="cancelInvoice()"
          ></p-button>

          <p-button
            label="طباعة النسخة الورقية"
            icon="pi pi-print"
            severity="secondary"
            outlined
          ></p-button>
        </div>
      </div>
    </div>

    <div
      class="flex flex-column align-items-center justify-content-center h-full p-8"
      *ngIf="loading"
    >
      <p-progressSpinner strokeWidth="4"></p-progressSpinner>
      <span class="mt-4 text-3xl font-black text-indigo-900"
        >جاري تحميل سجلات التوريد...</span
      >
    </div>

    <p-confirmDialog [style]="{ width: '550px' }" dir="rtl"></p-confirmDialog>
  `,
  styleUrls: ['./purchase-details.component.scss'],
  providers: [ConfirmationService],
})
export class PurchaseInvoiceDetailsComponent implements OnInit {
  readonly DocumentStatus = DocumentStatus;
  invoice: PurchaseInvoice | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private purchaseService: PurchaseInvoiceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadInvoice(id);
    }
  }

  loadInvoice(id: number) {
    this.loading = true;
    this.purchaseService
      .getById(id)
      .pipe(
        catchError((err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ للنظام',
            detail: 'فشل في استرجاع سجلات التوريد',
          });
          this.loading = false;
          return of(null);
        })
      )
      .subscribe((data) => {
        if (data) {
          this.invoice = data;
          this.loading = false;
        }
      });
  }

  approveInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message:
        'هل أنت متأكد من اعتماد هذا التوريد؟ سيتم زيادة الكميات في المخزون وتفعيل الأصناف للبيع.',
      header: 'تأكيد التوريد',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'تأكيد',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.purchaseService.approve(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'نجاح',
              detail: 'تم التوريد بنجاح والترحيل للمخازن',
            });
            this.loadInvoice(this.invoice!.id);
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  cancelInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message:
        'تحذير: هل أنت متأكد من إلغاء هذا التوريد؟ سيتم سحب الكميات من المخزون.',
      header: 'تأكيد الإلغاء',
      icon: 'pi pi-times-circle',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'نعم، إلغاء',
      rejectLabel: 'تراجع',
      accept: () => {
        this.purchaseService.cancel(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'info',
              summary: 'تم الإلغاء',
              detail: 'تم إلغاء التوريد وعكس الحركات',
            });
            this.loadInvoice(this.invoice!.id);
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  editInvoice() {
    if (!this.invoice) return;
    this.router.navigate(['/purchase-invoices/edit', this.invoice.id]);
  }

  deleteInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message:
        'هل أنت متأكد من حذف هذه المسودة نهائياً؟ لا يمكن التراجع عن هذه العملية.',
      header: 'حذف فاتورة',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.purchaseService.delete(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم الحذف',
              detail: 'تم حذف الفاتورة بنجاح',
            });
            this.router.navigate(['/purchase-invoices']);
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  returnItems() {
    if (!this.invoice) return;
    this.router.navigate(['/purchase-invoices/returns/create'], {
      queryParams: { invoiceId: this.invoice.id },
    });
  }

  handleError(err: any) {
    this.messageService.add({
      severity: 'error',
      summary: 'فشل العملية',
      detail: err.error?.message || 'خطأ في النظام',
    });
  }

  backToList() {
    this.router.navigate(['/purchase-invoices']);
  }

  getStatusLabel(status: any) {
    if (status === undefined || status === null) return 'قيد التدقيق';
    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '1':
      case 'TRUE':
        return 'تم التوريد والمراجعة';
      case 'DRAFT':
      case '0':
        return 'مسودة تكميلية';
      case 'CANCELLED':
      case '2':
        return 'عملية ملغاة';
      default:
        return status;
    }
  }

  getStatusClass(status: any) {
    if (status === undefined || status === null) return 'draft';
    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '1':
      case 'TRUE':
        return 'approved';
      case 'CANCELLED':
      case '2':
        return 'cancelled';
      default:
        return 'draft';
    }
  }
}
