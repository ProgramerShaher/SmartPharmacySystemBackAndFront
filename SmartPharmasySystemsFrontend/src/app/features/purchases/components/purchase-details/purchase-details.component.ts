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
  templateUrl: './purchase-details.component.html',
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
    this.router.navigate(['/purchases/edit', this.invoice.id]);
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
            this.router.navigate(['/purchases']);
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  returnItems() {
    if (!this.invoice) return;
    this.router.navigate(['/purchases/returns/create'], {
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
    this.router.navigate(['/purchases']);
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

  getStatusSeverity(status: any): any {
    if (status === undefined || status === null) return 'warning';
    const s = status.toString().toUpperCase();
    switch (s) {
      case 'APPROVED':
      case '1':
      case 'TRUE':
        return 'success';
      case 'DRAFT':
      case '0':
        return 'warning';
      case 'CANCELLED':
      case '2':
        return 'danger';
      default:
        return 'info';
    }
  }

  isExpired(date: any): boolean {
    if (!date) return false;
    return new Date(date) < new Date();
  }
}
