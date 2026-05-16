import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { FileUploadModule } from 'primeng/fileupload';
import { SettingsService } from '../../../../core/services/settings.service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-pharmacy-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ToastModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    InputTextareaModule,
    FileUploadModule
  ],
  providers: [MessageService],
  templateUrl: './pharmacy-profile.component.html',
  styleUrls: ['./pharmacy-profile.component.css']
})
export class PharmacyProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private settingsService = inject(SettingsService);
  private messageService = inject(MessageService);

  settingsForm: FormGroup;
  isLoading = false;
  isSaving = false;
  currentLogoUrl: string | null = null;
  serverUrl = environment.apiUrl.replace('/api', ''); // Get base server URL for images

  currencies = [
    { label: 'ريال سعودي (ر.س)', value: 'ر.س' },
    { label: 'دولار أمريكي ($)', value: '$' },
    { label: 'يورو (€)', value: '€' },
    { label: 'درهم إماراتي (د.إ)', value: 'د.إ' }
  ];

  constructor() {
    this.settingsForm = this.fb.group({
      pharmacyName: ['', Validators.required],
      address: [''],
      phoneNumber: [''],
      mobileNumber: [''],
      email: ['', Validators.email],
      taxNumber: [''],
      commercialRegister: [''],
      healthMinistryLicense: [''],
      website: [''],
      baseCurrency: ['ر.س', Validators.required],
      invoiceWelcomeMessage: ['']
    });
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.isLoading = true;
    this.settingsService.getSettings().subscribe({
      next: (settings) => {
        this.settingsForm.patchValue(settings);
        this.currentLogoUrl = settings.logoUrl ? `${this.serverUrl}${settings.logoUrl}` : null;
        this.isLoading = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الإعدادات' });
        this.isLoading = false;
      }
    });
  }

  onLogoSelect(event: any, fileUpload: any): void {
    const file = event.files[0];
    if (file) {
      this.settingsService.uploadLogo(file).subscribe({
        next: (res) => {
          this.currentLogoUrl = `${this.serverUrl}${res.logoUrl}`;
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الشعار بنجاح' });
          fileUpload.clear(); // Clear the selection
        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في رفع الشعار' });
          fileUpload.clear();
        }
      });
    }
  }

  saveSettings(): void {
    if (this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.settingsService.updateSettings(this.settingsForm.value).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الإعدادات بنجاح' });
        this.isSaving = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في حفظ الإعدادات' });
        this.isSaving = false;
      }
    });
  }
}
