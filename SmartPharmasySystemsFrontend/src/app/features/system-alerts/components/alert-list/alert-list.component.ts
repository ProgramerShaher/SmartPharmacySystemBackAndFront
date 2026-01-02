import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToolbarModule } from 'primeng/toolbar';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SystemAlertsService } from '../../services/system-alerts.service';
import {
  Alert,
  AlertQueryDto,
  AlertStatus,
  ExpiryStatus,
  AlertUtils,
} from '../../../../core/models';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-alert-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule,
    DropdownModule,
    ProgressSpinnerModule,
    ToolbarModule
  ],
  templateUrl: './alert-list.component.html',
  styleUrl: './alert-list.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class AlertListComponent implements OnInit {
  alerts: Alert[] = [];
  loading: boolean = true;
  errorMessage: string = '';
  errorDetails: any = null;
  searchTerm: string = '';
  batchNumberFilter: string = '';
  medicineNameFilter: string = '';
  alertTypeFilter: string = '';
  statusFilter: number | '' = ''; // To handle 0 correctly
  today = new Date();

  statusOptions = [
    { label: 'الكل', value: '' },
    { label: 'معلق', value: AlertStatus.Pending },
    { label: 'مقروء', value: AlertStatus.Read },
    { label: 'مرفوض', value: AlertStatus.Dismissed },
    { label: 'مؤرشف', value: AlertStatus.Archived }
  ];

  constructor(
    private systemAlertsService: SystemAlertsService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router,
    private route: ActivatedRoute // Inject ActivatedRoute
  ) { }

  ngOnInit(): void {
    console.log('🚀 Alert List Component Initialized');
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.loading = true;
    console.log('⏳ Loading alerts list...');

    const query: AlertQueryDto = {};
    if (this.searchTerm?.trim()) query.search = this.searchTerm.trim();
    if (this.batchNumberFilter?.trim()) query.batchNumber = this.batchNumberFilter.trim();
    if (this.medicineNameFilter?.trim()) query.medicineName = this.medicineNameFilter.trim();
    if (this.alertTypeFilter?.trim()) query.alertType = this.alertTypeFilter.trim();

    // Handle status filter correctly including 0
    if (this.statusFilter !== '') {
      query.status = +this.statusFilter;
    }

    console.log('📤 Query being sent:', query);

    this.systemAlertsService
      .getAllAlerts(query)
      .pipe(
        catchError((error) => {
          console.error('❌ فشل في تحميل التنبيهات:', error);
          this.errorMessage = this.getErrorMessage(error);
          this.errorDetails = error;
          this.messageService.add({
            severity: 'error',
            summary: 'فشل في تحميل التنبيهات',
            detail: this.errorMessage,
            life: 10000,
          });
          this.alerts = [];
          return of([]);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (result) => {
          console.log('📥 تم استلام النتيجة:', result);
          // Result is Alert[] due to service update
          this.alerts = result || [];
          this.errorMessage = '';
          this.errorDetails = null;
        },
        error: (error) => {
          console.error('💥 خطأ في الاشتراك:', error);
        },
      });
  }

  onSearch(): void {
    this.loadAlerts();
  }

  onStatusFilterChange(): void {
    this.loadAlerts();
  }

  createAlert(): void {
    this.router.navigate(['create'], {
      relativeTo: this.route,
    });
  }

  editAlert(alert: Alert): void {
    this.router.navigate(['edit', alert.id], {
      relativeTo: this.route,
    });
  }

  viewDetails(alert: Alert): void {
    this.router.navigate(['detail', alert.id], {
      relativeTo: this.route,
    });
  }

  markAsRead(alert: Alert): void {
    // If already read, ignore
    if (alert.status === 1 || alert.status === 2) return;

    this.systemAlertsService.markAsRead(alert.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم تحديد التنبيه كمقروء',
        });
        alert.status = 1; // Update UI directly
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في تحديد التنبيه كمقروء',
        });
      },
    });
  }

  deleteAlert(event: Event, alert: Alert): void {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف التنبيه "${alert.message}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.systemAlertsService.delete(alert.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف التنبيه بنجاح',
            });
            this.loadAlerts(); // Reload list
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في عملية الحذف',
            });
          },
        });
      },
    });
  }

  generateExpiryAlerts(): void {
    this.systemAlertsService.generateExpiryAlerts().subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم إنشاء تنبيهات انتهاء الصلاحية',
        });
        this.loadAlerts();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في إنشاء تنبيهات انتهاء الصلاحية',
        });
      },
    });
  }

  generateLowStockAlerts(): void {
    this.systemAlertsService.generateLowStockAlerts().subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم إنشاء تنبيهات نقص المخزون',
        });
        this.loadAlerts();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في إنشاء تنبيهات المخزون',
        });
      },
    });
  }

  // UI Helpers using AlertUtils
  getStatusLabel(status: string | number): string {
    return AlertUtils.getAlertStatusLabel(status);
  }

  getStatusSeverity(status: string | number): "success" | "secondary" | "info" | "warning" | "danger" | "contrast" {
    return AlertUtils.getAlertStatusSeverity(status);
  }

  getAlertTypeIcon(alertType: string): string {
    if (!alertType) return 'pi-bell';

    // Handle specific mappings
    if (alertType.includes('OneWeek') || alertType.includes('TwoWeeks') || alertType === '2') return 'pi-exclamation-triangle';
    if (alertType.includes('Expired') || alertType === '3') return 'pi-times-circle';

    return 'pi-info-circle';
  }

  getRowClass(alert: Alert): string {
    // We can use the utility to get severity color, but usually row class is for full row styling
    // If needed we can return a class based on alert type severity
    const color = AlertUtils.getExpiryStatusColor(alert.alertType);
    // Custom logic if you have CSS classes corresponding to colors
    return '';
  }

  getAlertTypeSeverity(alertType: string): "success" | "info" | "warning" | "danger" | "secondary" | "contrast" | undefined {
    // Map color to severity approximately
    const color = AlertUtils.getExpiryStatusColor(alertType);
    if (color === '#ef4444' || alertType === '3') return 'danger';
    if (color === '#f97316' || color === '#eab308' || alertType === '2') return 'warning';
    return 'info';
  }

  getAlertTypeLabel(type: string): string {
    return AlertUtils.getExpiryStatusLabel(type);
  }

  isExpired(dateStr?: string): boolean {
    if (!dateStr) return false;
    const expiryDate = new Date(dateStr);
    const now = new Date();
    // Reset time part for accurate date comparison if needed, or just compare roughly
    return expiryDate < now;
  }

  showErrorDetails(): void {
    const details = JSON.stringify(this.errorDetails, null, 2);
    alert(`تفاصيل الخطأ الكاملة:\n\n${details}`);
  }

  private getErrorMessage(error: any): string {
    if (error.error && typeof error.error === 'string') return error.error;
    if (error.error?.message) return error.error.message;
    if (error.message) return error.message;
    return 'خطأ غير معروف';
  }
}
