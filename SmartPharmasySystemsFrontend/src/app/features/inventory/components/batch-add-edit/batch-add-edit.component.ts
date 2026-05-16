import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { MessageService } from 'primeng/api';
import { DropdownModule } from 'primeng/dropdown';
import { MedicineBatchService } from '../../services/medicine-batch.service';
import { PurchaseInvoiceService } from '../../../purchases/services/purchase-invoice.service';
import { SupplierService } from '../../../partners/services/supplier.service';
import { CreateMedicineBatchDto, UpdateMedicineBatchDto, MedicineBatchResponseDto, Supplier } from '../../../../core/models';

@Component({
  selector: 'app-batch-add-edit',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    DialogModule, 
    ButtonModule, 
    InputTextModule, 
    InputNumberModule, 
    CalendarModule,
    DropdownModule
  ],
  templateUrl: './batch-add-edit.component.html',
  styles: [`
    :host { display: block; }
    .required::after { content: " *"; color: red; }
  `]
})
export class BatchAddEditComponent implements OnInit {
  @Input() visible = false;
  @Input() medicineId!: number;
  @Input() medicineName: string = '';
  @Input() batch: MedicineBatchResponseDto | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onSave = new EventEmitter<void>();

  form: FormGroup;
  loading = signal(false);
  suppliers: Supplier[] = [];
  paymentMethods = [
    { label: 'نقداً (Cash)', value: 1 },
    { label: 'آجل (Credit)', value: 2 }
  ];

  constructor(
    private fb: FormBuilder,
    private batchService: MedicineBatchService,
    private purchaseService: PurchaseInvoiceService,
    private supplierService: SupplierService,
    private messageService: MessageService
  ) {
    this.form = this.fb.group({
      companyBatchNumber: ['', Validators.required],
      expiryDate: [null, Validators.required],
      quantity: [null, [Validators.required, Validators.min(1)]],
      bonusQuantity: [0, [Validators.min(0)]],
      unitPurchasePrice: [null, [Validators.required, Validators.min(0)]],
      retailPrice: [null, [Validators.required, Validators.min(0)]],
      batchBarcode: [''],
      storageLocation: [''],
      supplierId: [null],
      paymentMethod: [1, Validators.required],
      notes: ['']
    });
  }

  ngOnInit() {
    this.loadSuppliers();
    if (this.batch) {
      this.form.patchValue({
        companyBatchNumber: this.batch.companyBatchNumber,
        expiryDate: new Date(this.batch.expiryDate),
        quantity: this.batch.quantity,
        unitPurchasePrice: this.batch.unitPurchasePrice,
        retailPrice: this.batch.retailPrice,
        batchBarcode: this.batch.batchBarcode,
        storageLocation: this.batch.storageLocation
      });
      this.form.get('quantity')?.disable();
      this.form.get('bonusQuantity')?.disable();
      this.form.get('supplierId')?.disable();
      this.form.get('paymentMethod')?.disable();
    }
  }

  loadSuppliers() {
    this.supplierService.getAll({ pageSize: 1000 }).subscribe({
      next: (res) => this.suppliers = res.items,
      error: (err) => console.error('Failed to load suppliers', err)
    });
  }

  close() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.form.reset({ paymentMethod: 1, bonusQuantity: 0 });
  }

  save() {
    if (this.form.invalid) {
      Object.keys(this.form.controls).forEach(key => {
        const control = this.form.get(key);
        control?.markAsDirty();
        control?.markAsTouched();
      });
      return;
    }

    this.loading.set(true);
    const val = this.form.getRawValue();

    // Adjust Date to ISO
    const expiryDate = val.expiryDate instanceof Date ? val.expiryDate.toISOString() : val.expiryDate;

    if (this.batch) {
        const payload: UpdateMedicineBatchDto = {
            id: this.batch.id,
            companyBatchNumber: val.companyBatchNumber,
            expiryDate: expiryDate,
            unitPurchasePrice: val.unitPurchasePrice,
            retailPrice: val.retailPrice,
            batchBarcode: val.batchBarcode,
            storageLocation: val.storageLocation
        };

        this.batchService.update(this.batch.id, payload).subscribe({
            next: () => this.handleSuccess('تم التعديل بنجاح'),
            error: (err) => this.handleError(err)
        });
    } else {
        // QUICK PURCHASE LOGIC (Exactly like Purchase Invoice)
        const payload = {
            medicineId: this.medicineId,
            companyBatchNumber: val.companyBatchNumber,
            expiryDate: expiryDate,
            quantity: val.quantity,
            bonusQuantity: val.bonusQuantity || 0,
            purchasePrice: val.unitPurchasePrice,
            salePrice: val.retailPrice,
            batchBarcode: val.batchBarcode,
            supplierId: val.supplierId,
            paymentMethod: val.paymentMethod,
            notes: val.notes
        };

        this.purchaseService.quickPurchase(payload).subscribe({
            next: () => this.handleSuccess('تم التوريد وتحديث المخزون والمالية بنجاح'),
            error: (err) => this.handleError(err)
        });
    }
  }

  private handleSuccess(msg: string) {
    this.loading.set(false);
    this.messageService.add({ severity: 'success', summary: 'نجاح التوريد', detail: msg });
    this.onSave.emit();
    this.close();
  }

  private handleError(err: any) {
    this.loading.set(false);
    console.error(err);
    this.messageService.add({ severity: 'error', summary: 'خطأ في التوريد', detail: err.error?.message || 'حدث خطأ غير متوقع' });
  }
}
