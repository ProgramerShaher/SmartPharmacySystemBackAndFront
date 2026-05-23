import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BranchService } from '../../services/branch.service';
import { BranchDto, CreateBranchDto, UpdateBranchDto } from '../../../../core/models';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-branch-form',
    standalone: true,
    imports: [CommonModule, FormsModule, DialogModule, ButtonModule, InputTextModule, DropdownModule, InputSwitchModule, ToastModule],
    templateUrl: './branch-form.component.html',
    styleUrls: ['./branch-form.component.scss'],
    providers: [MessageService]
})
export class BranchFormComponent implements OnInit {
    @Input() visible = false;
    @Input() branch: BranchDto | null = null;
    @Input() editMode = false;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    saving = signal(false);

    model: any = {
        branchCode: '',
        name: '',
        location: '',
        branchType: 2,
        isActive: true
    };

    branchTypeOptions = [
        { label: 'رئيسي', value: 1 },
        { label: 'فرعي', value: 2 }
    ];

    constructor(
        private branchService: BranchService,
        private messageService: MessageService
    ) {}

    ngOnInit() {
        if (this.editMode && this.branch) {
            this.model = {
                branchCode: this.branch.branchCode,
                name: this.branch.name,
                location: this.branch.location || '',
                branchType: this.branch.branchType,
                isActive: this.branch.isActive
            };
        }
    }

    save() {
        if (!this.model.branchCode.trim() || !this.model.name.trim()) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى ملء الحقول الإلزامية' });
            return;
        }

        this.saving.set(true);

        if (this.editMode && this.branch) {
            const dto: UpdateBranchDto = { ...this.model, id: this.branch.id };
            this.branchService.update(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الفرع' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => { this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التحديث' }); this.saving.set(false); }
            });
        } else {
            const dto: CreateBranchDto = { ...this.model };
            this.branchService.create(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إضافة الفرع' });
                    this.saving.set(false);
                    this.saved.emit();
                },
                error: () => { this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الإضافة' }); this.saving.set(false); }
            });
        }
    }

    cancel() { this.cancelled.emit(); }
}
