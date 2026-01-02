import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../../../../core/models';

@Component({
    selector: 'app-category-add-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        InputTextareaModule,
        TooltipModule,
        ToastModule,
        ProgressSpinnerModule
    ],
    templateUrl: './category-add-edit.component.html',
    styleUrls: ['./category-add-edit.component.scss']
})
export class CategoryAddEditComponent implements OnInit, OnChanges {
    @Input() categoryId: number | null = null;
    @Output() onSave = new EventEmitter<void>();
    @Output() onCancel = new EventEmitter<void>();

    categoryForm: FormGroup;
    editMode = false;
    saving = false;

    // بيانات الفئة
    categoryData: CategoryDto | null = null;

    constructor(
        private fb: FormBuilder,
        private categoryService: CategoryService,
        private messageService: MessageService
    ) {
        this.categoryForm = this.createForm();
    }

    ngOnInit(): void {
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['categoryId']) {
            if (this.categoryId) {
                this.editMode = true;
                this.loadCategoryData();
            } else {
                this.editMode = false;
                this.resetForm();
            }
        }
    }

    private createForm(): FormGroup {
        return this.fb.group({
            name: ['', [
                Validators.required,
                Validators.minLength(2),
                Validators.maxLength(100)
            ]],
            description: ['', Validators.maxLength(500)]
        });
    }

    private loadCategoryData(): void {
        if (!this.categoryId) return;

        this.categoryService.getById(this.categoryId).subscribe({
            next: (data) => {
                this.categoryData = data;
                this.patchFormValues(data);
            },
            error: (err) => {
                console.error('Error loading category:', err);
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل بيانات الفئة'
                });
            }
        });
    }

    private patchFormValues(data: CategoryDto): void {
        this.categoryForm.patchValue({
            name: data.name,
            description: data.description || ''
        });
    }

    private resetForm(): void {
        this.categoryForm.reset({
            name: '',
            description: ''
        });
        this.categoryData = null;
    }

    saveCategory(): void {
        if (this.categoryForm.invalid) {
            this.markFormGroupTouched(this.categoryForm);
            return;
        }

        this.saving = true;
        const formValue = this.categoryForm.value;

        if (this.editMode && this.categoryId) {
            const updateDto: UpdateCategoryDto = {
                id: this.categoryId,
                name: formValue.name,
                description: formValue.description
            };

            this.categoryService.update(this.categoryId, updateDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم تحديث الفئة بنجاح' });
                    this.onSave.emit();
                },
                error: (err) => {
                    console.error('Update Category Error:', err);
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء تحديث الفئة' });
                },
                complete: () => {
                    this.saving = false;
                }
            });
        } else {
            const createDto: CreateCategoryDto = {
                name: formValue.name,
                description: formValue.description
            };

            this.categoryService.create(createDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم إضافة الفئة بنجاح' });
                    this.onSave.emit();
                },
                error: (err) => {
                    console.error('Create Category Error:', err);
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء إضافة الفئة' });
                },
                complete: () => {
                    this.saving = false;
                }
            });
        }
    }

    cancel(): void {
        this.onCancel.emit();
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        Object.values(formGroup.controls).forEach(control => {
            control.markAsDirty();
            control.markAsTouched();
        });
    }

    get name() { return this.categoryForm.get('name')!; }
    get description() { return this.categoryForm.get('description')!; }
}