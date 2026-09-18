import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { QuotationService } from '../../services/quotation.service';
import { Quotation, QuotationPrintData, QuotationStatus, ConvertQuotationToInvoiceDto } from '../../models/quotation.interface';

@Component({
  selector: 'app-quotation-details',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, ButtonModule, TagModule,
    TableModule, ToastModule, DialogModule, DropdownModule,
    InputNumberModule, InputTextModule, DividerModule
  ],
  providers: [MessageService],
  templateUrl: './quotation-details.component.html',
  styleUrls: ['./quotation-details.component.scss']
})
export class QuotationDetailsComponent implements OnInit {
  private quotationService = inject(QuotationService);
  private messageService = inject(MessageService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  quotationId = signal<number>(0);
  quotation = signal<Quotation | null>(null);
  printData = signal<QuotationPrintData | null>(null);
  loading = signal<boolean>(false);

  QuotationStatus = QuotationStatus;

  // Convert Dialog
  showConvertDialog = signal<boolean>(false);
  converting = signal<boolean>(false);

  convertDto: ConvertQuotationToInvoiceDto = {
    paymentMethod: 0,
    paidAmount: 0,
    notes: ''
  };

  paymentMethods = [
    { label: 'نقدي (Cash)', value: 0 },
    { label: 'شبكة / بطاقة (Card)', value: 1 },
    { label: 'آجل / ذمم (Credit)', value: 2 },
    { label: 'تحويل بنكي (Transfer)', value: 3 }
  ];

  statusOptions = [
    { label: 'مسودة', value: QuotationStatus.Draft },
    { label: 'مرسل للعميل', value: QuotationStatus.Sent },
    { label: 'مقبول', value: QuotationStatus.Accepted },
    { label: 'مرفوض', value: QuotationStatus.Rejected },
    { label: 'منتهي الصلاحية', value: QuotationStatus.Expired }
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.quotationId.set(+id);
      this.loadData(+id);
    }
  }

  loadData(id: number): void {
    this.loading.set(true);
    this.quotationService.getById(id).subscribe({
      next: (data) => {
        this.quotation.set(data);
        this.quotationService.getPrintData(id).subscribe({
          next: (pData) => {
            this.printData.set(pData);
            this.loading.set(false);
          },
          error: () => { this.loading.set(false); }
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل بيانات عرض السعر'
        });
      }
    });
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

  printA4(): void {
    window.print();
  }

  openConvertDialog(): void {
    const q = this.quotation();
    if (!q) return;

    this.convertDto = {
      paymentMethod: 0,
      paidAmount: q.totalAmount,
      notes: `تحويل من عرض السعر رقم ${q.quotationNumber}`
    };
    this.showConvertDialog.set(true);
  }

  onPaymentMethodChange(): void {
    const q = this.quotation();
    if (!q) return;
    if (this.convertDto.paymentMethod === 2) {
      this.convertDto.paidAmount = 0;
    } else {
      this.convertDto.paidAmount = q.totalAmount;
    }
  }

  submitConversion(): void {
    const q = this.quotation();
    if (!q) return;

    this.converting.set(true);
    this.quotationService.convertToInvoice(q.id, this.convertDto).subscribe({
      next: (res) => {
        this.converting.set(false);
        this.showConvertDialog.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح التحويل',
          detail: `تم تحويل عرض السعر إلى فاتورة مبيعات بنجاح رقم: ${res.saleInvoiceNumber || res.id}`
        });
        this.loadData(q.id);
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

  changeStatus(newStatus: QuotationStatus): void {
    const q = this.quotation();
    if (!q) return;

    this.quotationService.changeStatus(q.id, newStatus).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم التحديث',
          detail: 'تم تحديث حالة عرض السعر بنجاح'
        });
        this.loadData(q.id);
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

  goBack(): void {
    this.router.navigate(['/sales/quotations']);
  }

  editQuotation(): void {
    this.router.navigate(['/sales/quotations/edit', this.quotationId()]);
  }
}
