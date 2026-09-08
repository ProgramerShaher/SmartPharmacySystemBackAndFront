import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BackupDashboard {
  isSubsystemHealthy: boolean;
  subsystemHealthMessage: string;
  totalBackups: number;
  successfulBackups: number;
  failedBackups: number;
  lastSuccessfulBackup?: Date;
  nextScheduledBackup?: Date;
  activeBackup?: BackupHistory;
}

export interface BackupConfig {
  isAutoBackupEnabled: boolean;
  frequency: string;
  intervalDays: number;
  scheduledTime: string;
  timezone: string;
  nextScheduledRun?: Date;
  retentionMode: string;
  maxBackupsToKeep: number;
  retentionDays: number;
  localRetentionDays: number;
  googleDriveFolderId?: string;
  googleDriveFolderName?: string;
  verifyAfterBackup: boolean;
  encryptBackup: boolean;
}

export interface BackupHistory {
  id: number;
  occurrenceId: string;
  databaseName: string;
  backupType: string;
  status: string;
  uploadStatus: string;
  isRecovery: boolean;
  startedAt: Date;
  completedAt?: Date;
  durationSeconds?: number;
  fileName?: string;
  fileSizeBytes?: number;
  isVerified: boolean;
  verificationMessage?: string;
  googleDriveWebViewLink?: string;
  errorMessage?: string;
  triggeredByUserName?: string;
  retryCount: number;
}

export interface DriveConnectionTest {
  isSuccess: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class BackupService {
  private endpoint = `${environment.apiUrl}/Backups`;

  constructor(private http: HttpClient) {
  }

  getDashboard(): Observable<{ data: BackupDashboard }> {
    return this.http.get<{ data: BackupDashboard }>(`${this.endpoint}/dashboard`);
  }

  getSettings(): Observable<{ data: BackupConfig }> {
    return this.http.get<{ data: BackupConfig }>(`${this.endpoint}/settings`);
  }

  updateSettings(data: BackupConfig): Observable<{ data: BackupConfig }> {
    return this.http.put<{ data: BackupConfig }>(`${this.endpoint}/settings`, data);
  }

  getHistory(page: number, pageSize: number, status?: string, type?: string): Observable<{ data: { items: BackupHistory[], totalCount: number } }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);
    if (type) params = params.set('type', type);

    return this.http.get<{ data: { items: BackupHistory[], totalCount: number } }>(`${this.endpoint}/history`, { params });
  }

  triggerManualBackup(): Observable<{ data: BackupHistory, message: string }> {
    return this.http.post<{ data: BackupHistory, message: string }>(`${this.endpoint}/manual`, {});
  }

  retryBackup(id: number): Observable<{ data: BackupHistory, message: string }> {
    return this.http.post<{ data: BackupHistory, message: string }>(`${this.endpoint}/retry/${id}`, {});
  }

  exportKey(currentPassword: string): Observable<{ data: { base64Key: string }, message: string }> {
    return this.http.post<{ data: { base64Key: string }, message: string }>(`${this.endpoint}/settings/export-key`, { currentPassword });
  }

  importKey(data: { base64Key: string, currentPassword: string }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.endpoint}/settings/import-key`, data);
  }

  testDriveConnection(): Observable<{ data: DriveConnectionTest }> {
    return this.http.post<{ data: DriveConnectionTest }>(`${this.endpoint}/settings/test-drive`, {});
  }
}
