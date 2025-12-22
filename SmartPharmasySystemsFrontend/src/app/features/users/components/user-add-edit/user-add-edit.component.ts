import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { UsersService } from '../../services/users.service';
import { User, UserCreateDto, UserUpdateDto } from '../../../../core/models';

// PrimeNG Imports
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-user-form',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        InputTextModule,
        InputTextareaModule,
        DropdownModule,
        PasswordModule,
        ButtonModule,
        InputSwitchModule,
        TooltipModule
    ],
    templateUrl: './user-add-edit.component.html',
    styleUrl: './user-add-edit.component.scss'
})
export class UserFormComponent implements OnInit {
    @Input() user: User | null = null;
    @Output() save = new EventEmitter<any>();
    @Output() cancel = new EventEmitter<void>();

    userForm!: FormGroup;
    loading = false;
    showPassword = false;
    showConfirmPassword = false;
    currentDate = new Date();
    passwordStrength = 0;

    roles = [
        { label: 'مدير النظام', value: 'Admin', icon: 'pi pi-shield' },
        { label: 'مدير صيدلية', value: 'Manager', icon: 'pi pi-briefcase' },
        { label: 'صيدلي (بائع)', value: 'User', icon: 'pi pi-user' }
    ];

    constructor(
        private fb: FormBuilder,
        private usersService: UsersService,
        private messageService: MessageService
    ) {
        this.initForm();
    }

    ngOnInit(): void {
        if (this.user) {
            this.userForm.patchValue({
                ...this.user,
                password: ''
            });
            // If editing, password is not required
            this.password.setValidators([Validators.minLength(8)]);
        }

        // Listen to password changes for strength meter
        this.password.valueChanges.subscribe(val => {
            this.calculatePasswordStrength(val);
        });
    }

    private initForm(): void {
        this.userForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            fullName: ['', [Validators.required]],
            email: ['', [Validators.email]],
            password: ['', [Validators.required, Validators.minLength(8)]],
            confirmPassword: [''],
            role: ['User', [Validators.required]],
            phoneNumber: ['', [Validators.pattern(/^[0-9]+$/)]],
            isActive: [true]
        }, { validators: this.passwordMatchValidator });
    }

    private passwordMatchValidator(g: FormGroup) {
        const password = g.get('password')?.value;
        const confirmPassword = g.get('confirmPassword')?.value;
        if (password && confirmPassword && password !== confirmPassword) {
            return { passwordMismatch: true };
        }
        return null;
    }

    // Getters for easy access
    get username() { return this.userForm.get('username')!; }
    get fullName() { return this.userForm.get('fullName')!; }
    get email() { return this.userForm.get('email')!; }
    get password() { return this.userForm.get('password')!; }
    get confirmPassword() { return this.userForm.get('confirmPassword')!; }
    get role() { return this.userForm.get('role')!; }
    get phoneNumber() { return this.userForm.get('phoneNumber')!; }
    get isActive() { return this.userForm.get('isActive')!; }

    togglePassword(): void {
        this.showPassword = !this.showPassword;
    }

    toggleConfirmPassword(): void {
        this.showConfirmPassword = !this.showConfirmPassword;
    }

    calculatePasswordStrength(password: string): void {
        if (!password) {
            this.passwordStrength = 0;
            return;
        }

        let strength = 0;
        if (password.length >= 8) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[^A-Za-z0-9]/.test(password)) strength++;

        this.passwordStrength = strength;
    }

    getPasswordStrengthText(): string {
        if (this.passwordStrength <= 2) return 'ضعيفة';
        if (this.passwordStrength <= 4) return 'متوسطة';
        return 'قوية جداً';
    }

    saveUser(): void {
        if (this.userForm.invalid) {
            this.userForm.markAllAsTouched();
            return;
        }

        this.loading = true;
        const formData = { ...this.userForm.value };
        delete formData.confirmPassword;

        if (this.user) {
            // Update
            const updateDto: UserUpdateDto = { ...formData };
            if (!updateDto.password) delete updateDto.password;

            this.usersService.update(this.user.id, updateDto).subscribe({
                next: (res) => {
                    this.loading = false;
                    this.save.emit(res);
                },
                error: () => this.loading = false
            });
        } else {
            // Create
            const createDto: UserCreateDto = { ...formData };
            this.usersService.create(createDto).subscribe({
                next: (res) => {
                    this.loading = false;
                    this.save.emit(res);
                },
                error: () => this.loading = false
            });
        }
    }

    onCancel(): void {
        this.cancel.emit();
    }
}
