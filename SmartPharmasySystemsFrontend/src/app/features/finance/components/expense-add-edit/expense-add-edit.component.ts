import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { RadioButtonModule } from 'primeng/radiobutton';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ExpenseService } from '../../services/expense.service';
import { ExpenseCategoryService } from '../../services/expense-category.service';
import {
    CreateExpenseDto,
    UpdateExpenseDto,
    ExpenseDto,
    ExpenseCategoryDto,
    PaymentType
} from '../../../../core/models';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
    selector: 'app-expense-add-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        InputTextareaModule,
        InputNumberModule,
        CalendarModule,
        DropdownModule,
        RadioButtonModule,
        ToastModule,
        ProgressSpinnerModule
    ],
    templateUrl: './expense-add-edit.component.html',
    styleUrl: './expense-add-edit.component.css',
    providers: [MessageService]
})
export class ExpenseAddEditComponent implements OnInit {
    expenseForm!: FormGroup;
    isEditMode: boolean = false;
    expenseId: number | null = null;
    loading: boolean = false;
    saving: boolean = false;

    categories: ExpenseCategoryDto[] = [];
    categoryOptions: any[] = [];

    paymentMethods = [
        { label: 'نقدي', value: PaymentType.Cash, icon: 'pi-wallet', color: '#10b981' },
        { label: 'آجل', value: PaymentType.Credit, icon: 'pi-credit-card', color: '#f59e0b' }
    ];

    PaymentType = PaymentType;

    constructor(
        private fb: FormBuilder,
        private expenseService: ExpenseService,
        private categoryService: ExpenseCategoryService,
        private messageService: MessageService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.initializeForm();
        this.loadCategories();
        this.checkEditMode();
    }

    initializeForm(): void {
        this.expenseForm = this.fb.group({
            categoryId: [null, Validators.required],
            amount: [null, [Validators.required, Validators.min(0.01)]],
            expenseDate: [new Date(), Validators.required],
            paymentMethod: [PaymentType.Cash, Validators.required],
            notes: ['']
        });
    }

    loadCategories(): void {
        this.categoryService.getAll()
            .pipe(
                catchError((error) => {
                    console.error('Failed to load categories:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في تحميل فئات المصروفات'
                    });
                    return of([]);
                })
            )
            .subscribe(categories => {
                this.categories = categories;
                this.categoryOptions = categories.map(c => ({
                    label: c.name,
                    value: c.id
                }));
            });
    }

    checkEditMode(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.isEditMode = true;
            this.expenseId = +id;
            this.loadExpense(this.expenseId);
        }
    }

    loadExpense(id: number): void {
        this.loading = true;
        this.expenseService.getById(id)
            .pipe(
                catchError((error) => {
                    console.error('Failed to load expense:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في تحميل بيانات المصروف'
                    });
                    this.router.navigate(['/finance/expenses']);
                    return of(null);
                }),
                finalize(() => this.loading = false)
            )
            .subscribe(expense => {
                if (expense) {
                    this.populateForm(expense);
                }
            });
    }

    populateForm(expense: ExpenseDto): void {
        this.expenseForm.patchValue({
            categoryId: expense.categoryId,
            amount: expense.amount,
            expenseDate: new Date(expense.expenseDate),
            paymentMethod: expense.paymentMethod,
            notes: expense.notes
        });
    }

    onSubmit(): void {
        if (this.expenseForm.invalid) {
            this.markFormGroupTouched(this.expenseForm);
            this.messageService.add({
                severity: 'warn',
                summary: 'تحذير',
                detail: 'يرجى ملء جميع الحقول المطلوبة'
            });
            return;
        }

        if (this.isEditMode) {
            this.updateExpense();
        } else {
            this.createExpense();
        }
    }

    createExpense(): void {
        this.saving = true;
        const formValue = this.expenseForm.value;

        const dto: CreateExpenseDto = {
            categoryId: formValue.categoryId,
            amount: formValue.amount,
            expenseDate: this.formatDate(formValue.expenseDate),
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes || ''
        };

        this.expenseService.create(dto)
            .pipe(
                catchError((error) => {
                    console.error('Failed to create expense:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: error.error?.message || 'فشل في إنشاء المصروف'
                    });
                    return of(null);
                }),
                finalize(() => this.saving = false)
            )
            .subscribe(result => {
                if (result) {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجح',
                        detail: 'تم إنشاء المصروف بنجاح'
                    });
                    setTimeout(() => {
                        this.router.navigate(['/finance/expenses']);
                    }, 1000);
                }
            });
    }

    updateExpense(): void {
        if (!this.expenseId) return;

        this.saving = true;
        const formValue = this.expenseForm.value;

        const dto: UpdateExpenseDto = {
            id: this.expenseId,
            categoryId: formValue.categoryId,
            amount: formValue.amount,
            expenseDate: this.formatDate(formValue.expenseDate),
            paymentMethod: formValue.paymentMethod,
            notes: formValue.notes || ''
        };

        this.expenseService.update(this.expenseId, dto)
            .pipe(
                catchError((error) => {
                    console.error('Failed to update expense:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: error.error?.message || 'فشل في تحديث المصروف'
                    });
                    return of(null);
                }),
                finalize(() => this.saving = false)
            )
            .subscribe(result => {
                if (result) {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجح',
                        detail: 'تم تحديث المصروف بنجاح'
                    });
                    setTimeout(() => {
                        this.router.navigate(['/finance/expenses']);
                    }, 1000);
                }
            });
    }

    cancel(): void {
        this.router.navigate(['/finance/expenses']);
    }

    formatDate(date: Date): string {
        return date.toISOString().split('T')[0];
    }

    markFormGroupTouched(formGroup: FormGroup): void {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            control?.markAsTouched();
        });
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.expenseForm.get(fieldName);
        return !!(field && field.invalid && (field.dirty || field.touched));
    }

    getFieldError(fieldName: string): string {
        const field = this.expenseForm.get(fieldName);
        if (field?.hasError('required')) {
            return 'هذا الحقل مطلوب';
        }
        if (field?.hasError('min')) {
            return 'يجب أن يكون المبلغ أكبر من صفر';
        }
        return '';
    }
}
