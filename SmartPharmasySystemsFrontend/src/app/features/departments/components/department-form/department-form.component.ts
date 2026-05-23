import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../services/department.service';
import { DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto } from '../../../../core/models';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-department-form',
    standalone: true,
    imports: [CommonModule, FormsModule, DialogModule, ButtonModule, InputTextModule, ToastModule],
    templateUrl: './department-form.component.html',
    styleUrls: ['./department-form.component.scss'],
    providers: [MessageService]
})
export class DepartmentFormComponent implements OnInit {
    @Input() visible = false;
    @Input() department: DepartmentDto | null = null;
    @Input() editMode = false;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    saving = signal(false);
    model = { name: '' };

    constructor(
        private departmentService: DepartmentService,
        private messageService: MessageService
    ) {}

    ngOnInit() {
        if (this.editMode && this.department) {
            this.model = { name: this.department.name };
        }
    }

    save() {
        if (!this.model.name.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'اسم القسم مطلوب' });
            return;
        }

        this.saving.set(true);

        if (this.editMode && this.department) {
            const dto: UpdateDepartmentDto = { id: this.department.id, name: this.model.name };
            this.departmentService.update(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث القسم' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => { this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التحديث' }); this.saving.set(false); }
            });
        } else {
            const dto: CreateDepartmentDto = { name: this.model.name };
            this.departmentService.create(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إضافة القسم' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => { this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الإضافة' }); this.saving.set(false); }
            });
        }
    }

    cancel() { this.cancelled.emit(); }
}
