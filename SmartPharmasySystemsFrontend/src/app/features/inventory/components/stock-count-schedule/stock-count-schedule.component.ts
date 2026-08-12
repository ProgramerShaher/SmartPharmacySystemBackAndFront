import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DialogModule } from 'primeng/dialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputSwitchModule } from 'primeng/inputswitch';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { StockCountService } from '../../services/stock-count.service';
import { WarehouseService } from '../../../warehouses/services/warehouse.service';
import { MessageService } from 'primeng/api';
import {
  StockCountScheduleDto,
  StockCountFrequency,
  CreateStockCountScheduleDto
} from '../../../../core/models/stock-count.interface';

@Component({
  selector: 'app-stock-count-schedule',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    DropdownModule,
    CalendarModule,
    InputTextareaModule,
    DialogModule,
    ProgressSpinnerModule,
    InputSwitchModule,
    ButtonModule,
    TableModule,
    ToastModule,
    TooltipModule
  ],
  providers: [MessageService],
  templateUrl: './stock-count-schedule.component.html',
  styleUrls: ['./stock-count-schedule.component.scss']
})
export class StockCountScheduleComponent implements OnInit {
  private readonly stockCountService = inject(StockCountService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly fb = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  schedules: StockCountScheduleDto[] = [];
  warehouses: any[] = [];
  loading = false;
  displayDialog = false;
  scheduleForm!: FormGroup;
  minDate: Date = new Date();

  frequencyOptions = [
    { label: 'يومي', value: StockCountFrequency.Daily },
    { label: 'أسبوعي', value: StockCountFrequency.Weekly },
    { label: 'شهري', value: StockCountFrequency.Monthly },
    { label: 'ربع سنوي', value: StockCountFrequency.Quarterly },
    { label: 'نصف سنوي', value: StockCountFrequency.SemiAnnually },
    { label: 'سنوي', value: StockCountFrequency.Annually }
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadWarehouses();
    this.loadSchedules();
  }

  initForm(): void {
    this.scheduleForm = this.fb.group({
      warehouseId: [null, Validators.required],
      frequency: [StockCountFrequency.Monthly, Validators.required],
      nextRunDate: [null, Validators.required],
      notes: ['']
    });
  }

  loadWarehouses(): void {
    this.warehouseService.getAll().subscribe({
      next: (data: any) => {
        this.warehouses = data;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المستودعات' });
      }
    });
  }

  loadSchedules(): void {
    this.loading = true;
    this.stockCountService.getAllSchedules().subscribe({
      next: (data: any) => {
        this.schedules = data;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل جداول الجرد' });
        this.loading = false;
      }
    });
  }

  showAddDialog(): void {
    this.scheduleForm.reset({
      frequency: StockCountFrequency.Monthly,
      nextRunDate: new Date()
    });
    this.displayDialog = true;
  }

  hideDialog(): void {
    this.displayDialog = false;
  }

  saveSchedule(): void {
    if (this.scheduleForm.invalid) return;

    const dto: CreateStockCountScheduleDto = this.scheduleForm.value;

    this.stockCountService.createSchedule(dto).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الجدولة بنجاح' });
        this.hideDialog();
        this.loadSchedules();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحفظ' });
      }
    });
  }

  deleteSchedule(id: number): void {
    if (confirm('هل أنت متأكد من حذف الجدولة الآلية؟')) {
      this.stockCountService.deleteSchedule(id).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الحذف' });
          this.loadSchedules();
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' });
        }
      });
    }
  }

  toggleActive(schedule: StockCountScheduleDto): void {
    this.stockCountService.updateSchedule(schedule.id, {
      frequency: schedule.frequency,
      nextRunDate: schedule.nextRunDate,
      isActive: schedule.isActive,
      notes: schedule.notes
    }).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الحالة' });
      },
      error: () => {
        schedule.isActive = !schedule.isActive;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التحديث' });
      }
    });
  }

  getActiveCount(): number {
    return this.schedules.filter(schedule => schedule.isActive).length;
  }

  getInactiveCount(): number {
    return this.schedules.length - this.getActiveCount();
  }

  getNextRunDate(): string | null {
    const next = this.schedules
      .filter(schedule => schedule.isActive && schedule.nextRunDate)
      .map(schedule => new Date(schedule.nextRunDate))
      .sort((a, b) => a.getTime() - b.getTime())[0];

    return next ? next.toLocaleDateString('ar-SA') : null;
  }
}
