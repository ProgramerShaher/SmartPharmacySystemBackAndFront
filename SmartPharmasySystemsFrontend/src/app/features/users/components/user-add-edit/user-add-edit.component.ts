import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { UsersService } from '../../services/users.service';
import { BranchService } from '../../../branches/services/branch.service';
import { DepartmentService } from '../../../departments/services/department.service';
import { EmployeeService } from '../../../employees/services/employee.service';
import { User, UserCreateDto, UserUpdateDto, BranchDto } from '../../../../core/models';

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

    branches: BranchDto[] = [];
    isLoadingBranches = false;

    departments: any[] = [];
    isLoadingDepartments = false;

    // Backend expects RoleId (1 = Admin, 2 = Pharmacist)
    roles = [
        { label: 'مدير النظام (Admin)', value: 1, icon: 'pi pi-shield' },
        { label: 'صيدلي (Pharmacist)', value: 2, icon: 'pi pi-user' }
    ];

    constructor(
        private fb: FormBuilder,
        private usersService: UsersService,
        private branchService: BranchService,
        private departmentService: DepartmentService,
        private employeeService: EmployeeService,
        private messageService: MessageService
    ) {
        this.initForm();
    }

    ngOnInit(): void {
        this.loadBranches();
        this.loadDepartments();

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

    private loadBranches() {
        this.isLoadingBranches = true;
        this.branchService.getActive().subscribe({
            next: (data) => {
                this.branches = data;
                this.isLoadingBranches = false;
            },
            error: () => {
                this.isLoadingBranches = false;
            }
        });
    }

    private loadDepartments() {
        this.isLoadingDepartments = true;
        this.departmentService.getAll().subscribe({
            next: (data: any) => {
                this.departments = data;
                this.isLoadingDepartments = false;
            },
            error: () => {
                this.isLoadingDepartments = false;
            }
        });
    }

    private initForm(): void {
        this.userForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            fullName: ['', [Validators.required]],
            email: ['', [Validators.email]],
            password: ['', [Validators.required, Validators.minLength(8)]],
            confirmPassword: [''],
            roleId: [2, [Validators.required]], // Default to Pharmacist
            branchId: [null, [Validators.required]], // Required for employee setup
            phoneNumber: ['', [Validators.pattern(/^[0-9]+$/)]],
            isActive: [true],

            // Employee specific fields
            isEmployee: [true],
            basicSalary: [0, [Validators.required, Validators.min(0)]],
            departmentId: [null, [Validators.required]],
            jobTitle: ['', [Validators.required]],
            nationalId: ['', [Validators.required]]
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
    get roleId() { return this.userForm.get('roleId')!; }
    get branchId() { return this.userForm.get('branchId')!; }
    get phoneNumber() { return this.userForm.get('phoneNumber')!; }
    get isActive() { return this.userForm.get('isActive')!; }

    // Employee getters
    get isEmployee() { return this.userForm.get('isEmployee')!; }
    get basicSalary() { return this.userForm.get('basicSalary')!; }
    get departmentId() { return this.userForm.get('departmentId')!; }
    get jobTitle() { return this.userForm.get('jobTitle')!; }
    get nationalId() { return this.userForm.get('nationalId')!; }

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
            // Create User
            const createDto: any = {
                ...formData,
                passwordHash: formData.password,
                confirmPassword: formData.password
            };
            delete createDto.password;

            this.usersService.create(createDto).subscribe({
                next: (res) => {
                    // Check if we need to create an employee too
                    if (formData.isEmployee) {
                        const employeeCode = 'EMP-' + Math.floor(1000 + Math.random() * 9000); // Generate random code
                        const createEmpDto = {
                            employeeCode: employeeCode,
                            fullName: formData.fullName,
                            nationalId: formData.nationalId,
                            branchId: formData.branchId,
                            departmentId: formData.departmentId,
                            jobTitle: formData.jobTitle,
                            hireDate: new Date(),
                            basicSalary: formData.basicSalary,
                            isActive: true
                        };

                        this.employeeService.create(createEmpDto as any).subscribe({
                            next: () => {
                                this.loading = false;
                                this.save.emit(res);
                            },
                            error: (err) => {
                                console.error('Error creating employee:', err);
                                // User created but employee failed
                                this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'تم إنشاء المستخدم بنجاح، لكن حدث خطأ أثناء إنشاء بيانات الموظف (تأكد من رقم الهوية).' });
                                this.loading = false;
                                this.save.emit(res);
                            }
                        });
                    } else {
                        this.loading = false;
                        this.save.emit(res);
                    }
                },
                error: (err) => {
                    console.error('Error creating user:', err);
                    let errMsg = 'فشل في إنشاء المستخدم، قد يكون اسم المستخدم مستخدماً بالفعل';
                    if (err?.error?.message) errMsg = err.error.message;
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: errMsg });
                    this.loading = false;
                }
            });
        }
    }

    onCancel(): void {
        this.cancel.emit();
    }
}
