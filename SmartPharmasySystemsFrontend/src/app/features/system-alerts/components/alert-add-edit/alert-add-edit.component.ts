import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { SystemAlertsService } from '../../services/system-alerts.service';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { CreateAlertDto, UpdateAlertDto, AlertStatus } from '../../../../core/models';

@Component({
  selector: 'app-alert-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    InputTextareaModule,
    CalendarModule,
    DropdownModule,
    ToastModule,
    TagModule
  ],
  templateUrl: './alert-add-edit.component.html',
  styleUrl: './alert-add-edit.component.scss',
  providers: [MessageService],
})
export class AlertAddEditComponent implements OnInit {
  alertForm!: FormGroup;
  isEditMode = false;
  alertId?: number;
  loading = false;
  batches: any[] = [];

  // Use values matching ExpiryStatus enum strings
  alertTypes = [
    { label: 'أسبوع واحد', value: 'ExpiryOneWeek' },
    { label: 'أسبوعين', value: 'ExpiryTwoWeeks' },
    { label: 'شهر واحد', value: 'ExpiryOneMonth' },
    { label: 'شهرين', value: 'ExpiryTwoMonths' },
    { label: 'تنبيه عام', value: 'General' }
  ];

  statusOptions = [
    { label: 'معلق', value: AlertStatus.Pending },
    { label: 'مقروء', value: AlertStatus.Read },
    { label: 'مرفوض', value: AlertStatus.Dismissed },
    { label: 'مؤرشف', value: AlertStatus.Archived }
  ];

  constructor(
    private fb: FormBuilder,
    private systemAlertsService: SystemAlertsService,
    private inventoryService: InventoryService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    console.log('🚀 Alert Add/Edit Component Initialized');
    this.loadBatches();

    this.route.params.subscribe((params) => {
      if (params['id']) {
        this.isEditMode = true;
        this.alertId = +params['id'];
        console.log(`📝 Edit mode for alert ID: ${this.alertId}`);
        this.loadAlert(this.alertId);
      } else {
        console.log('➕ Create mode');
      }
    });
  }

  private initializeForm(): void {
    this.alertForm = this.fb.group({
      batchId: [null, [Validators.required]],
      alertType: ['', Validators.required],
      executionDate: [new Date(), Validators.required],
      expiryDate: [null], // Optional or Required based on user pref, usually optional
      createdAt: [new Date()], // Default to now
      status: [AlertStatus.Pending, Validators.required],
      message: ['', [Validators.required, Validators.minLength(5)]],
    });
  }

  loadBatches(): void {
    this.inventoryService.getAllBatches().subscribe({
      next: (res) => {
        this.batches = res.map(b => ({
          label: `${b.companyBatchNumber} - ${b.medicineName || 'Unknown Medicine'}`,
          value: b.id
        }));
        console.log('📦 Batches loaded:', this.batches.length);
      },
      error: (err) => console.error('❌ Failed to load batches', err)
    });
  }

  loadAlert(id: number): void {
    this.loading = true;
    console.log(`⏳ Loading alert ${id} for editing`);

    this.systemAlertsService.getAlertById(id).subscribe({
      next: (alert) => {
        console.log(`📊 Alert loaded for editing:`, alert);
        // Handle status conversion if string
        let statusVal = alert.status;
        if (typeof alert.status === 'string') {
          if (alert.status === 'Pending') statusVal = AlertStatus.Pending;
          else if (alert.status === 'Read') statusVal = AlertStatus.Read;
          else if (alert.status === 'Dismissed') statusVal = AlertStatus.Dismissed;
          else if (alert.status === 'Archived') statusVal = AlertStatus.Archived;
          else statusVal = parseInt(alert.status) || AlertStatus.Pending;
        }

        this.alertForm.patchValue({
          batchId: alert.batchId,
          alertType: alert.alertType,
          executionDate: alert.executionDate ? new Date(alert.executionDate) : null,
          expiryDate: alert.expiryDate ? new Date(alert.expiryDate) : null,
          createdAt: alert.createdAt ? new Date(alert.createdAt) : new Date(),
          status: statusVal,
          message: alert.message,
        });
        this.loading = false;
      },
      error: (error) => {
        console.error(`❌ Failed to load alert ${id}:`, error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في تحميل بيانات التنبيه',
        });
        this.loading = false;
      },
    });
  }

  onSubmit(): void {
    if (this.alertForm.valid) {
      this.loading = true;
      const formValue = this.alertForm.value;

      if (this.isEditMode && this.alertId) {
        this.updateAlert(this.alertId, formValue);
      } else {
        this.createAlert(formValue);
      }
    } else {
      this.markFormGroupTouched();
      this.messageService.add({
        severity: 'error',
        summary: 'خطأ',
        detail: 'يرجى تصحيح الأخطاء في النموذج',
      });
    }
  }

  private createAlert(data: any): void {
    const createDto: CreateAlertDto = {
      batchId: data.batchId,
      alertType: data.alertType,
      executionDate: data.executionDate ? data.executionDate.toISOString() : new Date().toISOString(),
      message: data.message,
      expiryDate: data.expiryDate ? data.expiryDate.toISOString() : undefined,
      createdAt: data.createdAt ? data.createdAt.toISOString() : undefined,
      status: data.status
    };

    console.log('➕ Creating new alert:', createDto);

    this.systemAlertsService.createAlert(createDto).subscribe({
      next: (alert) => {
        console.log('✅ Alert created successfully:', alert);
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم إنشاء التنبيه بنجاح',
        });
        this.loading = false;
        this.router.navigate(['/system-alerts']);
      },
      error: (error) => {
        console.error('❌ Failed to create alert:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في إنشاء التنبيه',
        });
        this.loading = false;
      },
    });
  }

  private updateAlert(id: number, data: any): void {
    const updateDto: UpdateAlertDto = {
      id: id,
      batchId: data.batchId,
      alertType: data.alertType,
      executionDate: data.executionDate ? data.executionDate.toISOString() : undefined,
      message: data.message,
      expiryDate: data.expiryDate ? data.expiryDate.toISOString() : undefined,
      createdAt: data.createdAt ? data.createdAt.toISOString() : undefined,
      status: data.status
    };

    console.log(`✏️ Updating alert ${id}:`, updateDto);

    this.systemAlertsService.updateAlert(id, updateDto).subscribe({
      next: (alert) => {
        console.log('✅ Alert updated successfully:', alert);
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم تحديث التنبيه بنجاح',
        });
        this.loading = false;
        this.router.navigate(['/system-alerts']);
      },
      error: (error) => {
        console.error(`❌ Failed to update alert ${id}:`, error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في تحديث التنبيه',
        });
        this.loading = false;
      },
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.alertForm.controls).forEach((key) => {
      const control = this.alertForm.get(key);
      control?.markAsTouched();
    });
  }

  goBack(): void {
    console.log('↩️ Going back to alerts list');
    this.router.navigate(['/system-alerts']);
  }

  get formTitle(): string {
    return this.isEditMode ? 'تعديل التنبيه' : 'إضافة تنبيه جديد';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'تحديث التنبيه' : 'إنشاء التنبيه';
  }
}
