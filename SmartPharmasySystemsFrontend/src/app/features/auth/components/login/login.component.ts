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
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';
import { BranchDto } from '../../../../core/models';
import { BranchService } from '../../../branches/services/branch.service';
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
    RouterModule,
    DialogModule,
    DropdownModule,
    FormsModule
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

      <!-- Branch Selection Dialog for Admins -->
      <p-dialog header="تحديد فرع الدخول" [(visible)]="showBranchDialog" [modal]="true" [closable]="false" [style]="{width: '400px'}">
        <div class="p-4" dir="rtl">
            <p class="mb-4">مرحباً بك كمدير للنظام! يرجى اختيار الفرع الذي تود الدخول إليه:</p>
            <div class="field mb-4">
                <label for="branchSelect" class="block mb-2 font-bold">الفرع</label>
                <p-dropdown 
                    [options]="activeBranches" 
                    [(ngModel)]="selectedBranchId" 
                    optionLabel="name" 
                    optionValue="id" 
                    placeholder="اختر الفرع" 
                    [style]="{'width':'100%'}"
                    [showClear]="true"
                    appendTo="body">
                </p-dropdown>
            </div>
        </div>
        <ng-template pTemplate="footer">
            <p-button label="إلغاء" icon="pi pi-times" (click)="cancelBranchSelection()" styleClass="p-button-text p-button-secondary"></p-button>
            <p-button label="دخول للفرع" icon="pi pi-check" (click)="confirmBranchLogin()" [disabled]="!selectedBranchId" [loading]="loadingBranch"></p-button>
        </ng-template>
      </p-dialog>
    </div>
  `,
  styleUrls: ['./login.component.scss'],
  providers: [MessageService]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  loadingBranch = false;
  showBranchDialog = false;
  activeBranches: BranchDto[] = [];
  selectedBranchId: number | null = null;
  pendingCredentials: LoginRequest | null = null;
  pharmacyName = 'الصيدلية الذكية';
  pharmacyLogoUrl: string | null = null;
  readonly serverUrl = environment.apiUrl.replace('/api', '');

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService,
    private settingsService: SettingsService,
    private branchService: BranchService
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
        // If Admin and didn't select a branch yet, show branch dialog or auto-login to Main
        if ((response.roleName === 'Admin' || response.roleName === 'Administrator') && !credentials.branchId) {
          this.pendingCredentials = credentials;

          this.branchService.getActive().subscribe({
            next: (branches) => {
              const mainBranch = branches.find(b => b.branchType === 1 || b.branchType === 'Main');
              if (mainBranch) {
                this.selectedBranchId = mainBranch.id;
                this.confirmBranchLogin();
              } else {
                this.loading = false;
                this.activeBranches = branches;
                if (branches.length === 1) {
                  this.selectedBranchId = branches[0].id;
                }
                this.showBranchDialog = true;
              }
            },
            error: (err) => {
              this.loading = false;
              console.error('Failed to load branches', err);
              this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة الفروع' });
            }
          });
          return; // Don't navigate yet
        }

        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح',
          detail: `مرحباً ${response.fullName}! تم تسجيل الدخول بنجاح`,
          life: 2000
        });
        // Navigate immediately - token is already stored by authService.login()
        this.router.navigate(['/dashboard']);
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

  loadActiveBranches() {
    this.branchService.getActive().subscribe({
      next: (branches) => {
        this.activeBranches = branches;
        // If only one branch, auto-select it
        if (branches.length === 1) {
          this.selectedBranchId = branches[0].id;
        }
      },
      error: (err) => {
        console.error('Failed to load branches', err);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة الفروع' });
      }
    });
  }

  confirmBranchLogin() {
    if (!this.selectedBranchId || !this.pendingCredentials) return;

    this.loadingBranch = true;
    const finalCredentials: LoginRequest = {
      ...this.pendingCredentials,
      branchId: this.selectedBranchId
    };

    this.authService.login(finalCredentials).subscribe({
      next: (response) => {
        this.loading = false;
        this.loadingBranch = false;
        this.showBranchDialog = false;
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: `تم الدخول للفرع بنجاح` });
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.loading = false;
        this.loadingBranch = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تسجيل الدخول للفرع المحدد' });
      }
    });
  }

  cancelBranchSelection() {
    this.showBranchDialog = false;
    this.pendingCredentials = null;
    this.authService.logout(); // Clear the initial admin token
  }

  quickLogin(role: 'admin' | 'pharmacist') {
    const credentials: LoginRequest = {
      username: role === 'admin' ? 'admin' : 'pharmacist',
      password: '1234'
    };
    this.loginForm.patchValue(credentials);
    this.onSubmit();
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}