import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { DepartmentService } from '../../services/department.service';
import { DepartmentDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { DepartmentFormComponent } from '../department-form/department-form.component';

@Component({
    selector: 'app-department-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule,
        TooltipModule, ConfirmDialogModule, ToastModule, DepartmentFormComponent
    ],
    templateUrl: './department-list.component.html',
    styleUrls: ['./department-list.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class DepartmentListComponent implements OnInit {
    departments = signal<DepartmentDto[]>([]);
    totalRecords = signal(0);
    loading = signal(false);
    searchTerm = signal('');
    showFormDialog = signal(false);
    selectedDepartment: DepartmentDto | null = null;
    isEditMode = false;

    private searchSubject = new Subject<string>();

    constructor(
        private departmentService: DepartmentService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.searchSubject.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => this.loadDepartments());
    }

    ngOnInit() { this.loadDepartments(); }

    loadDepartments() {
        this.loading.set(true);
        this.departmentService.getAll(this.searchTerm() || undefined).subscribe({
            next: (data) => {
                this.departments.set(data);
                this.totalRecords.set(data.length);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الأقسام' });
                this.loading.set(false);
            }
        });
    }

    onSearch(value: string) {
        this.searchTerm.set(value);
        this.searchSubject.next(value);
    }

    openAddDialog() {
        this.isEditMode = false;
        this.selectedDepartment = null;
        this.showFormDialog.set(true);
    }

    openEditDialog(department: DepartmentDto) {
        this.isEditMode = true;
        this.selectedDepartment = { ...department };
        this.showFormDialog.set(true);
    }

    onFormSaved() { this.showFormDialog.set(false); this.loadDepartments(); }
    onFormCancelled() { this.showFormDialog.set(false); }

    confirmDelete(id: number, name: string) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف القسم "${name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.departmentService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف القسم بنجاح' });
                        this.loadDepartments();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف القسم' })
                });
            }
        });
    }
}
