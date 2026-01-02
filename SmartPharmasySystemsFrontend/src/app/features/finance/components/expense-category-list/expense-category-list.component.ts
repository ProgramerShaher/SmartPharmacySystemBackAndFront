import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { CardModule } from 'primeng/card';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ExpenseCategoryService } from '../../services/expense-category.service';
import { ExpenseCategoryDto, CreateExpenseCategoryDto, UpdateExpenseCategoryDto } from '../../../../core/models';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
    selector: 'app-expense-category-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        InputTextareaModule,
        ToastModule,
        ConfirmDialogModule,
        ProgressSpinnerModule,
        TagModule,
        TooltipModule,
        CardModule
    ],
    templateUrl: './expense-category-list.component.html',
    styleUrl: './expense-category-list.component.css',
    providers: [MessageService, ConfirmationService]
})
export class ExpenseCategoryListComponent implements OnInit {
    categories: ExpenseCategoryDto[] = [];
    loading: boolean = false;
    displayDialog: boolean = false;
    isEditMode: boolean = false;

    // Form model
    categoryForm: CreateExpenseCategoryDto | UpdateExpenseCategoryDto = {
        name: '',
        description: ''
    };

    constructor(
        private categoryService: ExpenseCategoryService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit(): void {
        this.loadCategories();
    }

    loadCategories(): void {
        this.loading = true;
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
                }),
                finalize(() => this.loading = false)
            )
            .subscribe(categories => {
                this.categories = categories;
            });
    }

    openNewDialog(): void {
        this.isEditMode = false;
        this.categoryForm = {
            name: '',
            description: ''
        };
        this.displayDialog = true;
    }

    openEditDialog(category: ExpenseCategoryDto): void {
        this.isEditMode = true;
        this.categoryForm = {
            id: category.id,
            name: category.name,
            description: category.description || ''
        };
        this.displayDialog = true;
    }

    hideDialog(): void {
        this.displayDialog = false;
        this.categoryForm = {
            name: '',
            description: ''
        };
    }

    saveCategory(): void {
        if (!this.categoryForm.name || this.categoryForm.name.trim() === '') {
            this.messageService.add({
                severity: 'warn',
                summary: 'تحذير',
                detail: 'يرجى إدخال اسم الفئة'
            });
            return;
        }

        if (this.isEditMode) {
            this.updateCategory();
        } else {
            this.createCategory();
        }
    }

    createCategory(): void {
        const dto: CreateExpenseCategoryDto = {
            name: this.categoryForm.name.trim(),
            description: this.categoryForm.description?.trim()
        };

        this.categoryService.create(dto)
            .pipe(
                catchError((error) => {
                    console.error('Failed to create category:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في إنشاء الفئة'
                    });
                    return of(null);
                })
            )
            .subscribe(result => {
                if (result) {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجح',
                        detail: 'تم إنشاء الفئة بنجاح'
                    });
                    this.hideDialog();
                    this.loadCategories();
                }
            });
    }

    updateCategory(): void {
        const dto = this.categoryForm as UpdateExpenseCategoryDto;

        this.categoryService.update(dto.id, dto)
            .pipe(
                catchError((error) => {
                    console.error('Failed to update category:', error);
                    this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'فشل في تحديث الفئة'
                    });
                    return of(null);
                })
            )
            .subscribe(() => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'نجح',
                    detail: 'تم تحديث الفئة بنجاح'
                });
                this.hideDialog();
                this.loadCategories();
            });
    }

    deleteCategory(event: Event, category: ExpenseCategoryDto): void {
        this.confirmationService.confirm({
            target: event.target as EventTarget,
            message: `هل أنت متأكد من حذف الفئة "${category.name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم',
            rejectLabel: 'لا',
            acceptButtonStyleClass: 'p-button-danger',
            rejectButtonStyleClass: 'p-button-text',
            accept: () => {
                this.categoryService.delete(category.id)
                    .pipe(
                        catchError((error) => {
                            console.error('Failed to delete category:', error);
                            this.messageService.add({
                                severity: 'error',
                                summary: 'خطأ',
                                detail: 'فشل في حذف الفئة. قد تكون مرتبطة بمصروفات موجودة'
                            });
                            return of(null);
                        })
                    )
                    .subscribe(() => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'نجح',
                            detail: 'تم حذف الفئة بنجاح'
                        });
                        this.loadCategories();
                    });
            }
        });
    }

    formatDate(date: string | Date): string {
        const d = typeof date === 'string' ? new Date(date) : date;
        return d.toLocaleDateString('ar-YE', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }
}
