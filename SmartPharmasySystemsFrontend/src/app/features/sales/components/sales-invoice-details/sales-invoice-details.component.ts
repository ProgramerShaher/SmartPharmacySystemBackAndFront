import { Component, OnInit, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { SalesInvoiceService } from '../../services/sales-invoice.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { SaleInvoice, InventoryMovement } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SaleInvoiceActionsDialogComponent } from '../sale-invoice-actions-dialog/sale-invoice-actions-dialog.component';
import { StockCardLiteComponent } from '../../../../shared/components/stock-card-lite/stock-card-lite.component';

@Component({
  selector: 'app-sales-invoice-details',
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
    <div
      class="flex flex-column h-full overflow-hidden"
      dir="rtl"
      *ngIf="invoice"
    >
      <!-- ERP Header -->
      <div class="erp-header">
        <div class="flex justify-content-between align-items-center">
          <div class="flex align-items-center gap-4">
            <p-button
              icon="pi pi-arrow-right"
              rounded
              text
              styleClass="text-white hover:bg-white-alpha-20 font-bold"
              (onClick)="backToList()"
            ></p-button>
            <div class="flex flex-column">
              <h2 class="m-0 text-3xl font-extrabold line-height-2">
                تفاصيل فاتورة مبيعات
              </h2>
              <div class="flex align-items-center gap-3 mt-1">
                <span
                  class="status-badge"
                  [ngClass]="invoice.status.toLowerCase()"
                  >{{ getStatusLabel(invoice.status) }}</span
                >
                <span class="text-white-alpha-80 font-bold"
                  >#{{ invoice.saleInvoiceNumber || invoice.id }}</span
                >
                <span class="text-white-alpha-60">|</span>
                <span class="text-white-alpha-80">{{
                  invoice.saleInvoiceDate | date : 'dd MMMM yyyy HH:mm'
                }}</span>
              </div>
            </div>
          </div>
          <div class="flex gap-3">
            <p-button
              *ngIf="invoice.status === 'DRAFT'"
              label="اعتماد وترحيل"
              icon="pi pi-verified"
              severity="success"
              (onClick)="approveInvoice()"
              styleClass="p-button-raised"
            ></p-button>
            <p-button
              *ngIf="invoice.status === 'APPROVED'"
              label="إلغاء الفاتورة"
              icon="pi pi-undo"
              severity="danger"
              (onClick)="cancelInvoice()"
              styleClass="p-button-raised"
            ></p-button>
            <p-button
              label="طباعة حرارية"
              icon="pi pi-print"
              severity="secondary"
              outlined
              styleClass="text-white border-white-alpha-30"
            ></p-button>
          </div>
        </div>
      </div>

      <div class="flex-grow-1 overflow-auto p-4 bg-gray-50">
        <div class="grid">
          <!-- Stats Section -->
          <div class="col-12 lg:col-8">
            <p-card styleClass="financial-card mb-4 h-full">
              <div class="flex justify-content-around text-center py-3">
                <div>
                  <span class="stat-label">المبلغ الإجمالي</span>
                  <span class="stat-value text-4xl text-primary"
                    >{{ invoice.totalAmount | number : '1.2-2' }} ريال
                    يمني</span
                  >
                </div>
                <p-divider layout="vertical"></p-divider>
                <div>
                  <span class="stat-label">إجمالي التكلفة</span>
                  <span class="stat-value text-2xl text-600"
                    >{{ invoice.totalCost | number : '1.2-2' }} ريال يمني</span
                  >
                </div>
                <p-divider layout="vertical"></p-divider>
                <div>
                  <span class="stat-label">صافي الربح</span>
                  <span class="stat-value text-3xl text-green-600"
                    >{{ invoice.totalProfit | number : '1.2-2' }} ريال
                    يمني</span
                  >
                </div>
              </div>
            </p-card>
          </div>

          <div class="col-12 lg:col-4">
            <p-card styleClass="h-full border-1 border-gray-200">
              <div class="flex flex-column gap-3">
                <div
                  class="flex justify-content-between align-items-center border-bottom-1 border-gray-100 pb-2"
                >
                  <span class="text-secondary font-bold">اسم العميل:</span>
                  <span class="font-extrabold text-primary">{{
                    invoice.customerName || 'عميل نقدي'
                  }}</span>
                </div>
                <div
                  class="flex justify-content-between align-items-center border-bottom-1 border-gray-100 pb-2"
                >
                  <span class="text-secondary font-bold">طريقة السداد:</span>
                  <span
                    class="font-bold border-1 border-blue-200 bg-blue-50 px-3 py-1 border-round-pill text-blue-700"
                  >
                    {{
                      invoice.paymentMethod === 'Cash'
                        ? 'نقدي (Cash)'
                        : 'آجل (On Credit)'
                    }}
                  </span>
                </div>
                <div class="flex justify-content-between align-items-center">
                  <span class="text-secondary font-bold">المحاسب المسؤول:</span>
                  <span class="text-600">{{
                    invoice.createdBy || 'النظام المركزي'
                  }}</span>
                </div>
              </div>
            </p-card>
          </div>

          <!-- Items Table -->
          <div class="col-12 mt-2">
            <p-card header="بنود الفاتورة وحركة المخزون">
              <p-table
                [value]="invoice.saleInvoiceDetails || []"
                dataKey="id"
                styleClass="p-datatable-gridlines p-datatable-sm shadow-1 border-round-lg overflow-hidden"
              >
                <ng-template pTemplate="header">
                  <tr>
                    <th style="width: 3rem"></th>
                    <th class="text-right">اسم الدواء / الصنف</th>
                    <th class="text-center" style="width: 140px">رقم الدفعة</th>
                    <th class="text-center" style="width: 100px">الكمية</th>
                    <th class="text-right" style="width: 150px">سعر البيع</th>
                    <th class="text-right" style="width: 150px">الإجمالي</th>
                    <th class="text-right" style="width: 120px">الربح</th>
                    <th class="text-center" style="width: 80px">خيارات</th>
                  </tr>
                </ng-template>
                <ng-template
                  pTemplate="body"
                  let-detail
                  let-expanded="expanded"
                >
                  <tr
                    [pRowToggler]="detail"
                    class="cursor-pointer hover:surface-50"
                  >
                    <td (click)="$event.stopPropagation()">
                      <button
                        type="button"
                        pButton
                        pRipple
                        [pRowToggler]="detail"
                        class="p-button-text p-button-rounded p-button-plain"
                        [icon]="
                          expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-left'
                        "
                      ></button>
                    </td>
                    <td class="font-bold text-primary">
                      {{ detail.medicineName }}
                    </td>
                    <td class="text-center">
                      <span
                        class="font-mono text-indigo-700 bg-indigo-50 px-2 py-1 border-round"
                        >{{ detail.companyBatchNumber }}</span
                      >
                    </td>
                    <td class="text-center font-bold text-xl">
                      {{ detail.quantity }}
                    </td>
                    <td class="text-right font-semibold">
                      {{ detail.salePrice | number : '1.2-2' }}
                    </td>
                    <td class="text-right font-extrabold text-indigo-800">
                      {{ detail.totalLineAmount | number : '1.2-2' }}
                    </td>
                    <td class="text-right text-green-600 font-bold">
                      {{ detail.profit | number : '1.2-2' }}
                    </td>
                    <td class="text-center" (click)="$event.stopPropagation()">
                      <p-button
                        icon="pi pi-cog"
                        rounded
                        text
                        severity="secondary"
                        pTooltip="تسوية مخزنية يدوية"
                        (onClick)="openActionDialog(detail)"
                        [disabled]="invoice.status === 'CANCELLED'"
                      ></p-button>
                    </td>
                  </tr>
                </ng-template>
                <ng-template pTemplate="rowexpansion" let-detail>
                  <tr>
                    <td colspan="8" class="p-0">
                      <div
                        class="p-4 bg-gray-50 border-y-1 border-gray-200 shadow-inner"
                      >
                        <div
                          class="flex align-items-center gap-2 mb-3 text-secondary font-bold"
                        >
                          <i class="pi pi-history text-primary"></i>
                          <span>كارت حركة الصنف - الدفعة المحددة</span>
                        </div>
                        <app-stock-card-lite
                          [batchId]="detail.batchId"
                        ></app-stock-card-lite>
                      </div>
                    </td>
                  </tr>
                </ng-template>
              </p-table>
            </p-card>
          </div>

          <div class="col-12 mt-2" *ngIf="invoice.notes">
            <p-card
              header="ملاحظات إضافية"
              styleClass="border-1 border-gray-200 shadow-none"
            >
              <p class="m-0 text-700 line-height-3">{{ invoice.notes }}</p>
            </p-card>
          </div>
        </div>
      </div>
    </div>

    <div
      class="flex flex-column align-items-center justify-content-center h-full p-8"
      *ngIf="loading"
    >
      <p-progressSpinner strokeWidth="4"></p-progressSpinner>
      <span class="mt-4 text-2xl font-bold text-primary"
        >جاري استرجاع تفاصيل الفاتورة من النظام اليمني...</span
      >
    </div>

    <p-confirmDialog [style]="{ width: '500px' }" dir="rtl"></p-confirmDialog>

    <app-sale-invoice-actions-dialog
      [visible]="actionDialogVisible"
      [batch]="selectedBatch"
      (onHide)="actionDialogVisible = false"
      (onComplete)="loadInvoice(invoice?.id!)"
    >
    </app-sale-invoice-actions-dialog>
  `,
  styleUrls: ['./sales-invoice-details.component.scss'],
  providers: [ConfirmationService],
})
export class SalesInvoiceDetailsComponent implements OnInit {
  invoice: SaleInvoice | null = null;
  loading = true;
  actionDialogVisible = false;
  selectedBatch: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private salesService: SalesInvoiceService,
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
    this.salesService
      .getById(id)
      .pipe(
        catchError((err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ للنظام',
            detail: 'فشل في استرجاع بيانات الفاتورة، يرجى التحقق من الاتصال',
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
        'هل أنت متأكد من اعتماد الفاتورة؟ سيتم ترحيل البيانات فوراً وخصم المخزون.',
      header: 'تأكيد ترحيل الفاتورة',
      icon: 'pi pi-check-circle',
      acceptLabel: 'تأكيد الترحيل',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.salesService.approve(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم الترحيل بنجاح',
              detail: 'تم اعتماد الفاتورة وتحديث قيود المخزن',
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
        'تحذير: هل أنت متأكد من إلغاء الفاتورة بالكامل؟ سيتم عكس كافة الحركات المترتبة عليها.',
      header: 'تأكيد إلغاء الفاتورة',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger p-button-raised',
      acceptLabel: 'نعم، إلغاء نهائي',
      rejectLabel: 'تراجع',
      accept: () => {
        this.salesService.cancel(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'warn',
              summary: 'تم الإلغاء',
              detail: 'تم إلغاء الفاتورة بنجاح وعكس الحركات المخزنية',
            });
            this.loadInvoice(this.invoice!.id);
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  handleError(err: any) {
    console.error('--- ERP DETAILS ERROR ---', err);
    this.messageService.add({
      severity: 'error',
      summary: 'فشل في معالجة الطلب',
      detail: err.error?.message || 'خطأ غير متوقع في الخادم',
    });
  }

  openActionDialog(detail: any) {
    this.selectedBatch = {
      id: detail.batchId,
      companyBatchNumber: detail.companyBatchNumber,
      remainingQuantity: detail.quantity,
    };
    this.actionDialogVisible = true;
  }

  backToList() {
    this.router.navigate(['/sales-invoices']);
  }

  getStatusSeverity(status: string) {
    if (!status) return 'info';
    switch (status.toUpperCase()) {
      case 'APPROVED':
        return 'success';
      case 'DRAFT':
        return 'warning';
      case 'CANCELLED':
        return 'danger';
      default:
        return 'info';
    }
  }

  getStatusLabel(status: string) {
    if (!status) return 'غير معروف';
    switch (status.toUpperCase()) {
      case 'APPROVED':
        return 'معتمدة ومرحلة';
      case 'DRAFT':
        return 'قيد المراجعة (مسودة)';
      case 'CANCELLED':
        return 'ملغاة نهائياً';
      default:
        return status;
    }
  }
}
