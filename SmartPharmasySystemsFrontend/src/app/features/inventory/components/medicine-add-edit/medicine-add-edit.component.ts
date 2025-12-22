import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../services/inventory.service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { DropdownModule } from 'primeng/dropdown';

@Component({
    selector: 'app-medicine-add-edit',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, InputTextareaModule, DropdownModule],
    templateUrl: './medicine-add-edit.component.html'
})
export class MedicineAddEditComponent implements OnInit, OnChanges {
    @Input() medicineId: number | null = null;
    @Output() onSave = new EventEmitter<void>();
    @Output() onCancel = new EventEmitter<void>();

    medicineForm: FormGroup;
    editMode = false;
    private originalMedicine: any = null;

    constructor(
        private fb: FormBuilder,
        private inventoryService: InventoryService,
        private messageService: MessageService
    ) {
        this.medicineForm = this.fb.group({
            name: ['', Validators.required],
            internalCode: [''],
            defaultBarcode: [''],
            categoryId: [null, Validators.required],
            minAlertQuantity: [0],
            defaultPurchasePrice: [0],
            defaultSalePrice: [0],
            notes: ['']
        });
    }

    categories: any[] = [];

    ngOnInit() {
        this.loadCategories();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['medicineId']) {
            if (this.medicineId) {
                this.editMode = true;
                this.loadMedicine(this.medicineId);
            } else {
                this.editMode = false;
                this.medicineForm.reset({
                    minAlertQuantity: 0,
                    defaultPurchasePrice: 0,
                    defaultSalePrice: 0
                });
            }
        }
    }

    loadCategories() {
        this.inventoryService.getAllCategories().subscribe({
            next: (data) => {
                console.log('✅ Categories loaded:', data);
                // The service now returns PagedResult<Category>, so we need data.items
                this.categories = data.items || [];
                if (!Array.isArray(this.categories)) console.error('⚠️ Categories items is NOT an array:', data);
            },
            error: (e) => {
                console.error('Error loading categories', e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة الفئات' });
            }
        });
    }

    loadMedicine(id: number) {
        this.inventoryService.getMedicineById(id).subscribe({
            next: (data) => {
                this.originalMedicine = data;
                this.medicineForm.patchValue(data);
            },
            error: (e) => {
                console.error('Load Medicine Error:', e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات الدواء' });
            }
        });
    }

    saveMedicine() {
        if (this.medicineForm.invalid) return;

        const formValue = this.medicineForm.value;

        if (this.editMode && this.medicineId) {
            // Merge original data with form values to preserve non-form fields (like manufacturer, status, etc.)
            // and ensure ID is included in the body
            const payload = {
                ...this.originalMedicine,
                ...formValue,
                id: this.medicineId
            };

            // Remove navigation properties to avoid backend serialization/cycle issues
            delete payload.category;
            delete payload.medicineBatches;
            delete payload.inventoryMovements;

            console.log('Updating medicine with payload:', payload);

            this.inventoryService.updateMedicine(this.medicineId, payload).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم التحديث', detail: 'تم تحديث بيانات الدواء بنجاح' });
                    this.onSave.emit();
                },
                error: (e) => {
                    console.error('Update Medicine API Error:', e);
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء تحديث الدواء' });
                }
            });
        } else {
            console.log('Creating medicine with payload:', formValue);
            this.inventoryService.createMedicine(formValue).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'تم الإضافة', detail: 'تم إضافة الدواء الجديد بنجاح' });
                    this.onSave.emit();
                },
                error: (e) => {
                    console.error('Create Medicine API Error:', e);
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء إضافة الدواء' });
                }
            });
        }
    }

    cancel() {
        this.onCancel.emit();
    }

    getCategoryName(id: number): string {
        const cat = this.categories.find(c => c.id === id);
        return cat ? cat.name : '';
    }
}
