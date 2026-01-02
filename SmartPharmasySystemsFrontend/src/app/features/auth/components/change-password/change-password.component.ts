import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-change-password',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        PasswordModule,
        ToastModule,
        RouterModule
    ],
    templateUrl: './change-password.component.html',
    styleUrl: './change-password.component.scss',
    providers: [MessageService]
})
export class ChangePasswordComponent implements OnInit {
    passwordForm: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private messageService: MessageService
    ) {
        this.passwordForm = this.fb.group({
            currentPassword: ['', Validators.required],
            newPassword: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', Validators.required]
        }, { validator: this.passwordMatchValidator });
    }

    ngOnInit(): void {}

    passwordMatchValidator(g: FormGroup) {
        return g.get('newPassword')?.value === g.get('confirmPassword')?.value
            ? null : { 'mismatch': true };
    }

    onSubmit() {
        if (this.passwordForm.invalid) return;

        this.loading = true;
        this.authService.changePassword(this.passwordForm.value).subscribe({
            next: () => {
                this.messageService.add({ 
                    severity: 'success', 
                    summary: 'تم بنجاح', 
                    detail: 'تم تغيير كلمة المرور بنجاح' 
                });
                this.passwordForm.reset();
                this.loading = false;
            },
            error: (err) => {
                this.messageService.add({ 
                    severity: 'error', 
                    summary: 'خطأ', 
                    detail: err.error?.message || 'فشل في تغيير كلمة المرور' 
                });
                this.loading = false;
            }
        });
    }
}
