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
    template: `
    <p-dialog [(visible)]="visible" [header]="'تعديل مخزون يدوي - ' + batch?.companyBatchNumber"
        [modal]="true" [style]="{width: '450px'}" (onHide)="onClose()" [rtl]="true">
        <form [formGroup]="actionForm" class="flex flex-column gap-4 py-3" dir="rtl">
            <div class="field">
                <label class="block font-bold mb-2">نوع الحركة</label>
                <p-dropdown [options]="actionTypes" formControlName="movementType"
                    placeholder="اختر النوع" styleClass="w-full" appendTo="body"></p-dropdown>
            </div>

            <div class="field">
                <label class="block font-bold mb-2">الكمية</label>
                <p-inputNumber formControlName="quantity" [min]="1" [max]="batch?.remainingQuantity || 1000"
                    styleClass="w-full" inputStyleClass="w-full"
                    [placeholder]="'الكمية المتاحة: ' + (batch?.remainingQuantity || 0)"></p-inputNumber>
                <small class="text-secondary" *ngIf="actionForm.get('movementType')?.value === 'DAMAGE'">سيتم خصم هذه الكمية كتالف.</small>
            </div>

            <div class="field">
                <label class="block font-bold mb-2">السبب / ملاحظات</label>
                <textarea pInputTextarea formControlName="notes" rows="3" class="w-full"
                    placeholder="يرجى توضيح سبب الحركة..."></textarea>
                <small class="p-error block" *ngIf="actionForm.get('notes')?.invalid && actionForm.get('notes')?.touched">
                    السبب مطلوب للحركات اليدوية.
                </small>
            </div>
        </form>

        <ng-template pTemplate="footer">
            <p-button label="إلغاء" icon="pi pi-times" severity="secondary" (onClick)="onClose()" outlined></p-button>
            <p-button label="تفيذ الحركة" icon="pi pi-check" [loading]="submitting"
                (onClick)="submit()" [disabled]="actionForm.invalid"></p-button>
        </ng-template>
    </p-dialog>
    `
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

        this.submitting = true;
        const value = this.actionForm.value;

        this.inventoryService.createManualMovement({
            batchId: this.batch.id,
            movementType: value.movementType,
            quantity: value.quantity,
            notes: value.notes
        }).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تمت العملية', detail: 'تم تسجيل الحركة اليدوية بنجاح' });
                this.submitting = false;
                this.onComplete.emit();
                this.onClose();
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في تسجيل الحركة' });
                this.submitting = false;
            }
        });
    }
}
