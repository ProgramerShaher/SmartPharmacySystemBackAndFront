import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../../../core/models';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { RippleModule } from 'primeng/ripple';

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
        RippleModule
    ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  providers: [MessageService]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
      private authService: AuthService,
      private router: Router,
      private messageService: MessageService
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
