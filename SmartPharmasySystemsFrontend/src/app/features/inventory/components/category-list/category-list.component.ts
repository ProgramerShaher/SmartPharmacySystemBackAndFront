import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { Category, CategoryQueryDto } from '../../../../core/models';
import { CategoryService } from '../../services/category.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

// Child Component
import { CategoryAddEditComponent } from '../category-add-edit/category-add-edit.component';

@Component({
    selector: 'app-category-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        TableModule,
        ToolbarModule,
        ButtonModule,
        InputTextModule,
        DialogModule,
        ConfirmDialogModule,
        TooltipModule,
        CategoryAddEditComponent
    ],
    templateUrl: './category-list.component.html'
})
export class CategoryListComponent implements OnInit {
    categories: Category[] = [];
    totalRecords = 0;
    loading = true;

    displayModal = false;
    selectedCategoryId: number | null = null;
    selectedCategories: Category[] = [];

    searchSubject = new Subject<string>();
    query: CategoryQueryDto = { page: 1, pageSize: 10 };

    constructor(
        private inventoryService: InventoryService,
        private categoryService: CategoryService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadCategories();

        this.searchSubject.pipe(
            debounceTime(400),
            distinctUntilChanged()
        ).subscribe(searchText => {
            this.query.search = searchText;
            this.query.page = 1;
            this.loadCategories();
        });
    }

    loadCategories(event?: any) {
        this.loading = true;

        if (event) {
            this.query.page = (event.first / event.rows) + 1;
            this.query.pageSize = event.rows;
        }

        this.inventoryService.getAllCategories(this.query).subscribe({
            next: (res) => {
                this.categories = res.items;
                this.totalRecords = res.totalCount;
                this.loading = false;
            },
            error: (err) => {
                console.error('Error loading categories:', err);
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الفئات' });
            }
        });
    }

    onSearch(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        this.searchSubject.next(value);
    }

    showAddModal() {
        this.selectedCategoryId = null;
        this.displayModal = true;
    }

    editCategory(category: Category) {
        this.selectedCategoryId = category.id;
        this.displayModal = true;
    }

    deleteCategory(category: Category) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف الفئة "${category.name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم',
            rejectLabel: 'لا',
            accept: () => {
                this.inventoryService.deleteCategory(category.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف الفئة بنجاح' });
                        this.loadCategories();
                    },
                    error: (err) => {
                        console.error('Delete Category Error:', err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء حذف الفئة' });
                    }
                });
            }
        });
    }

    deleteSelectedCategories() {
        if (!this.selectedCategories || this.selectedCategories.length === 0) return;

        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف ${this.selectedCategories.length} فئة؟`,
            header: 'تأكيد الحذف الجماعي',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                const ids = this.selectedCategories.map(c => c.id);
                this.categoryService.deleteBulk(ids).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الفئات بنجاح' });
                        this.selectedCategories = [];
                        this.loadCategories();
                    },
                    error: (err) => {
                        console.error('Delete Bulk Categories Error:', err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء الحذف' });
                    }
                });
            }
        });
    }

    downloadTemplate() {
        this.categoryService.downloadTemplate();
    }

    onFileSelected(event: any) {
        const file = event.target.files[0];
        if (file) {
            this.loading = true;
            this.categoryService.importFromExcel(file).subscribe({
                next: (res: any) => {
                    this.loading = false;
                    const successMsg = `تم استيراد ${res.data.successCount} صنف بنجاح، وتحديث ${res.data.updatedCount} صنف.`;
                    this.messageService.add({ severity: 'success', summary: 'نجاح الاستيراد', detail: successMsg });

                    if (res.data.failedCount > 0) {
                        this.messageService.add({ severity: 'warn', summary: 'بعض الأسطر فشلت', detail: `فشل استيراد ${res.data.failedCount} سطر.` });
                    }
                    this.loadCategories();
                },
                error: (err) => {
                    this.loading = false;
                    console.error('Import Error:', err);
                    this.messageService.add({ severity: 'error', summary: 'فشل الاستيراد', detail: err.error?.message || 'حدث خطأ غير متوقع' });
                }
            });
            // Reset input
            event.target.value = '';
        }
    }

    onModalSave() {
        this.displayModal = false;
        this.loadCategories();
    }

    onModalCancel() {
        this.displayModal = false;
    }
}
