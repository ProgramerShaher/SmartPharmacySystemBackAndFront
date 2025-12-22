// medicine-add-edit.component.ts
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Category } from '../../../../core/models';

interface CategoryStats {
    totalMedicines: number;
    availableMedicines: number;
    totalSales: number;
}

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
    selectedColor = 'primary';
    selectedIcon = 'pi pi-folder';

    // بيانات الفئة
    categoryData: Category | null = null;
    categoryStats: CategoryStats | null = null;

    // خيارات الألوان
    colorOptions = [
        { value: 'primary', name: 'أزرق أساسي', color: '#667eea' },
        { value: 'success', name: 'أخضر', color: '#10b981' },
        { value: 'danger', name: 'أحمر', color: '#ef4444' },
        { value: 'warning', name: 'برتقالي', color: '#f59e0b' },
        { value: 'info', name: 'سماوي', color: '#3b82f6' },
        { value: 'purple', name: 'بنفسجي', color: '#8b5cf6' },
        { value: 'pink', name: 'وردي', color: '#ec4899' },
        { value: 'teal', name: 'تركواز', color: '#14b8a6' }
    ];

    // خيارات الأيقونات
    iconOptions = [
        { value: 'pi pi-folder', name: 'مجلد' },
        { value: 'pi pi-capsule', name: 'كبسولة' },
        { value: 'pi pi-heart', name: 'قلب' },
        { value: 'pi pi-star', name: 'نجمة' },
        { value: 'pi pi-shield', name: 'درع' },
        { value: 'pi pi-box', name: 'صندوق' },
        { value: 'pi pi-tag', name: 'علامة' },
        { value: 'pi pi-bolt', name: 'برق' },
        { value: 'pi pi-pill', name: 'حبة دواء' },
        { value: 'pi pi-flask', name: 'قارورة' },
        { value: 'pi pi-plus-circle', name: 'إضافة' },
        { value: 'pi pi-check-circle', name: 'تحقق' }
    ];

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
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
            description: ['', Validators.maxLength(500)],
            color: ['primary'],
            icon: ['pi pi-folder']
        });
    }

    private loadCategoryData(): void {
        if (!this.categoryId) return;

        this.inventoryService.getCategoryById(this.categoryId).subscribe({
            next: (data) => {
                this.categoryData = data;
                this.patchFormValues(data);
                this.loadCategoryStats();
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

    private loadCategoryStats(): void {
        if (!this.categoryId) return;

        this.inventoryService.getCategoryStats(this.categoryId).subscribe({
            next: (stats) => {
                this.categoryStats = stats;
            },
            error: (err) => {
                console.error('Error loading category stats:', err);
            }
        });
    }

    private patchFormValues(data: Category): void {
        this.categoryForm.patchValue({
            name: data.name,
            description: data.description || '',
            color: data.color || 'primary',
            icon: data.icon || 'pi pi-folder'
        });

        this.selectedColor = data.color || 'primary';
        this.selectedIcon = data.icon || 'pi pi-folder';
    }

    private resetForm(): void {
        this.categoryForm.reset({
            name: '',
            description: '',
            color: 'primary',
            icon: 'pi pi-folder'
        });

        this.selectedColor = 'primary';
        this.selectedIcon = 'pi pi-folder';
        this.categoryData = null;
        this.categoryStats = null;
    }

    selectColor(color: string): void {
        this.selectedColor = color;
        this.categoryForm.patchValue({ color });
    }

    selectIcon(icon: string): void {
        this.selectedIcon = icon;
        this.categoryForm.patchValue({ icon });
    }

    saveCategory(): void {
        if (this.categoryForm.invalid) {
            this.markFormGroupTouched(this.categoryForm);
            return;
        }

        this.saving = true;
        const formValue = this.categoryForm.value;

        if (this.editMode && this.categoryId) {
            const payload = {
                ...formValue,
                id: this.categoryId
            };

            this.inventoryService.updateCategory(this.categoryId, payload).subscribe({
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
            this.inventoryService.createCategory(formValue).subscribe({
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

    getCategoryColor(): string {
        const value = this.selectedColor;
        const option = this.colorOptions.find(o => o.value === value);
        return option ? option.color : '#667eea';
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        Object.values(formGroup.controls).forEach(control => {
            control.markAsDirty();
            control.markAsTouched();
        });
    }

    get name() { return this.categoryForm.get('name')!; }
    get description() { return this.categoryForm.get('description')!; }
    get color() { return this.categoryForm.get('color')!; }
    get icon() { return this.categoryForm.get('icon')!; }
}