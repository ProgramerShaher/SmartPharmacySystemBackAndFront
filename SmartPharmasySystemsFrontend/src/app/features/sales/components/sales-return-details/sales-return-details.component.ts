import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { SalesReturnService } from '../../services/sales-return.service';
import { SalesReturn, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-sales-return-details',
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
    ToastModule,
    DialogModule
  ],
  templateUrl: './sales-return-details.component.html',
  styleUrls: ['./sales-return-details.component.scss'],
  providers: [ConfirmationService],
})
export class SalesReturnDetailsComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();

  @Input() invoiceId: number | null = null;
  @Output() onAction = new EventEmitter<void>();

  readonly DocumentStatus = DocumentStatus;
  invoice: SalesReturn | null = null;
  loading = false;

  constructor(
    private returnService: SalesReturnService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['invoiceId'] && this.invoiceId) {
      this.loadInvoice(this.invoiceId);
    }
  }

  loadInvoice(id: number) {
    this.loading = true;
    this.returnService
      .getById(id)
      .pipe(
        catchError((err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ للنظام',
            detail: 'فشل في استرجاع فاتورة المرتجع',
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

  close() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  deleteInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message:
        'هل أنت متأكد من حذف هذا المرتجع نهائياً؟ لا يمكن التراجع عن هذه العملية.',
      header: 'حذف المرتجع',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.returnService.delete(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم الحذف',
              detail: 'تم حذف المرتجع بنجاح',
            });
            this.onAction.emit();
            this.close();
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  approveInvoice() {
    if (!this.invoice) return;
    this.confirmationService.confirm({
      message:
        'هل أنت متأكد من اعتماد هذا المرتجع؟ سيتم استرجاع الكميات إلى المخزون.',
      header: 'تأكيد المرتجع',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'تأكيد',
      rejectLabel: 'إلغاء',
      accept: () => {
        this.returnService.approve(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'نجاح',
              detail: 'تم اعتماد المرتجع بنجاح',
            });
            this.loadInvoice(this.invoice!.id);
            this.onAction.emit();
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
        'تحذير: هل أنت متأكد من إلغاء هذا المرتجع؟ سيتم إلغاء تأثيرات المرتجع من المخزون.',
      header: 'تأكيد الإلغاء',
      icon: 'pi pi-times-circle',
      acceptButtonStyleClass: 'p-button-danger',
      acceptLabel: 'نعم، إلغاء',
      rejectLabel: 'تراجع',
      accept: () => {
        this.returnService.cancel(this.invoice!.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'info',
              summary: 'تم الإلغاء',
              detail: 'تم إلغاء المرتجع وعكس الحركات',
            });
            this.loadInvoice(this.invoice!.id);
            this.onAction.emit();
          },
          error: (err) => this.handleError(err),
        });
      },
    });
  }

  handleError(err: any) {
    this.messageService.add({
      severity: 'error',
      summary: 'فشل العملية',
      detail: err.error?.message || 'خطأ في النظام',
    });
  }

  getStatusLabel(status: any) {
    if (status === undefined || status === null) return 'قيد التدقيق';
    const statusNum = Number(status);

    switch (statusNum) {
      case DocumentStatus.Approved:
        return 'معتمد';
      case DocumentStatus.Draft:
        return 'مسودة';
      case DocumentStatus.Cancelled:
        return 'عملية ملغاة';
      default:
        return status;
    }
  }

  getStatusClass(status: any) {
    if (status === undefined || status === null) return 'draft';
    const statusNum = Number(status);

    switch (statusNum) {
      case DocumentStatus.Approved:
        return 'approved';
      case DocumentStatus.Cancelled:
        return 'cancelled';
      case DocumentStatus.Draft:
        return 'draft';
      default:
        return 'draft';
    }
  }
}
