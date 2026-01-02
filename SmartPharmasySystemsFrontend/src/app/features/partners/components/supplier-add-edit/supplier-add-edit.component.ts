import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { RippleModule } from 'primeng/ripple';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.models';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-supplier-add-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        InputTextModule,
        InputTextareaModule,
        InputNumberModule,
        RippleModule,
        CardModule,
        ProgressSpinnerModule,
        ToastModule,
        TooltipModule
    ],
    templateUrl: './supplier-add-edit.component.html',
    styleUrl: './supplier-add-edit.component.scss',
    providers: [MessageService]
})
export class SupplierAddEditComponent implements OnInit {
    supplierForm: FormGroup;
    isEditMode: boolean = false;
    supplierId?: number;
    loading: boolean = false;
    saving: boolean = false;

    constructor(
        private fb: FormBuilder,
        private supplierService: SupplierService,
        private messageService: MessageService,
        private route: ActivatedRoute,
        private router: Router
    ) {
        this.supplierForm = this.fb.group({
            name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
            contactPerson: ['', [Validators.required, Validators.maxLength(50)]],
            phoneNumber: ['', [Validators.required, Validators.pattern(/^[\d\s\+\-\(\)]{7,15}$/)]],
            email: ['', [Validators.email, Validators.maxLength(100)]],
            address: ['', [Validators.required, Validators.maxLength(200)]],
            notes: ['', [Validators.maxLength(500)]],
            balance: [0, [Validators.min(0)]]
        });
    }

    ngOnInit(): void {
        console.log('🚀 Supplier Add/Edit Component Initialized');
        this.route.params.subscribe(params => {
            if (params['id']) {
                this.isEditMode = true;
                this.supplierId = +params['id'];
                console.log(`📝 Edit mode activated for supplier ID: ${this.supplierId}`);
                this.loadSupplier(this.supplierId);
            } else {
                console.log('🆕 Create new supplier mode activated');
            }
        });
    }

    loadSupplier(id: number): void {
        this.loading = true;
        console.log(`⏳ Loading supplier data for ID: ${id}`);

        this.supplierService.getById(id)
            .pipe(finalize(() => {
                this.loading = false;
                console.log(`✅ Finished loading supplier ID: ${id}`);
            }))
            .subscribe({
                next: (supplier) => {
                    console.log(`📋 Supplier data loaded successfully:`, supplier);
                    this.supplierForm.patchValue({
                        name: supplier.name,
                        contactPerson: supplier.contactPerson || '',
                        phoneNumber: supplier.phoneNumber,
                        email: supplier.email || '',
                        address: supplier.address,
                        notes: supplier.notes || '',
                        balance: supplier.balance || 0
                    });
                },
                error: (error) => {
                    console.error(`❌ Failed to load supplier ID ${id}:`, error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في تحميل بيانات المورد'
                    });
                    this.router.navigate(['/partners/suppliers']);
                }
            });
    }

    onSubmit(): void {
        console.log('📤 Form submission started');

        if (this.supplierForm.invalid) {
            console.warn('⚠️ Form is invalid, showing validation errors');
            console.log('📊 Form validation errors:', this.supplierForm.errors);
            this.markFormGroupTouched(this.supplierForm);
            this.messageService.add({
                severity: 'warn',
                summary: 'تحذير',
                detail: 'يرجى تعبئة جميع الحقول المطلوبة بشكل صحيح'
            });
            return;
        }

        this.saving = true;
        const formValue = this.supplierForm.value;
        const supplierData = {
            ...formValue,
            contactPerson: formValue.contactPerson || '',
            email: formValue.email || '',
            notes: formValue.notes || '',
            balance: formValue.balance || 0
        };

        console.log(`📝 Submitting supplier data:`, supplierData);

        const request = this.isEditMode && this.supplierId
            ? this.supplierService.update(this.supplierId, supplierData)
            : this.supplierService.create(supplierData);

        const operation = this.isEditMode ? 'تحديث' : 'إضافة';
        console.log(`🔄 ${operation} supplier operation started`);

        request
            .pipe(finalize(() => {
                this.saving = false;
                console.log(`✅ ${operation} operation completed`);
            }))
            .subscribe({
                next: (result) => {
                    console.log(`🎉 Supplier ${operation} successful:`, result);
                    this.messageService.add({
                        severity: 'success',
                        summary: 'تم بنجاح',
                        detail: this.isEditMode ? 'تم تحديث بيانات المورد بنجاح' : 'تم إضافة المورد بنجاح'
                    });
                    setTimeout(() => {
                        console.log('🔄 Redirecting to suppliers list');
                        this.router.navigate(['/partners/suppliers']);
                    }, 1500);
                },
                error: (error) => {
                    console.error(`❌ Failed to ${operation} supplier:`, error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: this.isEditMode ? 'فشل في تحديث بيانات المورد' : 'فشل في إضافة المورد'
                    });
                }
            });
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        console.log('🔍 Marking form fields as touched');
        Object.values(formGroup.controls).forEach(control => {
            control.markAsTouched();
            if (control instanceof FormGroup) {
                this.markFormGroupTouched(control);
            }
        });
    }

    onCancel(): void {
        console.log('↩️ Cancel button clicked');

        if (this.supplierForm.dirty) {
            console.log('⚠️ Form has unsaved changes, asking for confirmation');
            if (confirm('هل أنت متأكد من الخروج؟ سيتم فقدان التغييرات غير المحفوظة.')) {
                console.log('✅ User confirmed cancellation, navigating back');
                this.router.navigate(['/partners/suppliers']);
            } else {
                console.log('❌ User cancelled the cancellation');
            }
        } else {
            console.log('✅ No unsaved changes, navigating back');
            this.router.navigate(['/partners/suppliers']);
        }
    }

    getFieldError(fieldName: string): string {
        const field = this.supplierForm.get(fieldName);

        if (!field?.touched || !field?.errors) return '';

        const errors = field.errors;

        if (errors['required']) return 'هذا الحقل مطلوب';
        if (errors['minlength']) return `يجب أن يكون ${errors['minlength'].requiredLength} أحرف على الأقل`;
        if (errors['maxlength']) return `يجب أن يكون ${errors['maxlength'].requiredLength} أحرف كحد أقصى`;
        if (errors['email']) return 'البريد الإلكتروني غير صالح';
        if (errors['pattern']) {
            if (fieldName === 'phoneNumber') {
                return 'رقم الهاتف غير صالح. استخدم أرقامًا فقط (7-15 رقم)';
            }
            return 'صيغة غير صالحة';
        }
        if (errors['min']) return 'القيمة يجب أن تكون أكبر من أو تساوي 0';

        return 'قيمة غير صالحة';
    }
}
