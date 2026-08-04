import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SystemAlertsService } from '../../services/system-alerts.service';
import { Alert, AlertUtils } from '../../../../core/models';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ProgressSpinnerModule } from "primeng/progressspinner";

@Component({
  selector: 'app-alert-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    CardModule,
    TagModule,
    ChipModule,
    DividerModule,
    ConfirmDialogModule,
    ToastModule,
    TooltipModule,
    ProgressSpinnerModule
],
  templateUrl: './alert-detail.component.html',
  styleUrl: './alert-detail.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class AlertDetailComponent implements OnInit {
  @Input() alertId?: number | null = null;
  @Input() alertData?: Alert | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() edit = new EventEmitter<Alert>();

  alert?: Alert;
  loading: boolean = true;
  isModalMode = false;

  constructor(
    private systemAlertsService: SystemAlertsService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    console.log('🚀 Alert Detail Component Initialized');
    if (this.alertData) {
      this.isModalMode = true;
      this.alert = this.alertData;
      this.loading = false;
    } else if (this.alertId) {
      this.isModalMode = true;
      this.loadAlert(this.alertId);
    } else {
      this.route.params.subscribe((params) => {
        if (params['id']) {
          const id = +params['id'];
          console.log(`📋 Loading details for alert ID: ${id}`);
          this.loadAlert(id);
        }
      });
    }
  }

  loadAlert(id: number): void {
    this.loading = true;
    console.log(`⏳ Fetching alert details for ID: ${id}`);

    this.systemAlertsService
      .getAlertById(id)
      .pipe(
        catchError((error) => {
          console.error(`❌ Failed to load alert details for ID ${id}:`, error);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل في تحميل بيانات التنبيه',
          });
          this.loading = false;
          return of(undefined);
        })
      )
      .subscribe({
        next: (alert) => {
          this.alert = alert;
          console.log(`📊 Alert details loaded:`, alert);
          this.loading = false;
        },
      });
  }

  markAsRead(): void {
    if (!this.alert || this.alert.status === 1 || this.alert.status === 2) {
      return;
    }

    console.log(`👁️ Marking alert ${this.alert.id} as read`);

    this.systemAlertsService.markAsRead(this.alert.id).subscribe({
      next: () => {
        console.log(`✅ Alert ${this.alert!.id} marked as read`);
        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: 'تم تحديد التنبيه كمقروء',
        });
        // Update local status
        if (this.alert) {
          this.alert.status = 1;
        }
      },
      error: (err) => {
        console.error(
          `❌ Failed to mark alert ${this.alert!.id} as read:`,
          err
        );
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل في تحديد التنبيه كمقروء',
        });
      },
    });
  }

  editAlert(): void {
    if (this.alert) {
      console.log(`✏️ Navigating/Emitting edit for alert ID: ${this.alert.id}`);
      this.edit.emit(this.alert);
      if (!this.isModalMode) {
        this.router.navigate(['/system-alerts/edit', this.alert.id]);
      }
    }
  }

  deleteAlert(event: Event): void {
    if (!this.alert) {
      console.warn('⚠️ Attempted to delete undefined alert');
      return;
    }

    console.log(
      `🗑️ Delete confirmation requested for alert: ${this.alert.message}`
    );

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف التنبيه "${this.alert.message}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، احذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        console.log(
          `✅ User confirmed deletion of alert ID: ${this.alert!.id}`
        );
        this.systemAlertsService.delete(this.alert!.id).subscribe({
          next: () => {
            console.log(`🎉 Alert ID ${this.alert!.id} deleted successfully`);
            this.messageService.add({
              severity: 'success',
              summary: 'تم الحذف',
              detail: 'تم حذف التنبيه بنجاح',
            });
            this.close.emit();
            if (!this.isModalMode) {
              setTimeout(() => {
                console.log('🔄 Redirecting to alerts list after deletion');
                this.router.navigate(['/system-alerts']);
              }, 1000);
            }
          },
          error: (err) => {
            console.error(
              `❌ Failed to delete alert ID ${this.alert!.id}:`,
              err
            );
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في حذف التنبيه',
            });
          },
        });
      },
      reject: () => {
        console.log('❌ User cancelled deletion');
      },
    });
  }

  goBack(): void {
    console.log('↩️ Emitting close / going back to alerts list');
    this.close.emit();
    if (!this.isModalMode) {
      this.router.navigate(['/system-alerts']);
    }
  }

  getStatusLabel(status: string | number): string {
    return AlertUtils.getAlertStatusLabel(status);
  }

  getStatusSeverity(status: string | number): "success" | "secondary" | "info" | "warning" | "danger" | "contrast" {
    return AlertUtils.getAlertStatusSeverity(status);
  }

  getAlertTypeIcon(alertType: string): string {
    if (!alertType) return 'pi-bell';
    if (alertType.includes('OneWeek') || alertType.includes('TwoWeeks')) {
      return 'pi-exclamation-triangle';
    }
    if (alertType.includes('OneMonth')) {
      return 'pi-info-circle';
    }
    return 'pi-bell';
  }

  getAlertTypeSeverity(alertType: string): "success" | "info" | "warning" | "danger" | "secondary" | "contrast" | undefined {
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
    return expiryDate < now;
  }

  getDaysUntilExpiry(): number {
    if (!this.alert) return 0;
    const expiryDate = new Date(this.alert.expiryDate);
    const now = new Date();
    const diffTime = expiryDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }
}
