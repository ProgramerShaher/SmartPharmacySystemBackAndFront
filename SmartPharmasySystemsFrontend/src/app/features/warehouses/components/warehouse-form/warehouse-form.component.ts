import { Component, Input, Output, EventEmitter, OnChanges, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WarehouseService } from '../../services/warehouse.service';
import { BranchService } from '../../../branches/services/branch.service';
import { WarehouseDto, WarehouseType, CreateWarehouseDto, UpdateWarehouseDto, BranchDto } from '../../../../core/models';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-warehouse-form',
    standalone: true,
    imports: [
        CommonModule, FormsModule, DialogModule, ButtonModule, InputTextModule,
        DropdownModule, ToastModule
    ],
    templateUrl: './warehouse-form.component.html',
    styleUrls: ['./warehouse-form.component.scss'],
    providers: [MessageService]
})
export class WarehouseFormComponent implements OnChanges {
    @Input() visible = false;
    @Input() warehouse: WarehouseDto | null = null;
    @Input() isEdit = false;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    branches = signal<BranchDto[]>([]);
    submitting = signal(false);

    model: any = {
        branchId: null,
        type: null,
        name: ''
    };

    typeOptions = [
        { label: 'رئيسي', value: WarehouseType.Main },
        { label: 'فرعي', value: WarehouseType.Branch },
        { label: 'تالف', value: WarehouseType.Damaged }
    ];

    WarehouseType = WarehouseType;

    constructor(
        private warehouseService: WarehouseService,
        private branchService: BranchService,
        private messageService: MessageService
    ) {
        effect(() => {
            if (this.visible) {
                this.loadBranches();
            }
        });
    }

    ngOnChanges() {
        if (this.warehouse) {
            this.model = {
                branchId: this.warehouse.branchId,
                type: this.warehouse.type,
                name: this.warehouse.name
            };
        } else {
            this.model = { branchId: null, type: null, name: '' };
        }
    }

    private loadBranches() {
        this.branchService.getActive().subscribe(data => this.branches.set(data));
    }

    get isFormValid(): boolean {
        return !!this.model.branchId && this.model.type !== null && this.model.type !== undefined
            && this.model.name?.trim()?.length > 0;
    }

    submit() {
        if (!this.isFormValid || this.submitting()) return;

        this.submitting.set(true);
        if (this.isEdit && this.warehouse) {
            const dto: UpdateWarehouseDto = {
                branchId: this.model.branchId,
                type: this.model.type,
                name: this.model.name.trim()
            };
            this.warehouseService.update(this.warehouse.id, dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم التحديث', detail: 'تم تحديث المخزن بنجاح' });
                    this.submitting.set(false);
                    this.saved.emit();
                },
                error: (err) => {
                    this.submitting.set(false);
                    const msg = err?.error?.message || 'فشل تحديث المخزن';
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: msg });
                }
            });
        } else {
            const dto: CreateWarehouseDto = {
                branchId: this.model.branchId,
                type: this.model.type,
                name: this.model.name.trim()
            };
            this.warehouseService.create(dto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم الإضافة', detail: 'تم إضافة المخزن بنجاح' });
                    this.submitting.set(false);
                    this.saved.emit();
                },
                error: (err) => {
                    this.submitting.set(false);
                    const msg = err?.error?.message || 'فشل إضافة المخزن';
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: msg });
                }
            });
        }
    }

    close() {
        this.visibleChange.emit(false);
        this.cancelled.emit();
    }
}
