import { Component, OnInit, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { SaleInvoice, InventoryMovement, DocumentStatus } from '../../../../core/models';
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
  templateUrl: './sales-invoice-details.component.html',
  styleUrls: ['./sales-invoice-details.component.scss'],
  providers: [ConfirmationService],
})
export class SalesInvoiceDetailsComponent implements OnInit {
  readonly DocumentStatus = DocumentStatus;
  invoice: SaleInvoice | null = null;
  loading = true;
  actionDialogVisible = false;
  selectedBatch: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private salesService: SaleInvoiceService,
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
          error: (err: any) => this.handleError(err),
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
          error: (err: any) => this.handleError(err),
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
    this.router.navigate(['/sales']);
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

  getStatusLabel(status: any) {
    if (!status) return 'غير معروف';
    // Check if status is a number (Enum value)
    if (typeof status === 'number') {
      if (status === 2) return 'معتمدة ومرحلة';
      if (status === 1) return 'قيد المراجعة (مسودة)';
      if (status === 3) return 'ملغاة نهائياً';
      if (status === 4) return 'مرتجعة';
    }

    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '2':
        return 'معتمدة ومرحلة';
      case 'DRAFT':
      case '1':
        return 'قيد المراجعة (مسودة)';
      case 'CANCELLED':
      case '3':
        return 'ملغاة نهائياً';
      case 'RETURNED':
      case '4':
        return 'مرتجعة';
      default:
        return status;
    }
  }

  getStatusClass(status: any) {
    if (!status) return 'draft';

    // Check if status is a number
    if (typeof status === 'number') {
      if (status === 2) return 'approved';
      if (status === 1) return 'draft';
      if (status === 3) return 'cancelled';
      if (status === 4) return 'details-badge'; // fallback style
    }

    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '2':
        return 'approved';
      case 'DRAFT':
      case '1':
        return 'draft';
      case 'CANCELLED':
      case '3':
        return 'cancelled';
      case 'RETURNED':
      case '4':
        return 'details-badge';
      default:
        return 'draft';
    }
  }
}
