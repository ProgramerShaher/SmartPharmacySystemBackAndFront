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
import { SelectButtonModule } from 'primeng/selectbutton';
import { AuthService } from '../../../auth/services/auth.service';
import { BranchService } from '../../../branches/services/branch.service';

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
        SelectButtonModule,
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

    transferTypeOptions = [
        { label: 'تحويل داخلي (في نفس الفرع)', value: 'internal' },
        { label: 'شحن لفرع آخر (نقل خارجي)', value: 'external' }
    ];

    branches: any[] = [];

    saving = false;

    get currentUserId(): number {
        return this.authService.currentUserValue?.userId ?? 1;
    }

    get currentUserBranchId(): number {
        const user = this.authService.currentUserValue;
        return user?.branchId ? Number(user.branchId) : 0;
    }

    constructor(
        private fb: FormBuilder,
        private transferService: StockTransferService,
        private warehouseService: WarehouseService,
        private branchService: BranchService,
        private messageService: MessageService,
        private router: Router,
        private authService: AuthService
    ) {
        this.transferForm = this.fb.group({
            transferType: ['internal', Validators.required],
            destinationBranchId: [null],
            sourceWarehouseId: [null, Validators.required],
            destinationWarehouseId: [null, Validators.required],
            notes: [''],
            items: this.fb.array([])
        });
    }

    ngOnInit() {
        console.log("Current User Branch ID:", this.currentUserBranchId);
        this.loadBranches();
        this.loadWarehouses();

        // Listen for transferType changes
        this.transferForm.get('transferType')?.valueChanges.subscribe(type => {
            console.log("Transfer type changed to:", type);
            this.transferForm.get('destinationBranchId')?.setValue(null);
            this.transferForm.get('destinationWarehouseId')?.setValue(null);
            this.updateDestinationWarehouses();

            if (type === 'external') {
                this.transferForm.get('destinationBranchId')?.setValidators([Validators.required]);
                this.transferForm.get('destinationWarehouseId')?.clearValidators();
            } else {
                this.transferForm.get('destinationBranchId')?.clearValidators();
                this.transferForm.get('destinationWarehouseId')?.setValidators([Validators.required]);
            }
            this.transferForm.get('destinationBranchId')?.updateValueAndValidity();
            this.transferForm.get('destinationWarehouseId')?.updateValueAndValidity();
        });

        // Listen for destination branch changes (for external transfers)
        this.transferForm.get('destinationBranchId')?.valueChanges.subscribe(branchId => {
            console.log("Destination branch changed to:", branchId);
            this.transferForm.get('destinationWarehouseId')?.setValue(null);
            this.updateDestinationWarehouses();
        });

        // Listen for changes on source warehouse to load its inventory
        this.transferForm.get('sourceWarehouseId')?.valueChanges.subscribe(warehouseId => {
            if (warehouseId) {
                this.loadSourceInventory(warehouseId);
                this.updateDestinationWarehouses();
                // Clear items array as source changed
                this.items.clear();
            } else {
                this.sourceInventory = [];
                this.availableMedicines = [];
                this.items.clear();
            }
        });
    }

    loadBranches() {
        this.branchService.getAll().subscribe(res => {
            console.log("Loaded Branches:", res);
            // Exclude current branch from destination branches list if we know it
            if (this.currentUserBranchId > 0) {
                this.branches = res.filter((b: any) => Number(b.id) !== this.currentUserBranchId);
            } else {
                this.branches = res;
            }
        });
    }

    loadWarehouses() {
        this.warehouseService.getAll().subscribe(res => {
            console.log("Loaded Warehouses:", res);
            this.warehouses = res;

            // Backend serializes Enums as Strings (e.g. "Damaged", "Main", "Branch") or Numbers.
            const isDamaged = (type: any) => type === 'Damaged' || type === 3 || type === '3';

            // Source warehouses are strictly those in the current user's branch (excluding damaged)
            if (this.currentUserBranchId > 0) {
                this.sourceWarehouses = res.filter(w => !isDamaged(w.type) && Number(w.branchId) === this.currentUserBranchId);
            } else {
                // If branchId is unknown, show all non-damaged warehouses
                this.sourceWarehouses = res.filter(w => !isDamaged(w.type));
            }

            console.log("Source Warehouses:", this.sourceWarehouses);
            this.updateDestinationWarehouses();

            if (this.sourceWarehouses.length === 0 && this.currentUserBranchId > 0) {
                this.messageService.add({ severity: 'warn', summary: 'لا توجد مخازن', detail: 'فرعك الحالي لا يمتلك أي مخازن نشطة لإجراء عملية التحويل منها.' });
            }
        });
    }

    updateDestinationWarehouses() {
        const type = this.transferForm.get('transferType')?.value;
        const sourceId = this.transferForm.get('sourceWarehouseId')?.value;
        const sourceWarehouse = this.warehouses.find(w => Number(w.id) === Number(sourceId));

        // Determine the effective source branch
        const effectiveSourceBranchId = sourceWarehouse ? Number(sourceWarehouse.branchId) : this.currentUserBranchId;

        if (type === 'internal') {
            // For internal, destination must be in the same branch, and not the source itself
            if (effectiveSourceBranchId > 0) {
                this.destinationWarehouses = this.warehouses.filter(w =>
                    Number(w.branchId) === effectiveSourceBranchId && Number(w.id) !== Number(sourceId));
            } else {
                this.destinationWarehouses = this.warehouses.filter(w => Number(w.id) !== Number(sourceId));
            }
        } else {
            // For external, destination must be in the selected destination branch
            const destBranchId = this.transferForm.get('destinationBranchId')?.value;
            if (destBranchId) {
                this.destinationWarehouses = this.warehouses.filter(w => Number(w.branchId) === Number(destBranchId));
            } else {
                this.destinationWarehouses = [];
            }
        }
        console.log("Destination Warehouses:", this.destinationWarehouses);
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
            destinationBranchId: formValue.destinationBranchId,
            transferType: formValue.transferType === 'external' ? 5 : 4, // 5 = External, 4 = Internal
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

