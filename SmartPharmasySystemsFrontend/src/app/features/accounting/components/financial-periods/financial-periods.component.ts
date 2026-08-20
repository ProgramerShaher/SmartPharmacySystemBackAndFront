import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FinancialPeriodService } from '../../services/financial-period.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-financial-periods',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    CalendarModule,
    TagModule,
    ConfirmDialogModule,
    DialogModule,
    ReactiveFormsModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './financial-periods.component.html',
  styleUrls: ['./financial-periods.component.scss']
})
export class FinancialPeriodsComponent implements OnInit {
  periods: any[] = [];
  loading = false;

  createModalVisible = false;
  periodForm: FormGroup;

  constructor(
    private periodService: FinancialPeriodService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private fb: FormBuilder,
    private titleService: Title
  ) {
    this.titleService.setTitle('الفترات المحاسبية - الصيدلية الذكية');
    this.periodForm = this.fb.group({
      periodName: ['', Validators.required],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadPeriods();
  }

  loadPeriods() {
    this.loading = true;
    this.periodService.getAll().subscribe({
      next: (res: any) => {
        this.periods = res.data || res;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الفترات المحاسبية' });
        this.loading = false;
      }
    });
  }

  showCreateModal() {
    this.periodForm.reset();

    // Auto suggest next month based on current date or last period
    const today = new Date();
    const start = new Date(today.getFullYear(), today.getMonth(), 1);
    const end = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    const monthNames = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];

    this.periodForm.patchValue({
      periodName: `شهر ${monthNames[today.getMonth()]} ${today.getFullYear()}`,
      startDate: start,
      endDate: end
    });

    this.createModalVisible = true;
  }

  createPeriod() {
    if (this.periodForm.invalid) return;

    this.loading = true;
    const val = this.periodForm.value;

    const payload = {
      periodName: val.periodName,
      startDate: val.startDate.toISOString().split('T')[0],
      endDate: val.endDate.toISOString().split('T')[0]
    };

    this.periodService.create(payload).subscribe({
      next: (res: any) => {
        if (res.success || !res.message?.includes('Error')) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إنشاء الفترة بنجاح' });
          this.createModalVisible = false;
          this.loadPeriods();
        } else {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message });
        }
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إنشاء الفترة' });
        this.loading = false;
      }
    });
  }

  confirmClosePeriod(period: any) {
    this.confirmationService.confirm({
      message: `هل أنت متأكد من إغلاق الفترة "${period.periodName}"؟ لن يمكن إجراء أي حركة مالية في هذه الفترة بعد الإغلاق.`,
      header: 'تأكيد الإغلاق الشهري',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، إغلاق الفترة',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.closePeriod(period.id);
      }
    });
  }

  closePeriod(id: number) {
    this.loading = true;
    this.periodService.close(id).subscribe({
      next: (res: any) => {
        if (res.success || !res.message?.includes('Error')) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إغلاق الفترة بنجاح' });
          this.loadPeriods();
        } else {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message });
        }
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل إغلاق الفترة' });
        this.loading = false;
      }
    });
  }
}
