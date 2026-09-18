import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { QuotationService } from '../../services/quotation.service';
import { Quotation, QuotationStatus, ConvertQuotationToInvoiceDto } from '../../models/quotation.interface';

@Component({
  selector: 'app-quotation-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, TableModule, ButtonModule,
    TagModule, ToastModule, ConfirmDialogModule, DialogModule,
    InputTextModule, DropdownModule, TooltipModule, InputNumberModule,
    CalendarModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './quotation-list.component.html',
  styleUrls: ['./quotation-list.component.scss']
})
export class QuotationListComponent implements OnInit {
  private quotationService = inject(QuotationService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private router = inject(Router);

  quotations = signal<Quotation[]>([]);
  totalCount = signal<number>(0);
  loading = signal<boolean>(false);

  // Filters
  searchTerm: string = '';
  selectedStatus: number | null = null;
  dateRange: Date[] | null = null;
  page: number = 1;
  pageSize: number = 20;

  // Status Enum
  QuotationStatus = QuotationStatus;

  statusOptions = [
    { label: 'الكل', value: null },
    { label: 'مسودة', value: QuotationStatus.Draft },
    { label: 'مرسل للعميل', value: QuotationStatus.Sent },
    { label: 'مقبول', value: QuotationStatus.Accepted },
    { label: 'مرفوض', value: QuotationStatus.Rejected },
    { label: 'منتهي الصلاحية', value: QuotationStatus.Expired },
    { label: 'محول لفاتورة', value: QuotationStatus.ConvertedToInvoice }
  ];

  // Convert to Invoice Dialog
  showConvertDialog = signal<boolean>(false);
  converting = signal<boolean>(false);
  selectedQuotation: Quotation | null = null;

  convertDto: ConvertQuotationToInvoiceDto = {
    paymentMethod: 0, // Cash
    paidAmount: 0,
    notes: ''
  };

  paymentMethods = [
    { label: 'نقدي (Cash)', value: 0 },
    { label: 'شبكة / بطاقة (Card)', value: 1 },
    { label: 'آجل / ذمم (Credit)', value: 2 },
    { label: 'تحويل بنكي (Transfer)', value: 3 }
  ];

  ngOnInit(): void {
    this.loadQuotations();
  }

  loadQuotations(): void {
    this.loading.set(true);
    const query: any = {
      page: this.page,
      pageSize: this.pageSize
    };

    if (this.searchTerm?.trim()) query.search = this.searchTerm.trim();
    if (this.selectedStatus !== null) query.status = this.selectedStatus;
    if (this.dateRange && this.dateRange[0]) query.dateFrom = this.dateRange[0].toISOString();
    if (this.dateRange && this.dateRange[1]) query.dateTo = this.dateRange[1].toISOString();

    this.quotationService.getPaged(query).subscribe({
      next: (res) => {
        this.quotations.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'تعذر تحميل قائمة عروض الأسعار'
        });
      }
    });
  }

  onFilterChange(): void {
    this.page = 1;
    this.loadQuotations();
  }

  onPageChange(event: any): void {
    this.page = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadQuotations();
  }

  getStatusSeverity(status: QuotationStatus): string {
    switch (status) {
      case QuotationStatus.Draft: return 'secondary';
      case QuotationStatus.Sent: return 'info';
      case QuotationStatus.Accepted: return 'success';
      case QuotationStatus.Rejected: return 'danger';
      case QuotationStatus.Expired: return 'warning';
      case QuotationStatus.ConvertedToInvoice: return 'help';
      default: return 'info';
    }
  }

  openConvertDialog(quotation: Quotation): void {
    this.selectedQuotation = quotation;
    this.convertDto = {
      paymentMethod: 0,
      paidAmount: quotation.totalAmount,
      notes: `تحويل من عرض السعر رقم ${quotation.quotationNumber}`
    };
    this.showConvertDialog.set(true);
  }

  onPaymentMethodChange(): void {
    if (!this.selectedQuotation) return;
    if (this.convertDto.paymentMethod === 2) {
      // Credit
      this.convertDto.paidAmount = 0;
    } else {
      this.convertDto.paidAmount = this.selectedQuotation.totalAmount;
    }
  }

  submitConversion(): void {
    if (!this.selectedQuotation) return;
    this.converting.set(true);

    this.quotationService.convertToInvoice(this.selectedQuotation.id, this.convertDto).subscribe({
      next: (res) => {
        this.converting.set(false);
        this.showConvertDialog.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح التحويل',
          detail: `تم تحويل عرض السعر إلى فاتورة مبيعات بنجاح رقم: ${res.saleInvoiceNumber || res.id}`
        });
        this.loadQuotations();
      },
      error: (err) => {
        this.converting.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'فشل التحويل',
          detail: err.error?.message || 'حدث خطأ أثناء تحويل عرض السعر إلى فاتورة'
        });
      }
    });
  }

  changeStatus(quotation: Quotation, newStatus: QuotationStatus): void {
    this.quotationService.changeStatus(quotation.id, newStatus).subscribe({
      next: (updated) => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم التحديث',
          detail: `تم تحديث حالة عرض السعر رقم ${quotation.quotationNumber}`
        });
        this.loadQuotations();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: err.error?.message || 'فشل تحديث الحالة'
        });
      }
    });
  }

  deleteQuotation(quotation: Quotation): void {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف عرض السعر رقم ${quotation.quotationNumber}؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.quotationService.delete(quotation.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم الحذف',
              detail: 'تم حذف عرض السعر بنجاح'
            });
            this.loadQuotations();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: err.error?.message || 'تعذر حذف عرض السعر'
            });
          }
        });
      }
    });
  }

  viewDetails(id: number): void {
    this.router.navigate(['/sales/quotations', id]);
  }

  editQuotation(id: number): void {
    this.router.navigate(['/sales/quotations/edit', id]);
  }

  createNew(): void {
    this.router.navigate(['/sales/quotations/create']);
  }
}
