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

import { UploadService } from '../../../../core/services/upload.service';

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
    uploading = false;

    // بيانات الفئة
    categoryData: CategoryDto | null = null;

    constructor(
        private fb: FormBuilder,
        private categoryService: CategoryService,
        private messageService: MessageService,
        private uploadService: UploadService
    ) {
        this.categoryForm = this.createForm();
    }

    onFileSelected(event: any) {
        const file: File = event.target.files[0];
        if (!file) return;

        // التحقق من الحجم (الحد الأقصى 2 ميجابايت) لمنع تعليق الجهاز
        if (file.size > 2 * 1024 * 1024) {
            this.messageService.add({ 
                severity: 'warn', 
                summary: 'تنبيه', 
                detail: 'حجم الصورة كبير جداً، يرجى اختيار صورة أقل من 2 ميجابايت' 
            });
            return;
        }

        this.uploading = true;
        this.uploadService.uploadCategoryImage(file, this.categoryForm.get('name')?.value || 'category').subscribe({
            next: (res: any) => {
                // نخزن المسار فقط في قاعدة البيانات
                this.categoryForm.patchValue({ imageUrl: res.imageUrl });
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ مسار الأيقونة بنجاح' });
                this.uploading = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الرفع، تأكد من تشغيل السيرفر' });
                this.uploading = false;
            }
        });
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
            description: ['', Validators.maxLength(500)],
            imageUrl: ['']
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
            description: data.description || '',
            imageUrl: data.imageUrl || ''
        });
    }

    private resetForm(): void {
        this.categoryForm.reset({
            name: '',
            description: '',
            imageUrl: ''
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
                description: formValue.description,
                imageUrl: formValue.imageUrl
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
                description: formValue.description,
                imageUrl: formValue.imageUrl
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