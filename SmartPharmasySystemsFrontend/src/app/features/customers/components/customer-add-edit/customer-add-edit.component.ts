import { Component, OnInit, signal, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import { MessageService } from 'primeng/api';

// PrimeNG
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputSwitchModule } from 'primeng/inputswitch';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from 'primeng/dropdown';
import { PricelistService, PricelistSelectDto } from '../../../inventory/services/pricelist.service';

@Component({
  selector: 'app-customer-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    InputTextareaModule,
    InputSwitchModule,
    ToastModule,
    ProgressSpinnerModule,
    DropdownModule
  ],
  providers: [MessageService],
  templateUrl: './customer-add-edit.component.html',
  styleUrls: ['./customer-add-edit.component.scss']
})
export class CustomerAddEditComponent implements OnInit, OnChanges {
  @Input() customerId: number | null = null;
  @Output() onSave = new EventEmitter<void>();
  @Output() onClose = new EventEmitter<void>();

  customerForm: FormGroup;
  isEditMode = signal(false);
  loading = signal(false);
  saving = signal(false);
  pricelists = signal<PricelistSelectDto[]>([]);

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private pricelistService: PricelistService,
    private messageService: MessageService
  ) {
    this.customerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      phoneNumber: ['', [Validators.required]],
      email: ['', [Validators.email]],
      address: [''],
      creditLimit: [0],
      notes: [''],
      pricelistId: [null],
      isActive: [true]
    });
  }

  ngOnInit() {
    this.initForm();
    this.loadPricelists();
  }

  loadPricelists() {
    this.pricelistService.getSelectList().subscribe({
      next: (data) => this.pricelists.set(data),
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قوائم الأسعار' })
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['customerId']) {
      this.initForm();
    }
  }

  initForm() {
    if (this.customerId) {
      this.isEditMode.set(true);
      this.loadCustomer();
    } else {
      this.isEditMode.set(false);
      this.customerForm.reset({
        isActive: true,
        creditLimit: 0
      });
    }
  }

  loadCustomer() {
    if (!this.customerId) return;
    this.loading.set(true);
    this.customerService.getById(this.customerId).subscribe({
      next: (customer) => {
        this.customerForm.patchValue(customer);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات العميل' });
        this.loading.set(false);
      }
    });
  }

  onSubmit() {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const customerData = this.customerForm.value;

    if (this.isEditMode()) {
      const updateData = { ...customerData, id: this.customerId };
      this.customerService.update(this.customerId!, updateData).subscribe({
        next: () => {
          this.handleSuccess('تم تحديث بيانات العميل بنجاح');
          this.onSave.emit();
        },
        error: () => this.handleError('فشل في تحديث بيانات العميل')
      });
    } else {
      this.customerService.create(customerData).subscribe({
        next: () => {
          this.handleSuccess('تم إضافة العميل بنجاح');
          this.onSave.emit();
        },
        error: () => this.handleError('فشل في إضافة العميل')
      });
    }
  }

  private handleSuccess(message: string) {
    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: message });
    // Keep saving true briefly or just reset?
    this.saving.set(false);
  }

  private handleError(message: string) {
    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: message });
    this.saving.set(false);
  }

  cancel() {
    this.onClose.emit();
  }
}
