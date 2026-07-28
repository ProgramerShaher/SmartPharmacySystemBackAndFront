import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StockTransferService, CreateStockTransferDto } from '../../services/stock-transfer.service';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto } from '../../../../core/models';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
    selector: 'app-create-stock-transfer',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        ButtonModule,
        DropdownModule,
        InputTextModule,
        InputNumberModule,
        TableModule,
        CardModule,
        FormsModule
    ],
    templateUrl: './create-stock-transfer.component.html',
    styleUrls: ['./create-stock-transfer.component.scss']
})
export class CreateStockTransferComponent implements OnInit {
    transferForm: FormGroup;
    warehouses: WarehouseDto[] = [];
    sourceWarehouses: WarehouseDto[] = [];
    destinationWarehouses: WarehouseDto[] = [];

    // Inventory items available in the selected source warehouse
    sourceInventory: any[] = [];
    // Medicines list derived from inventory
    availableMedicines: { label: string; value: any; }[] = [];

    saving = false;

    get currentUserId(): number {
        return this.authService.currentUserValue?.userId ?? 1;
    }

    constructor(
        private fb: FormBuilder,
        private transferService: StockTransferService,
        private warehouseService: WarehouseService,
        private messageService: MessageService,
        private router: Router,
        private authService: AuthService
    ) {
        this.transferForm = this.fb.group({
            sourceWarehouseId: [null, Validators.required],
            destinationWarehouseId: [null, Validators.required],
            notes: [''],
            items: this.fb.array([])
        });
    }

    ngOnInit() {
        this.loadWarehouses();

        // Listen for changes on source warehouse to load its inventory
        this.transferForm.get('sourceWarehouseId')?.valueChanges.subscribe(warehouseId => {
            if (warehouseId) {
                this.loadSourceInventory(warehouseId);
                // Filter destination warehouses
                this.destinationWarehouses = this.warehouses.filter(w => w.id !== warehouseId);
                // Clear items array as source changed
                this.items.clear();
            } else {
                this.sourceInventory = [];
                this.availableMedicines = [];
                this.items.clear();
            }
        });
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => {
            this.warehouses = res;
            this.sourceWarehouses = res.filter(w => w.type !== 3); // Exclude damaged warehouse from sources
            this.destinationWarehouses = res;
        });
    }

    loadSourceInventory(warehouseId: number) {
        this.warehouseService.getInventoryStocks(warehouseId).subscribe(res => {
            this.sourceInventory = res.filter(s => s.quantity > 0);

            // Map to dropdown format — show storage location if available
            this.availableMedicines = this.sourceInventory
                .filter(item => item.quantity > 0)
                .map(item => {
                    const locationPart = item.storageLocation ? ` 📦 ${item.storageLocation}` : '';
                    const expiryPart = item.expiryDate
                        ? ` | ينتهي: ${new Date(item.expiryDate).toLocaleDateString('ar-EG', { year: 'numeric', month: '2-digit' })}`
                        : '';
                    return {
                        label: `${item.medicineName} (تشغيلة: ${item.batchNumber} — متوفر: ${item.quantity}${locationPart}${expiryPart})`,
                        value: item
                    };
                });
        });
    }

    get items() {
        return this.transferForm.get('items') as FormArray;
    }

    addItem() {
        const itemGroup = this.fb.group({
            selectedStockItem: [null, Validators.required],
            medicineId: [null, Validators.required],
            medicineName: [''],
            batchNumber: ['', Validators.required],
            expiryDate: [null],
            storageLocation: [null],
            quantity: [1, [Validators.required, Validators.min(1)]],
            availableQuantity: [0]
        });

        // Listen for item selection changes to populate details
        itemGroup.get('selectedStockItem')?.valueChanges.subscribe((stockItem: any) => {
            if (stockItem) {
                itemGroup.patchValue({
                    medicineId: stockItem.medicineId,
                    medicineName: stockItem.medicineName,
                    batchNumber: stockItem.batchNumber,
                    expiryDate: stockItem.expiryDate,
                    storageLocation: stockItem.storageLocation ?? null,
                    availableQuantity: stockItem.quantity,
                    quantity: 1
                });
                itemGroup.get('quantity')?.setValidators([
                    Validators.required,
                    Validators.min(1),
                    Validators.max(stockItem.quantity)
                ]);
                itemGroup.get('quantity')?.updateValueAndValidity();
            }
        });

        this.items.push(itemGroup);
    }

    removeItem(index: number) {
        this.items.removeAt(index);
    }

    saveTransfer() {
        if (this.transferForm.invalid) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى إكمال جميع الحقول المطلوبة ومراجعة كميات التحويل' });
            return;
        }

        if (this.items.length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يجب إضافة صنف واحد على الأقل للتحويل' });
            return;
        }

        this.saving = true;
        const formValue = this.transferForm.getRawValue();

        const payload: any = {
            sourceWarehouseId: formValue.sourceWarehouseId,
            destinationWarehouseId: formValue.destinationWarehouseId,
            notes: formValue.notes,
            items: formValue.items.map((item: any) => ({
                medicineId: item.medicineId,
                batchNumber: item.batchNumber,
                expiryDate: item.expiryDate,
                quantityRequested: item.quantity
            }))
        };

        this.transferService.create(payload, this.currentUserId).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إرسال طلب التحويل المخزني بنجاح' });
                this.router.navigate(['/warehouses/transfers']);
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ في الحفظ', detail: err.error?.message || 'فشل إرسال الطلب' });
                this.saving = false;
            }
        });
    }

    backToList() {
        this.router.navigate(['/warehouses/transfers']);
    }
}

