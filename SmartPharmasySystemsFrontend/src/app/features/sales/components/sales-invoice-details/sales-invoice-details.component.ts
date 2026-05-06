import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SaleInvoice, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { PrintService } from '../../../../core/services/print.service';

@Component({
  selector: 'app-sales-invoice-details',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    ConfirmDialogModule,
    TooltipModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './sales-invoice-details.component.html',
  styleUrls: ['./sales-invoice-details.component.scss'],
  providers: [ConfirmationService],
})
export class SalesInvoiceDetailsComponent implements OnInit {
  readonly DocumentStatus = DocumentStatus;
  invoice: SaleInvoice | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private salesService: SaleInvoiceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private printService: PrintService
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
          console.error('--- ERP DETAILS LOAD ERROR ---', err);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ في النظام',
            detail: 'فشل في استرجاع بيانات الفاتورة، يرجى التحقق من الاتصال',
          });
          this.loading = false;
          return of(null);
        })
      )
      .subscribe((data) => {
        if (data) {
          this.invoice = data;
        }
        this.loading = false;
      });
  }

  approveInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من اعتماد الفاتورة؟ سيتم ترحيل العملية وخصم الكميات من المخزون.',
      header: 'تأكيد اعتماد الفاتورة',
      icon: 'pi pi-check-circle',
      acceptLabel: 'اعتماد',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.salesService.approve(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم الاعتماد',
              detail: 'تم اعتماد الفاتورة وتحديث قيود المخزون',
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
      message: 'هل أنت متأكد من إلغاء الفاتورة؟ سيتم عكس الحركات المرتبطة بها.',
      header: 'تأكيد إلغاء الفاتورة',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger p-button-raised',
      acceptLabel: 'إلغاء الفاتورة',
      rejectLabel: 'تراجع',
      accept: () => {
        this.salesService.cancel(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'warn',
              summary: 'تم الإلغاء',
              detail: 'تم إلغاء الفاتورة وعكس الحركات المخزنية',
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

  backToList() {
    this.router.navigate(['/sales']);
  }

  printInvoice() {
    if (!this.invoice) return;
    this.printService.printInvoice(this.invoice.id);
  }

  getStatusSeverity(status: string) {
    if (!status) return 'info';
    switch (status.toUpperCase()) {
      case 'APPROVED':
      case '2':
        return 'success';
      case 'DRAFT':
      case '1':
        return 'warning';
      case 'CANCELLED':
      case '3':
        return 'danger';
      default:
        return 'info';
    }
  }

  getStatusLabel(status: any) {
    if (!status) return 'غير معروف';

    if (typeof status === 'number') {
      if (status === 2) return 'معتمدة';
      if (status === 1) return 'مسودة';
      if (status === 3) return 'ملغاة';
      if (status === 4) return 'مرتجعة';
    }

    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '2':
        return 'معتمدة';
      case 'DRAFT':
      case '1':
        return 'مسودة';
      case 'CANCELLED':
      case '3':
        return 'ملغاة';
      case 'RETURNED':
      case '4':
        return 'مرتجعة';
      default:
        return status;
    }
  }

  getPaymentMethodLabel(paymentMethod: any) {
    const method = paymentMethod?.toString().toUpperCase();
    if (paymentMethod === 1 || method === 'CASH') return 'نقدي';
    if (paymentMethod === 2 || method === 'CREDIT') return 'آجل';
    return paymentMethod || 'غير محدد';
  }

  getStatusClass(status: any) {
    if (!status) return 'draft';

    if (typeof status === 'number') {
      if (status === 2) return 'approved';
      if (status === 1) return 'draft';
      if (status === 3) return 'cancelled';
      if (status === 4) return 'details-badge';
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
