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

import { CheckboxModule } from 'primeng/checkbox';

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
    CheckboxModule
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
      message: `هل أنت متأكد من إقفال يوم ${this.selectedDate.toLocaleDateString('ar-SA')}؟ لن يتمكن أي مستخدم من إجراء أو تعديل أي حركة مالية في هذا اليوم بعد الإقفال.`,
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

  transferToMainSafe = false;

  processClosing() {
    this.loading = true;
    this.dailyClosingService.closeDay(this.selectedDate, this.transferToMainSafe).subscribe({
      next: (res: any) => {
        if (res.success || !res.message?.includes('Error')) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الإقفال اليومي بنجاح' });
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

  getStatusSeverity(status: string) {
    switch (status) {
      case 'Approved': return 'success';
      case 'Draft': return 'warning';
      default: return 'info';
    }
  }

  getStatusName(status: string) {
    switch (status) {
      case 'Approved': return 'مكتمل / مقفل';
      case 'Draft': return 'مسودة';
      default: return status;
    }
  }
}
