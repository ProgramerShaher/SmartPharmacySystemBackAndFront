import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DamagedGoodsService, CreateDamagedGoodsRecordDto } from '../../services/damaged-goods.service';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto } from '../../../../core/models';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CardModule } from 'primeng/card';

@Component({
    selector: 'app-create-damaged-goods',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        DropdownModule,
        InputTextModule,
        InputNumberModule,
        CardModule,
        FormsModule
    ],
    templateUrl: './create-damaged-goods.component.html',
    styleUrls: ['./create-damaged-goods.component.scss']
})
export class CreateDamagedGoodsComponent implements OnInit {
    damagedForm: FormGroup;
    warehouses: WarehouseDto[] = [];
    
    // Inventory items available in the selected warehouse
    warehouseInventory: any[] = [];
    availableMedicines: { label: string; value: any; }[] = [];
    
    damageTypes = [
        { label: 'منتهي الصلاحية', value: 'Expired' },
        { label: 'تالف/مكسور', value: 'PhysicalDamage' },
        { label: 'عيب تصنيع', value: 'ManufacturingDefect' }
    ];

    saving = false;

    constructor(
        private fb: FormBuilder,
        private damagedService: DamagedGoodsService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private router: Router
    ) {
        this.damagedForm = this.fb.group({
            sourceWarehouseId: [null, Validators.required],
            selectedStockItem: [null, Validators.required],
            medicineId: [null, Validators.required],
            batchNumber: ['', Validators.required],
            expiryDate: ['', Validators.required],
            quantity: [1, [Validators.required, Validators.min(1)]],
            availableQuantity: [0],
            damageType: ['Expired', Validators.required],
            reason: ['']
        });
    }

    ngOnInit() {
        this.loadWarehouses();

        // Listen for changes on warehouse to load its inventory
        this.damagedForm.get('sourceWarehouseId')?.valueChanges.subscribe(warehouseId => {
            if (warehouseId) {
                this.loadWarehouseInventory(warehouseId);
                this.clearSelectedItem();
            } else {
                this.warehouseInventory = [];
                this.availableMedicines = [];
                this.clearSelectedItem();
            }
        });

        // Listen for item selection changes
        this.damagedForm.get('selectedStockItem')?.valueChanges.subscribe(stockItem => {
            if (stockItem) {
                this.damagedForm.patchValue({
                    medicineId: stockItem.medicineId,
                    batchNumber: stockItem.batchNumber,
                    expiryDate: stockItem.expiryDate,
                    availableQuantity: stockItem.quantity,
                    quantity: 1
                });
                this.damagedForm.get('quantity')?.setValidators([
                    Validators.required,
                    Validators.min(1),
                    Validators.max(stockItem.quantity)
                ]);
                this.damagedForm.get('quantity')?.updateValueAndValidity();
            } else {
                this.clearSelectedItem();
            }
        });
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => {
            this.warehouses = res.filter(w => w.type !== 3); // Exclude Damaged Warehouse from sources
        });
    }

    loadWarehouseInventory(warehouseId: number) {
        this.warehouseService.getInventoryStocks(warehouseId).subscribe(res => {
            this.warehouseInventory = res.filter(s => s.quantity > 0);
            this.availableMedicines = this.warehouseInventory.map(item => ({
                label: `${item.medicineName} (التشغيلة: ${item.batchNumber} - الرصيد: ${item.quantity})`,
                value: item
            }));
        });
    }

    clearSelectedItem() {
        this.damagedForm.patchValue({
            medicineId: null,
            batchNumber: '',
            expiryDate: '',
            availableQuantity: 0,
            quantity: 1
        }, { emitEvent: false });
    }

    saveRecord() {
        if (this.damagedForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال الحقول المطلوبة والتأكد من الكمية المدخلة' });
            return;
        }

        this.saving = true;
        const formValue = this.damagedForm.getRawValue();

        const payload: CreateDamagedGoodsRecordDto = {
            sourceWarehouseId: formValue.sourceWarehouseId,
            medicineId: formValue.medicineId,
            batchNumber: formValue.batchNumber,
            expiryDate: formValue.expiryDate,
            quantity: formValue.quantity,
            damageType: formValue.damageType,
            reason: formValue.reason
        };

        this.damagedService.create(payload).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل البضاعة التالفة بنجاح وبانتظار الاعتماد' });
                this.router.navigate(['/warehouses/damaged']);
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ في الحفظ', detail: err.error?.message || 'فشل الحفظ' });
                this.saving = false;
            }
        });
    }

    backToList() {
        this.router.navigate(['/warehouses/damaged']);
    }
}
