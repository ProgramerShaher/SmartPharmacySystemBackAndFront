import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../../../core/models';
import { SettingsService } from '../../../../core/services/settings.service';
import { PharmacySettings } from '../../../../core/models/settings/pharmacy-settings.interface';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { RippleModule } from 'primeng/ripple';
import { environment } from '../../../../../environments/environment';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
      ButtonModule,
      InputTextModule,
      PasswordModule,
      CheckboxModule,
      ToastModule,
        RippleModule,
        RouterModule
  ],
  template: `
    <div class="login-wrapper" dir="rtl">
      <p-toast></p-toast>

      <section class="login-card">
        <div class="card-accent"></div>

        <div class="login-header">
          <div class="logo-box">
            <img *ngIf="pharmacyLogoUrl; else defaultLoginLogo" [src]="pharmacyLogoUrl" [alt]="pharmacyName">
            <ng-template #defaultLoginLogo>
              <i class="pi pi-shield"></i>
            </ng-template>
          </div>
          <div>
            <h1>{{ pharmacyName }}</h1>
            <p>تسجيل الدخول إلى نظام إدارة الصيدلية</p>
          </div>
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
          <label class="field-label" for="username">اسم المستخدم</label>
          <span class="p-input-icon-left w-full">
            <i class="pi pi-user"></i>
            <input pInputText id="username" formControlName="username" placeholder="مثال: admin" autocomplete="username" />
          </span>

          <label class="field-label" for="password">كلمة المرور</label>
          <span class="p-input-icon-left w-full">
            <i class="pi pi-lock"></i>
            <p-password
              inputId="password"
              formControlName="password"
              placeholder="أدخل كلمة المرور"
              [toggleMask]="true"
              [feedback]="false"
              styleClass="w-full"
              inputStyleClass="w-full"
              autocomplete="current-password">
            </p-password>
          </span>

          <div class="form-row">
            <div class="remember">
              <p-checkbox formControlName="rememberMe" [binary]="true" inputId="rem"></p-checkbox>
              <label for="rem">تذكرني</label>
            </div>
            <a class="muted-link">نسيت كلمة المرور؟</a>
          </div>

          <p-button
            type="submit"
            label="تسجيل الدخول"
            icon="pi pi-sign-in"
            [loading]="loading"
            [disabled]="loginForm.invalid"
            styleClass="auth-submit w-full">
          </p-button>

          <div class="quick-actions">
            <button type="button" class="quick-login-btn admin" (click)="quickLogin('admin')">
              <i class="pi pi-user-edit"></i>
              <span>مدير</span>
            </button>
            <button type="button" class="quick-login-btn pharmacist" (click)="quickLogin('pharmacist')">
              <i class="pi pi-user"></i>
              <span>صيدلي</span>
            </button>
          </div>
        </form>

        <div class="login-footer">
          ليس لديك حساب؟
          <a routerLink="/auth/register">إنشاء حساب جديد</a>
        </div>
      </section>
    </div>
  `,
  styleUrls: ['./login.component.scss'],
  providers: [MessageService]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  pharmacyName = 'الصيدلية الذكية';
  pharmacyLogoUrl: string | null = null;
  readonly serverUrl = environment.apiUrl.replace('/api', '');

    constructor(
        private fb: FormBuilder,
      private authService: AuthService,
      private router: Router,
      private messageService: MessageService,
      private settingsService: SettingsService
    ) { }

  ngOnInit() {
    // Redirect if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return;
    }

        this.loginForm = this.fb.group({
            username: ['', Validators.required],
          password: ['', Validators.required],
          rememberMe: [false]
        });

    this.loadPharmacySettings();
    }

  private loadPharmacySettings(): void {
    this.settingsService.getSettings().subscribe({
      next: (settings: PharmacySettings) => {
        this.pharmacyName = settings.pharmacyName || this.pharmacyName;
        this.pharmacyLogoUrl = settings.logoUrl ? `${this.serverUrl}${settings.logoUrl}` : null;
      }
    });
  }

    onSubmit() {
      if (this.loginForm.invalid) {
        Object.keys(this.loginForm.controls).forEach(key => {
          this.loginForm.get(key)?.markAsTouched();
        });
        return;
      }

      this.loading = true;

      const credentials: LoginRequest = {
        username: this.loginForm.value.username,
        password: this.loginForm.value.password
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          this.messageService.add({
            severity: 'success',
            summary: 'نجاح',
            detail: `مرحباً ${response.fullName}! تم تسجيل الدخول بنجاح`,
            life: 3000
          });

          setTimeout(() => {
                this.router.navigate(['/dashboard']);
              }, 1000);
        },
        error: (error) => {
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ في تسجيل الدخول',
            detail: error.error?.message || 'اسم المستخدم أو كلمة المرور غير صحيحة',
            life: 5000
          });
        }
      });
  }

  quickLogin(role: 'admin' | 'pharmacist') {
    const credentials: LoginRequest = {
      username: role === 'admin' ? 'admin' : 'pharmacist',
      password: '123456' // Default password for demo
    };

    this.loginForm.patchValue(credentials);
    this.onSubmit();
    }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
    }
}
