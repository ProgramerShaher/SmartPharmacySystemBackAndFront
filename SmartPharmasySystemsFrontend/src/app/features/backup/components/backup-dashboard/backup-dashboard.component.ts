import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BackupService, BackupDashboard } from '../../../../core/services/backup.service';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ProgressBarModule } from 'primeng/progressbar';
import { Subscription, interval } from 'rxjs';
import { MessageModule } from 'primeng/message';
import { MessagesModule } from 'primeng/messages';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-backup-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    TagModule,
    ProgressBarModule,
    MessageModule,
    MessagesModule,
    ToastModule
  ],
  templateUrl: './backup-dashboard.component.html',
  styleUrls: ['./backup-dashboard.component.scss'],
  providers: [MessageService]
})
export class BackupDashboardComponent implements OnInit, OnDestroy {
  dashboardData: BackupDashboard | null = null;
  loading = true;
  isTriggering = false;
  private pollSubscription?: Subscription;

  constructor(
    private backupService: BackupService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
    // Poll every 5 seconds if there's an active backup
    this.pollSubscription = interval(5000).subscribe(() => {
      if (this.dashboardData?.activeBackup) {
        this.loadDashboard(false);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
    }
  }

  loadDashboard(showLoading = true): void {
    if (showLoading) this.loading = true;
    this.backupService.getDashboard().subscribe({
      next: (res) => {
        this.dashboardData = res.data;
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات لوحة التحكم' });
        this.loading = false;
      }
    });
  }

  triggerManualBackup(): void {
    if (this.isTriggering) return;
    this.isTriggering = true;
    this.backupService.triggerManualBackup().subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: res.message });
        this.loadDashboard();
        this.isTriggering = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء محاولة بدء النسخ الاحتياطي.' });
        this.isTriggering = false;
      }
    });
  }

  getStatusSeverity(status: string): string {
    switch (status) {
      case 'Completed': return 'success';
      case 'Failed': return 'danger';
      case 'Pending': return 'warning';
      default: return 'info';
    }
  }
  
  getStatusLabel(status: string): string {
    switch (status) {
      case 'Completed': return 'مكتمل';
      case 'Failed': return 'فشل';
      case 'Pending': return 'قيد الانتظار';
      case 'SqlBackupRunning': return 'جاري نسخ قاعدة البيانات';
      case 'VerificationRunning': return 'جاري التحقق';
      case 'EncryptionRunning': return 'جاري التشفير';
      case 'Uploading': return 'جاري الرفع السحابي';
      case 'Retrying': return 'إعادة المحاولة';
      default: return status;
    }
  }
}
