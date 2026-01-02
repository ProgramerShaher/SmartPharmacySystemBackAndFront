import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { MedicineBatch } from '../../../../core/models';

@Component({
    selector: 'app-sale-invoice-actions-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        DialogModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        DropdownModule,
        InputTextareaModule
    ],
    templateUrl: './sale-invoice-actions-dialog.component.html',
    styleUrls: ['./sale-invoice-actions-dialog.component.scss']
})
export class SaleInvoiceActionsDialogComponent {
    @Input() visible = false;
    @Input() batch: MedicineBatch | null = null;
    @Output() onComplete = new EventEmitter<void>();
    @Output() onHide = new EventEmitter<void>();

    actionForm: FormGroup;
    submitting = false;

    actionTypes = [
        { label: 'تالف (Damage)', value: 'DAMAGE' },
        { label: 'تسوية (Adjustment)', value: 'ADJUSTMENT' }
    ];

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) {
        this.actionForm = this.fb.group({
            movementType: ['DAMAGE', Validators.required],
            quantity: [1, [Validators.required, Validators.min(1)]],
            notes: ['', [Validators.required, Validators.minLength(5)]]
        });
    }

    onClose() {
        this.visible = false;
        this.onHide.emit();
        this.actionForm.reset({ movementType: 'DAMAGE', quantity: 1, notes: '' });
    }

    submit() {
        if (this.actionForm.invalid || !this.batch) return;

        // Validate medicineId exists
        if (!this.batch.medicineId) {
            this.messageService.add({
                severity: 'error',
                summary: 'خطأ في البيانات',
                detail: 'معرف الدواء (medicineId) غير موجود في بيانات الدفعة. يرجى التحقق من البيانات.'
            });
            console.error('❌ Missing medicineId in batch:', this.batch);
            return;
        }

        this.submitting = true;
        const value = this.actionForm.value;

        // Map movement type string to number
        const typeMap: { [key: string]: number } = {
            'ADJUSTMENT': 5,
            'DAMAGE': 6,
            'EXPIRY': 7
        };

        const payload = {
            medicineId: this.batch.medicineId,
            batchId: this.batch.id,
            quantity: value.quantity,
            type: typeMap[value.movementType] || 5,
            reason: value.notes,
            approvedBy: 0
        };

        // Log payload for debugging
        console.log('📤 Sending manual movement:', payload);

        this.inventoryService.createManualMovement(payload).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تمت العملية', detail: 'تم تسجيل الحركة اليدوية بنجاح' });
                this.submitting = false;
                this.onComplete.emit();
                this.onClose();
            },
            error: (err) => {
                console.error('❌ Error creating manual movement:', err);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في تسجيل الحركة' });
                this.submitting = false;
            }
        });
    }
}
