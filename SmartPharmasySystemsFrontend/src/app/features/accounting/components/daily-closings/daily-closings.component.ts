import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FormsModule } from '@angular/forms';
import { DailyClosingService } from '../../services/daily-closing.service';
import { Title } from '@angular/platform-browser';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-daily-closings',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    CalendarModule,
    TagModule,
    ConfirmDialogModule,
    FormsModule,
    ToastModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './daily-closings.component.html',
  styleUrls: ['./daily-closings.component.scss']
})
export class DailyClosingsComponent implements OnInit {
  closings: any[] = [];
  loading = false;
  selectedDate: Date = new Date();

  constructor(
    private dailyClosingService: DailyClosingService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private titleService: Title
  ) {
    this.titleService.setTitle('الإقفالات اليومية - الصيدلية الذكية');
  }

  ngOnInit(): void {
    this.loadClosings();
  }

  loadClosings() {
    this.loading = true;
    this.dailyClosingService.getAll().subscribe({
      next: (res: any) => {
        this.closings = res.data || res;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الإقفالات اليومية' });
        this.loading = false;
      }
    });
  }

  confirmCloseDay() {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من إقفال يوم ${this.selectedDate.toLocaleDateString('ar-SA')}؟
سيتم تلقائياً:
• حساب جميع مبيعات ومصروفات اليوم
• ترحيل رصيد الدرج بالكامل إلى الخزينة الرئيسية
• اعتماد الإقفال فوراً`,
      header: 'تأكيد الإقفال اليومي',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'تأكيد الإقفال',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.processClosing();
      }
    });
  }

  processClosing() {
    this.loading = true;
    // إرسال التاريخ فقط — الـ backend يحسب كل شيء تلقائياً
    this.dailyClosingService.closeDay(this.selectedDate).subscribe({
      next: (res: any) => {
        if (res.success !== false) {
          this.messageService.add({
            severity: 'success',
            summary: 'تم الإقفال',
            detail: 'تم الإقفال اليومي واعتماده بنجاح — رصيد الدرج رُحّل للخزينة الرئيسية'
          });
          this.loadClosings();
        } else {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message || 'حدث خطأ أثناء الإقفال' });
        }
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ غير متوقع' });
        this.loading = false;
      }
    });
  }

  getStatusSeverity(status: any): 'success' | 'secondary' | 'info' | 'warning' | 'danger' | 'contrast' | undefined {
    // status يُرسل من الـ backend كـ integer: Draft=1, PendingApproval=2, Approved=3
    if (status === 3 || status === 'Approved') return 'success';
    if (status === 2 || status === 'PendingApproval') return 'warning';
    if (status === 1 || status === 'Draft') return 'secondary';
    return 'info';
  }

  getStatusName(status: any): string {
    if (status === 3 || status === 'Approved') return 'معتمد';
    if (status === 2 || status === 'PendingApproval') return 'بانتظار الاعتماد';
    if (status === 1 || status === 'Draft') return 'مسودة';
    return String(status);
  }
}
