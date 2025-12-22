import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { RippleModule } from 'primeng/ripple';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        InputTextModule,
        ButtonModule,
        CheckboxModule,
        RippleModule
    ],
    templateUrl: './login.component.html'
})
export class LoginComponent {
    loginForm: FormGroup;
    loading = false;

    constructor(
        private fb: FormBuilder,
        private router: Router
    ) {
        this.loginForm = this.fb.group({
            username: ['', Validators.required],
            password: ['', Validators.required]
        });
    }

    onSubmit() {
        if (this.loginForm.valid) {
            this.loading = true;
            // Mock login
            setTimeout(() => {
                localStorage.setItem('token', 'mock-token');
                this.loading = false;
                this.router.navigate(['/dashboard']);
            }, 1000);
        }
    }
}
