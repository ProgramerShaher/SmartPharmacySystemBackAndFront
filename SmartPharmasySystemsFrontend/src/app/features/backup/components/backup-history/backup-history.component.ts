import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BackupService, BackupHistory } from '../../../../core/services/backup.service';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-backup-history',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    CardModule,
    ButtonModule,
    TagModule,
    DropdownModule,
    FormsModule,
    TooltipModule,
    ToastModule
  ],
  templateUrl: './backup-history.component.html',
  styleUrls: ['./backup-history.component.scss'],
  providers: [MessageService]
})
export class BackupHistoryComponent implements OnInit {
  histories: BackupHistory[] = [];
  totalRecords = 0;
  loading = false;
  
  currentPage = 1;
  pageSize = 10;
  
  statusFilter?: string;
  typeFilter?: string;

  statusOptions = [
    { label: 'الكل', value: undefined },
    { label: 'مكتمل', value: 'Completed' },
    { label: 'فشل', value: 'Failed' },
    { label: 'قيد الانتظار', value: 'Pending' },
    { label: 'جاري العمل', value: 'SqlBackupRunning' } // For simplicity we group active ones or just search explicitly
  ];

  typeOptions = [
    { label: 'الكل', value: undefined },
    { label: 'تلقائي', value: 'Automatic' },
    { label: 'يدوي', value: 'Manual' }
  ];

  constructor(
    private backupService: BackupService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(event?: any): void {
    this.loading = true;
    
    if (event) {
      this.currentPage = (event.first / event.rows) + 1;
      this.pageSize = event.rows;
    }

    this.backupService.getHistory(this.currentPage, this.pageSize, this.statusFilter, this.typeFilter).subscribe({
      next: (res) => {
        this.histories = res.data.items;
        this.totalRecords = res.data.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل السجل' });
        this.loading = false;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadHistory();
  }

  retryBackup(id: number): void {
    this.loading = true;
    this.backupService.retryBackup(id).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم جدولة إعادة المحاولة.' });
        this.loadHistory();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل بدء إعادة المحاولة' });
        this.loading = false;
      }
    });
  }

  getStatusSeverity(status: string): 'success' | 'danger' | 'warning' | 'info' | 'secondary' | 'contrast' | undefined {
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
      case 'SqlBackupRunning': return 'نسخ SQL';
      case 'VerificationRunning': return 'فحص';
      case 'EncryptionRunning': return 'تشفير';
      case 'Uploading': return 'رفع';
      case 'Retrying': return 'إعادة محاولة';
      default: return status;
    }
  }

  formatBytes(bytes?: number, decimals = 2): string {
    if (bytes === undefined || bytes === null || bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  formatDuration(seconds?: number): string {
    if (seconds === undefined || seconds === null) return '-';
    if (seconds < 60) return `${seconds} ثانية`;
    const min = Math.floor(seconds / 60);
    const sec = seconds % 60;
    return `${min} د و ${sec} ث`;
  }
}
