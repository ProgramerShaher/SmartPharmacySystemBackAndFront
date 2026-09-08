import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BackupService, BackupConfig, DriveConnectionTest } from '../../../../core/services/backup.service';
import { MessageService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { TabViewModule } from 'primeng/tabview';
import { DialogModule } from 'primeng/dialog';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { MessagesModule } from 'primeng/messages';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-backup-settings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    CardModule,
    InputTextModule,
    DropdownModule,
    InputSwitchModule,
    InputNumberModule,
    ButtonModule,
    TabViewModule,
    DialogModule,
    PasswordModule,
    MessageModule,
    MessagesModule,
    ToastModule
  ],
  templateUrl: './backup-settings.component.html',
  styleUrls: ['./backup-settings.component.scss'],
  providers: [MessageService]
})
export class BackupSettingsComponent implements OnInit {
  settingsForm!: FormGroup;
  loading = true;
  saving = false;
  testingDrive = false;
  driveTestResult: DriveConnectionTest | null = null;

  frequencyOptions = [
    { label: 'يومياً', value: 'Daily' },
    { label: 'كل يومين', value: 'Every2Days' },
    { label: 'كل 3 أيام', value: 'Every3Days' },
    { label: 'أسبوعياً', value: 'Weekly' },
    { label: 'شهرياً', value: 'Monthly' },
    { label: 'مخصص (بالأيام)', value: 'Custom' }
  ];

  retentionOptions = [
    { label: 'بالعدد (أقصى عدد)', value: 'ByCount' },
    { label: 'بالأيام (أقدم من)', value: 'ByDays' }
  ];

  timezoneOptions = [
    { label: 'توقيت اليمن (Arab Standard Time)', value: 'Arab Standard Time' }
  ];

  // Key Management Dialogs
  displayExportDialog = false;
  displayImportDialog = false;
  exportPassword = '';
  importPassword = '';
  importKeyString = '';
  exporting = false;
  importing = false;
  exportedKey: string | null = null;

  constructor(
    private fb: FormBuilder,
    private backupService: BackupService,
    private messageService: MessageService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  initForm(): void {
    this.settingsForm = this.fb.group({
      isAutoBackupEnabled: [false],
      frequency: ['Daily', Validators.required],
      intervalDays: [1, [Validators.required, Validators.min(1), Validators.max(365)]],
      scheduledTime: ['02:00:00', Validators.required],
      timezone: ['Arab Standard Time', Validators.required],
      retentionMode: ['ByCount', Validators.required],
      maxBackupsToKeep: [10, [Validators.required, Validators.min(1)]],
      retentionDays: [30, [Validators.required, Validators.min(1)]],
      localRetentionDays: [3, [Validators.required, Validators.min(1), Validators.max(30)]],
      googleDriveFolderId: [''],
      googleDriveFolderName: ['SmartPharmacyBackups'],
      verifyAfterBackup: [true],
      encryptBackup: [true]
    });

    this.settingsForm.get('frequency')?.valueChanges.subscribe(val => {
      if (val === 'Custom') {
        this.settingsForm.get('intervalDays')?.enable();
      } else {
        this.settingsForm.get('intervalDays')?.disable();
      }
    });

    this.settingsForm.get('retentionMode')?.valueChanges.subscribe(val => {
      if (val === 'ByCount') {
        this.settingsForm.get('maxBackupsToKeep')?.enable();
        this.settingsForm.get('retentionDays')?.disable();
      } else {
        this.settingsForm.get('maxBackupsToKeep')?.disable();
        this.settingsForm.get('retentionDays')?.enable();
      }
    });
  }

  loadSettings(): void {
    this.loading = true;
    this.backupService.getSettings().subscribe({
      next: (res) => {
        this.settingsForm.patchValue(res.data);
        // trigger value changes logic
        this.settingsForm.get('frequency')?.updateValueAndValidity();
        this.settingsForm.get('retentionMode')?.updateValueAndValidity();
        this.loading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الإعدادات' });
        this.loading = false;
      }
    });
  }

  saveSettings(): void {
    if (this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    const payload = this.settingsForm.getRawValue();
    
    // Ensure scheduledTime is properly formatted even if it's missing seconds (e.g. "02:30" -> "02:30:00")
    if (payload.scheduledTime && payload.scheduledTime.length === 5) {
      payload.scheduledTime = payload.scheduledTime + ':00';
    }

    this.backupService.updateSettings(payload).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الإعدادات بنجاح' });
        this.saving = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في حفظ الإعدادات' });
        this.saving = false;
      }
    });
  }

  testDrive(): void {
    this.testingDrive = true;
    this.driveTestResult = null;
    this.backupService.testDriveConnection().subscribe({
      next: (res) => {
        this.driveTestResult = res.data;
        if (res.data.isSuccess) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: res.data.message });
        } else {
          this.messageService.add({ severity: 'warn', summary: 'تحذير', detail: res.data.message });
        }
        this.testingDrive = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء فحص الاتصال' });
        this.testingDrive = false;
      }
    });
  }

  // Key Management
  openExportDialog(): void {
    this.exportPassword = '';
    this.exportedKey = null;
    this.displayExportDialog = true;
  }

  exportKey(): void {
    if (!this.exportPassword) return;
    this.exporting = true;
    this.backupService.exportKey(this.exportPassword).subscribe({
      next: (res) => {
        this.exportedKey = res.data.base64Key;
        this.exporting = false;
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تصدير المفتاح بنجاح. يرجى نسخه والاحتفاظ به في مكان آمن جداً.' });
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل التصدير' });
        this.exporting = false;
      }
    });
  }

  copyKey(): void {
    if (this.exportedKey) {
      navigator.clipboard.writeText(this.exportedKey).then(() => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم نسخ المفتاح' });
      });
    }
  }

  openImportDialog(): void {
    this.importPassword = '';
    this.importKeyString = '';
    this.displayImportDialog = true;
  }

  importKey(): void {
    if (!this.importPassword || !this.importKeyString) return;
    this.importing = true;
    this.backupService.importKey({ base64Key: this.importKeyString, currentPassword: this.importPassword }).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: res.message });
        this.displayImportDialog = false;
        this.importing = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل استيراد المفتاح' });
        this.importing = false;
      }
    });
  }
}
