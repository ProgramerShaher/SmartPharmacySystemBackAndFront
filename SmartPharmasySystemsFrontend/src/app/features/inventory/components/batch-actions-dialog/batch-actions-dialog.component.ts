import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { InventoryService } from '../../services/inventory.service';
import { MedicineBatch, StockMovementType } from '../../../../core/models';

@Component({
    selector: 'app-batch-actions-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        DialogModule,
        ButtonModule,
        InputNumberModule,
        InputTextareaModule,
        DropdownModule
    ],
    templateUrl: './batch-actions-dialog.component.html',
    styleUrls: ['./batch-actions-dialog.component.scss']
})

export class BatchActionsDialogComponent implements OnInit {
    @Input() visible = false;
    @Input() batch: MedicineBatch | null = null;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() onClose = new EventEmitter<void>();
    @Output() onSuccess = new EventEmitter<void>();

    actionForm: FormGroup;
    loading = false;

    actionTypes = [
        { label: 'تالف (Damage)', value: StockMovementType.Damage },
        { label: 'تسوية (Adjustment)', value: StockMovementType.Adjustment }
    ];

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) {
        this.actionForm = this.fb.group({
            movementType: [null, Validators.required],
            quantity: [null, [Validators.required, Validators.min(1)]],
            notes: ['', [Validators.required, Validators.minLength(5)]]
        });
    }

    ngOnInit() {
        if (this.batch) {
            this.actionForm.get('quantity')?.setValidators([
                Validators.required,
                Validators.min(1),
                Validators.max(this.batch.remainingQuantity)
            ]);
            this.actionForm.get('quantity')?.updateValueAndValidity();
        }
    }

    submit() {
        if (this.actionForm.invalid || !this.batch) return;

        // Validate medicineId exists
        if (!this.batch.medicineId) {
            this.messageService.add({
                severity: 'error',
                summary: 'خطأ في البيانات',
                detail: 'معرف الدواء (medicineId) غير موجود في بيانات الدفعة.'
            });
            console.error('❌ Missing medicineId in batch:', this.batch);
            return;
        }

        this.loading = true;
        const value = this.actionForm.value;

        // Map movement type to number
        const typeMap: { [key: number]: number } = {
            [StockMovementType.Adjustment]: 5,
            [StockMovementType.Damage]: 6,
            [StockMovementType.Expiry]: 7
        };

        const payload = {
            medicineId: this.batch.medicineId,
            batchId: this.batch.id,
            quantity: value.quantity,
            type: typeMap[value.movementType] || 5,
            reason: value.notes,
            approvedBy: 0
        };

        console.log('📤 Sending manual movement:', payload);

        this.inventoryService.createManualMovement(payload).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم تسجيل الحركة اليدوية بنجاح' });
                this.onSuccess.emit();
                this.loading = false;
            },
            error: (err: any) => {
                console.error('❌ Error creating manual movement:', err);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في حفظ الحركة اليدوية' });
                this.loading = false;
            }
        });
    }

    close() {
        this.visible = false;
        this.visibleChange.emit(false);
        this.onClose.emit();
    }
}
