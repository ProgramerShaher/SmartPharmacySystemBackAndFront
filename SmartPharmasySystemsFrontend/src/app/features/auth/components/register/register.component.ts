import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        PasswordModule,
        CheckboxModule,
        ToastModule
    ],
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss',
    providers: [MessageService]
})
export class RegisterComponent {
    registerForm: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
        private messageService: MessageService
    ) {
        this.registerForm = this.fb.group({
            fullName: ['', Validators.required],
            username: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            agreeTerms: [false, Validators.requiredTrue]
        });
    }

    onSubmit() {
        if (this.registerForm.invalid) return;

        this.loading = true;
        // Logic would go here
        setTimeout(() => {
            this.messageService.add({
                severity: 'success',
                summary: 'تم الطلب',
                detail: 'تم إرسال طلب إنشاء الحساب للمدير للموافقة'
            });
            this.loading = false;
        }, 1500);
    }
}
