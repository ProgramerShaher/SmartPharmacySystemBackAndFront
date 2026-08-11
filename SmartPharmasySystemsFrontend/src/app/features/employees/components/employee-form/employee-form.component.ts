import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../../services/employee.service';
import { EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto, BranchDto, DepartmentDto, User } from '../../../../core/models';
import { UsersService } from '../../../users/services/users.service';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-employee-form',
    standalone: true,
    imports: [
        CommonModule, FormsModule, DialogModule, ButtonModule, InputTextModule,
        DropdownModule, CalendarModule, InputNumberModule, InputSwitchModule, ToastModule
    ],
    templateUrl: './employee-form.component.html',
    styleUrls: ['./employee-form.component.scss'],
    providers: [MessageService]
})
export class EmployeeFormComponent implements OnInit {
    @Input() visible = false;
    @Input() employee: EmployeeDto | null = null;
    @Input() editMode = false;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    saving = signal(false);
    branches = signal<BranchDto[]>([]);
    departments = signal<DepartmentDto[]>([]);
    users = signal<User[]>([]);

    errors: Record<string, string> = {};

    shiftOptions = [
        { label: 'صباحي', value: 1 },
        { label: 'مسائي', value: 2 },
        { label: 'ليلي', value: 3 }
    ];

    model: any = {
        employeeCode: '',
        fullName: '',
        nationalId: '',
        branchId: 0,
        departmentId: 0,
        jobTitle: '',
        hireDate: '',
        basicSalary: 0,
        isActive: true,
        userId: null,
        shift: 1,
        shiftStartTime: null,
        shiftEndTime: null,
        workingHours: 8
    };

    constructor(
        private employeeService: EmployeeService,
        private usersService: UsersService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadDropdowns();
        if (this.editMode && this.employee) {
            this.model = {
                employeeCode: this.employee.employeeCode,
                fullName: this.employee.fullName,
                nationalId: this.employee.nationalId,
                branchId: this.employee.branchId,
                departmentId: this.employee.departmentId,
                jobTitle: this.employee.jobTitle,
                hireDate: typeof this.employee.hireDate === 'string'
                    ? this.employee.hireDate.substring(0, 10)
                    : this.employee.hireDate,
                basicSalary: this.employee.basicSalary,
                isActive: this.employee.isActive,
                userId: this.employee.userId,
                shift: this.employee.shift ?? 1,
                shiftStartTime: this.employee.shiftStartTime ? new Date(`1970-01-01T${this.employee.shiftStartTime}`) : null,
                shiftEndTime: this.employee.shiftEndTime ? new Date(`1970-01-01T${this.employee.shiftEndTime}`) : null,
                workingHours: this.employee.workingHours ?? 8
            };
        }
    }

    loadDropdowns() {
        this.employeeService.getBranches().subscribe({
            next: (data) => this.branches.set(data),
            error: () => { }
        });
        this.employeeService.getDepartments().subscribe({
            next: (data) => this.departments.set(data),
            error: () => { }
        });
        this.usersService.search({}).subscribe({
            next: (data) => this.users.set(data.items),
            error: () => { }
        });
    }

    validate(): boolean {
        this.errors = {};
        if (!this.model.employeeCode.trim()) this.errors['employeeCode'] = 'كود الموظف مطلوب';
        if (!this.model.fullName.trim()) this.errors['fullName'] = 'الاسم الكامل مطلوب';
        if (!this.model.nationalId.trim()) this.errors['nationalId'] = 'رقم الهوية مطلوب';
        if (!this.model.jobTitle.trim()) this.errors['jobTitle'] = 'المسمى الوظيفي مطلوب';
        if (!this.model.branchId) this.errors['branchId'] = 'الفرع مطلوب';
        if (!this.model.departmentId) this.errors['departmentId'] = 'القسم مطلوب';
        if (!this.model.hireDate) this.errors['hireDate'] = 'تاريخ التعيين مطلوب';
        if (this.model.basicSalary < 0) this.errors['basicSalary'] = 'الراتب لا يمكن أن يكون سالباً';
        if (!this.model.shift) this.errors['shift'] = 'تحديد الوردية مطلوب';
        if (this.model.workingHours <= 0 || this.model.workingHours > 24) this.errors['workingHours'] = 'ساعات العمل يجب أن تكون بين 1 و 24';
        return Object.keys(this.errors).length === 0;
    }

    save() {
        if (!this.validate()) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تصحيح الأخطاء قبل الحفظ' });
            return;
        }

        this.saving.set(true);

        const rawHireDate = this.model.hireDate;
        const hireDateStr = rawHireDate instanceof Date
            ? rawHireDate.toISOString()
            : String(rawHireDate);

        const dto: CreateEmployeeDto = {
            employeeCode: this.model.employeeCode,
            fullName: this.model.fullName,
            nationalId: this.model.nationalId,
            branchId: this.model.branchId,
            departmentId: this.model.departmentId,
            jobTitle: this.model.jobTitle,
            hireDate: hireDateStr,
            basicSalary: this.model.basicSalary,
            isActive: this.model.isActive,
            userId: this.model.userId,
            shift: this.model.shift,
            shiftStartTime: this.model.shiftStartTime ? (this.model.shiftStartTime as Date).toTimeString().split(' ')[0] : undefined,
            shiftEndTime: this.model.shiftEndTime ? (this.model.shiftEndTime as Date).toTimeString().split(' ')[0] : undefined,
            workingHours: this.model.workingHours
        };

        if (this.editMode && this.employee) {
            const updateDto: UpdateEmployeeDto = { ...dto, id: this.employee.id };
            this.employeeService.update(updateDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث بيانات الموظف' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحديث بيانات الموظف' });
                    this.saving.set(false);
                }
            });
        } else {
            this.employeeService.create(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إضافة الموظف بنجاح' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إضافة الموظف' });
                    this.saving.set(false);
                }
            });
        }
    }

    cancel() {
        this.cancelled.emit();
    }

    hasError(field: string): boolean {
        return !!this.errors[field];
    }
}
