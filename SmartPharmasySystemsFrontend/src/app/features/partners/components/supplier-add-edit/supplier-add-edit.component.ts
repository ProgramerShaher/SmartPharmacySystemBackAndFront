import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { RippleModule } from 'primeng/ripple';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.models';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-supplier-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    RippleModule,
    ToastModule,
    ProgressSpinnerModule
  ],
  templateUrl: './supplier-add-edit.component.html',
  styleUrl: './supplier-add-edit.component.scss',
  providers: [MessageService]
})
export class SupplierAddEditComponent implements OnInit, OnChanges {
  @Input() dialogMode = false;
  @Input() editSupplierId?: number | null;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  supplierForm: FormGroup;
  isEditMode = false;
  supplierId?: number;
  loading = false;
  savingState = false;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.supplierForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      contactPerson: ['', [Validators.required, Validators.maxLength(50)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^[\d\s\+\-\(\)]{7,15}$/)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.maxLength(200)]],
      notes: ['', [Validators.maxLength(500)]],
      balance: [0, [Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    if (this.dialogMode) {
      this.initDialogMode();
      return;
    }

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.supplierId = +params['id'];
        this.loadSupplier(this.supplierId);
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.dialogMode || !changes['editSupplierId']) return;
    this.initDialogMode();
  }

  private initDialogMode(): void {
    this.supplierForm.reset({
      name: '',
      contactPerson: '',
      phoneNumber: '',
      email: '',
      address: '',
      notes: '',
      balance: 0
    });

    this.isEditMode = !!this.editSupplierId;
    this.supplierId = this.editSupplierId || undefined;

    if (this.supplierId) {
      this.loadSupplier(this.supplierId);
    }
  }

  get saving(): boolean {
    return this.savingState;
  }

  loadSupplier(id: number): void {
    this.loading = true;
    this.supplierService.getById(id)
      .pipe(finalize(() => { this.loading = false; }))
      .subscribe({
        next: (supplier: Supplier) => {
          this.supplierForm.patchValue({
            name: supplier.name,
            contactPerson: supplier.contactPerson || '',
            phoneNumber: supplier.phoneNumber,
            email: supplier.email || '',
            address: supplier.address,
            notes: supplier.notes || '',
            balance: supplier.Balance || 0
          });
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات المورد' });
          if (!this.dialogMode) {
            this.router.navigate(['/partners/suppliers']);
          }
        }
      });
  }

  // supplier-add-edit.component.ts

 onSubmit(): void {
  if (this.supplierForm.invalid) {
    this.markFormGroupTouched(this.supplierForm);
    this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تعبئة الحقول المطلوبة بشكل صحيح' });
    return;
  }

  this.savingState = true;
  const formValue = this.supplierForm.value;
  
  // بناء الـ Payload مع إضافة الـ id في حالة التعديل
  const payload: any = {
    ...formValue,
    contactPerson: formValue.contactPerson || '',
    email: formValue.email || '',
    notes: formValue.notes || '',
    Balance: formValue.balance || 0
  };

  // إضافة الـ id هنا ليرسل مع البيانات في الـ Body
  if (this.isEditMode && this.supplierId) {
    payload.id = this.supplierId; 
  }

  const request = this.isEditMode && this.supplierId
    ? this.supplierService.update(this.supplierId, payload)
    : this.supplierService.create(payload);

  request.pipe(finalize(() => { this.savingState = false; }))
    .subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: this.isEditMode ? 'تم تحديث المورد' : 'تم إضافة المورد' });
        this.saved.emit();
        if (this.dialogMode) {
          this.closed.emit();
        } else {
          this.router.navigate(['/partners/suppliers']);
        }
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: this.isEditMode ? 'فشل تحديث المورد' : 'فشل إضافة المورد' });
      }
    });
}

  onCancel(): void {
    if (this.supplierForm.dirty && !confirm('هل أنت متأكد من الخروج؟ سيتم فقدان التغييرات غير المحفوظة.')) return;

    if (this.dialogMode) {
      this.closed.emit();
    } else {
      this.router.navigate(['/partners/suppliers']);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) this.markFormGroupTouched(control);
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.supplierForm.get(fieldName);
    if (!field?.touched || !field?.errors) return '';

    const errors = field.errors;
    if (errors['required']) return 'هذا الحقل مطلوب';
    if (errors['minlength']) return `يجب ألا يقل عن ${errors['minlength'].requiredLength} أحرف`;
    if (errors['maxlength']) return `الحد الأقصى ${errors['maxlength'].requiredLength} حرف`; 
    if (errors['email']) return 'البريد الإلكتروني غير صالح';
    if (errors['pattern']) return fieldName === 'phoneNumber' ? 'رقم الهاتف غير صالح' : 'صيغة غير صالحة';
    if (errors['min']) return 'القيمة يجب أن تكون 0 أو أكبر';
    return 'قيمة غير صالحة';
  }
}
