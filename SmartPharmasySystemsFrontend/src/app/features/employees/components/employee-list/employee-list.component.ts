import { Component, OnInit, signal, computed, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EmployeeService } from '../../services/employee.service';
import { EmployeeDto } from '../../../../core/models';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectButtonModule } from 'primeng/selectbutton';
import { CheckboxModule } from 'primeng/checkbox';
import { EmployeeFormComponent } from '../employee-form/employee-form.component';

interface ColumnDef {
    field: string;
    header: string;
    visible: boolean;
    width?: string;
    type?: 'text' | 'number' | 'date' | 'boolean' | 'currency';
}

@Component({
    selector: 'app-employee-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        ToastModule,
        DropdownModule,
        DialogModule,
        ToggleButtonModule,
        MultiSelectModule,
        SelectButtonModule,
        CheckboxModule,
        EmployeeFormComponent
    ],
    templateUrl: './employee-list.component.html',
    styleUrls: ['./employee-list.component.scss'],
    providers: [ConfirmationService, MessageService]
})
export class EmployeeListComponent implements OnInit {
    employees = signal<EmployeeDto[]>([]);
    filteredEmployees = signal<EmployeeDto[]>([]);
    loading = signal(false);
    showFilters = signal(false);
    showColumnManager = signal(false);
    showFormDialog = signal(false);
    selectedEmployee: EmployeeDto | null = null;
    isEditMode = false;

    // Filters
    globalFilter = signal('');
    filters: { [key: string]: string } = {};

    // Columns definition
    columns = signal<ColumnDef[]>([
        { field: 'employeeCode', header: 'كود الموظف', visible: true, width: '120px' },
        { field: 'fullName', header: 'الاسم الكامل', visible: true, width: '180px' },
        { field: 'jobTitle', header: 'المسمى الوظيفي', visible: true, width: '150px' },
        { field: 'nationalId', header: 'رقم الهوية', visible: true, width: '140px' },
        { field: 'branchName', header: 'الفرع', visible: true, width: '140px' },
        { field: 'departmentName', header: 'القسم', visible: true, width: '140px' },
        { field: 'hireDate', header: 'تاريخ التعيين', visible: true, width: '130px', type: 'date' },
        { field: 'basicSalary', header: 'الراتب', visible: true, width: '120px', type: 'currency' },
        { field: 'isActive', header: 'الحالة', visible: true, width: '100px', type: 'boolean' },
        { field: 'totalLoans', header: 'إجمالي السلف', visible: false, width: '120px', type: 'currency' },
        { field: 'remainingLoans', header: 'السلف المتبقية', visible: false, width: '130px', type: 'currency' }
    ]);

    visibleColumns = computed(() => this.columns().filter(c => c.visible));

    constructor(
        private employeeService: EmployeeService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {}

    ngOnInit() {
        this.loadEmployees();
    }

    loadEmployees() {
        this.loading.set(true);
        this.employeeService.getAll().subscribe({
            next: (data) => {
                this.employees.set(data);
                this.applyFilters();
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات الموظفين' });
                this.loading.set(false);
            }
        });
    }

    applyFilters() {
        let result = this.employees();
        const global = this.globalFilter()?.toLowerCase().trim();

        if (global) {
            result = result.filter(e =>
                e.fullName.toLowerCase().includes(global) ||
                e.employeeCode.toLowerCase().includes(global) ||
                e.nationalId.toLowerCase().includes(global) ||
                e.jobTitle.toLowerCase().includes(global) ||
                e.branchName.toLowerCase().includes(global) ||
                e.departmentName.toLowerCase().includes(global)
            );
        }

        Object.keys(this.filters).forEach(key => {
            const val = this.filters[key]?.toLowerCase().trim();
            if (val) {
                result = result.filter((e: any) => {
                    const fieldVal = e[key];
                    if (fieldVal === null || fieldVal === undefined) return false;
                    return String(fieldVal).toLowerCase().includes(val);
                });
            }
        });

        this.filteredEmployees.set(result);
    }

    onColumnFilter(field: string, event: any) {
        this.filters[field] = event.target?.value || '';
        this.applyFilters();
    }

    toggleFilters() {
        this.showFilters.update(v => !v);
    }

    toggleColumnManager() {
        this.showColumnManager.update(v => !v);
    }

    toggleColumnVisibility(field: string) {
        this.columns.update(cols =>
            cols.map(c => c.field === field ? { ...c, visible: !c.visible } : c)
        );
    }

    openAddDialog() {
        this.isEditMode = false;
        this.selectedEmployee = null;
        this.showFormDialog = true;
    }

    openEditDialog(employee: EmployeeDto) {
        this.isEditMode = true;
        this.selectedEmployee = { ...employee };
        this.showFormDialog = true;
    }

    onFormSaved() {
        this.showFormDialog = false;
        this.selectedEmployee = null;
        this.loadEmployees();
    }

    onFormCancelled() {
        this.showFormDialog = false;
        this.selectedEmployee = null;
    }

    confirmDelete(id: number, name: string) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف الموظف "${name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.employeeService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف الموظف بنجاح' });
                        this.loadEmployees();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف الموظف' })
                });
            }
        });
    }

    getFieldValue(employee: EmployeeDto, field: string): any {
        return (employee as any)[field];
    }

    getActiveSeverity(isActive: boolean): 'success' | 'danger' {
        return isActive ? 'success' : 'danger';
    }

    getActiveLabel(isActive: boolean): string {
        return isActive ? 'نشط' : 'غير نشط';
    }
}
