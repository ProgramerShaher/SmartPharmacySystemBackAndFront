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
    template: `
    <p-dialog [header]="'تسجيل حركة يدوية - ' + batch?.companyBatchNumber" [(visible)]="visible" [modal]="true" 
        [style]="{width: '500px'}" [dir]="'rtl'" (onHide)="close()">
        <form [formGroup]="actionForm" (ngSubmit)="submit()" class="p-fluid mt-3">
            <div class="field mb-4">
                <label for="type" class="block mb-2 font-bold">نوع الحركة</label>
                <p-dropdown id="type" [options]="actionTypes" formControlName="movementType" placeholder="اختر النوع" styleClass="border-round-xl"></p-dropdown>
                <small class="text-red-500" *ngIf="actionForm.get('movementType')?.touched && actionForm.get('movementType')?.invalid">يرجى تحديد النوع</small>
            </div>

            <div class="field mb-4">
                <label for="quantity" class="block mb-2 font-bold">الكمية</label>
                <p-inputNumber id="quantity" formControlName="quantity" [min]="1" [max]="batch?.remainingQuantity || 0" 
                    placeholder="الكمية المراد تعديلها" styleClass="border-round-xl">
                </p-inputNumber>
                <small class="text-secondary block mt-1">الرصيد المتاح: {{ batch?.remainingQuantity }}</small>
                <small class="text-red-500" *ngIf="actionForm.get('quantity')?.touched && actionForm.get('quantity')?.invalid">الكمية غير صحيحة أو تتجاوز المتاح</small>
            </div>

            <div class="field mb-4">
                <label for="notes" class="block mb-2 font-bold">السبب / ملاحظات</label>
                <textarea id="notes" pInputTextarea formControlName="notes" rows="3" placeholder="اكتب سبب الحركة هنا..." class="border-round-xl"></textarea>
                <small class="text-red-500" *ngIf="actionForm.get('notes')?.touched && actionForm.get('notes')?.invalid">السبب مطلوب للحركات اليدوية</small>
            </div>

            <div class="flex justify-content-end gap-2 mt-4">
                <button pButton type="button" label="إلغاء" class="p-button-text border-round-xl" (click)="close()"></button>
                <button pButton type="submit" label="حفظ الحركة" icon="pi pi-check" class="p-button-primary border-round-xl shadow-1" [disabled]="actionForm.invalid || loading"></button>
            </div>
        </form>
    </p-dialog>
  `
})
export class BatchActionsDialogComponent implements OnInit {
    @Input() visible = false;
    @Input() batch: MedicineBatch | null = null;
    @Output() onClose = new EventEmitter<void>();
    @Output() onSuccess = new EventEmitter<void>();

    actionForm: FormGroup;
    loading = false;

    actionTypes = [
        { label: 'تالف (Damage)', value: StockMovementType.DAMAGE },
        { label: 'تسوية (Adjustment)', value: StockMovementType.ADJUSTMENT }
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

        this.loading = true;
        const payload = {
            batchId: this.batch.id,
            ...this.actionForm.value
        };

        // Ensure quantity is negative for damage/adjustment if backend expects it to be subtracted
        // Usually backend handles this based on type, but we should align.
        // Assuming backend takes positive quantity and subtracts based on type 'DAMAGE'.

        this.inventoryService.createManualMovement(payload).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم الحفظ', detail: 'تم تسجيل الحركة اليدوية بنجاح' });
                this.onSuccess.emit();
                this.loading = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في حفظ الحركة اليدوية' });
                this.loading = false;
            }
        });
    }

    close() {
        this.onClose.emit();
    }
}
