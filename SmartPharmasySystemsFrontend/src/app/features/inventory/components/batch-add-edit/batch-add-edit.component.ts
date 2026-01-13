import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CalendarModule } from 'primeng/calendar';
import { MessageService } from 'primeng/api';
import { MedicineBatchService } from '../../services/medicine-batch.service';
import { CreateMedicineBatchDto, UpdateMedicineBatchDto, MedicineBatchResponseDto } from '../../../../core/models';

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
    CalendarModule
  ],
  templateUrl: './batch-add-edit.component.html',
  styles: [`
    :host { display: block; }
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

  constructor(
    private fb: FormBuilder,
    private batchService: MedicineBatchService,
    private messageService: MessageService
  ) {
    this.form = this.fb.group({
      companyBatchNumber: ['', Validators.required],
      expiryDate: [null, Validators.required],
      quantity: [null, [Validators.required, Validators.min(1)]],
      unitPurchasePrice: [null, [Validators.required, Validators.min(0)]],
      retailPrice: [null, [Validators.required, Validators.min(0)]],
      batchBarcode: [''],
      storageLocation: ['']
    });
  }

  ngOnInit() {
    if (this.batch) {
      this.form.patchValue({
        companyBatchNumber: this.batch.companyBatchNumber,
        expiryDate: new Date(this.batch.expiryDate),
        quantity: this.batch.quantity, // Note: Creating usually sets Init Qty, Editing might be different logic.
        unitPurchasePrice: this.batch.unitPurchasePrice,
        retailPrice: this.batch.retailPrice,
        batchBarcode: this.batch.batchBarcode,
        storageLocation: this.batch.storageLocation
      });
      // Disable quantity editing if it's existing batch (usually handled via adjustments)
      this.form.get('quantity')?.disable();
    }
  }

  close() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.form.reset();
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
    const val = this.form.value;

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
        const payload: CreateMedicineBatchDto = {
            medicineId: this.medicineId,
            companyBatchNumber: val.companyBatchNumber,
            expiryDate: expiryDate,
            quantity: val.quantity,
            unitPurchasePrice: val.unitPurchasePrice,
            retailPrice: val.retailPrice,
            batchBarcode: val.batchBarcode,
            storageLocation: val.storageLocation
        };

        this.batchService.create(payload).subscribe({
            next: () => this.handleSuccess('تم إضافة الدفعة بنجاح'),
            error: (err) => this.handleError(err)
        });
    }
  }

  private handleSuccess(msg: string) {
    this.loading.set(false);
    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: msg });
    this.onSave.emit();
    this.close();
  }

  private handleError(err: any) {
    this.loading.set(false);
    console.error(err);
    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ غير متوقع' });
  }
}
