import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { InventoryMovement, Medicine, MedicineBatch } from '../../../../core/models';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CalendarModule } from 'primeng/calendar';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-movement-form',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        DropdownModule,
        InputNumberModule,
        InputTextModule,
        InputTextareaModule,
        CalendarModule,
        ButtonModule
    ],
    templateUrl: './movement-form.component.html',
    styleUrl: './movement-form.component.scss'
})
export class MovementFormComponent implements OnInit {
    @Input() movement: InventoryMovement | null = null;
    @Output() save = new EventEmitter<void>();
    @Output() cancel = new EventEmitter<void>();

    movementForm!: FormGroup;
    medicines: Medicine[] = [];
    batches: MedicineBatch[] = [];
    loading = false;
    loadingBatches = false;
    today = new Date();

    movementTypes = [
        { label: 'دخول (توريد)', value: 'IN' },
        { label: 'خروج (صرف)', value: 'OUT' },
        { label: 'مرتجع', value: 'RETURN' },
        { label: 'تالف', value: 'DAMAGE' },
        { label: 'تعديل', value: 'ADJUSTMENT' }
    ];

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.initForm();
        this.loadMedicines();

        if (this.movement) {
            this.movementForm.patchValue({
                ...this.movement,
                date: new Date(this.movement.date)
            });
            this.onMedicineChange(this.movement.medicineId);
        }
    }

    private initForm() {
        this.movementForm = this.fb.group({
            medicineId: [null, Validators.required],
            batchId: [null],
            movementType: ['IN', Validators.required],
            quantity: [null, [Validators.required, Validators.min(1)]],
            date: [new Date(), Validators.required],
            referenceId: [''],
            notes: ['']
        });
    }

    loadMedicines() {
        this.inventoryService.searchMedicines({}).subscribe(data => {
            this.medicines = data.items || [];
        });
    }

    onMedicineChange(medicineId: number) {
        if (!medicineId) {
            this.batches = [];
            this.movementForm.patchValue({ batchId: null });
            return;
        }

        this.loadingBatches = true;
        this.inventoryService.getBatchesByMedicineId(medicineId).subscribe({
            next: (data) => {
                this.batches = data;
                this.loadingBatches = false;
            },
            error: () => {
                this.loadingBatches = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل التشغيلات لهذا الدواء' });
            }
        });
    }

    onSave() {
        if (this.movementForm.invalid) {
            this.movementForm.markAllAsTouched();
            return;
        }

        const formValue = this.movementForm.value;

        // Validation: Prevent OUT movements if stock is insufficient
        if (formValue.movementType === 'OUT' || formValue.movementType === 'DAMAGE') {
            const selectedBatch = this.batches.find(b => b.id === formValue.batchId);
            const availableQty = selectedBatch ? selectedBatch.quantity : 0;

            if (formValue.quantity > availableQty) {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ في الكمية',
                    detail: `الكمية المطلوبة (${formValue.quantity}) أكبر من الكمية المتوفرة في المخزن (${availableQty})`
                });
                return;
            }
        }

        this.loading = true;
        const movementData: Partial<InventoryMovement> = {
            ...formValue,
            date: formValue.date
        };

        if (this.movement) {
            this.inventoryService.updateMovement(this.movement.id, movementData).subscribe({
                next: () => {
                    this.loading = false;
                    this.save.emit();
                },
                error: () => this.loading = false
            });
        } else {
            this.inventoryService.createMovement(movementData).subscribe({
                next: () => {
                    this.loading = false;
                    this.save.emit();
                },
                error: () => this.loading = false
            });
        }
    }

    onCancel() {
        this.cancel.emit();
    }
}
