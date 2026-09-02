import { Component, EventEmitter, Input, OnInit, Output, OnChanges, SimpleChanges, signal, HostListener, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { SidebarModule } from 'primeng/sidebar';
import { MessageService } from 'primeng/api';
import { MedicineService } from '../../services/medicine.service';
import { CategoryService } from '../../services/category.service';
import { MedicineDto, CreateMedicineDto, UpdateMedicineDto, CategoryDto } from '../../../../core/models';
import { TagModule } from "primeng/tag";
import { DialogModule } from "primeng/dialog";
import { UploadService } from '../../../../core/services/upload.service';

@Component({
    selector: 'app-medicine-add-edit',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        SidebarModule,
        ButtonModule,
        InputTextModule,
        DropdownModule,
        InputNumberModule,
        CheckboxModule,
        InputTextareaModule,
        TagModule,
        DialogModule
    ],
    templateUrl: './medicine-add-edit.component.html',
    styleUrls: ['./medicine-add-edit.component.scss']
})
export class MedicineAddEditComponent implements OnInit, OnChanges {
    @Input() visible = false;
    @Input() medicine: MedicineDto | null = null;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Output() onSave = new EventEmitter<void>();

    medicineForm: FormGroup;
    newUnitForm: FormGroup;
    loading = signal(false);
    uploading = signal(false);
    unitModalVisible = false;
    categories = signal<CategoryDto[]>([]);

    unitOptions = [
        'حبة',
        'شريط',
        'علبة',
        'باكت',
        'كرتون',
        'امبولة',
        'فيال',
        'قطرة',
        'مرهم',
        'كريم',
        'شراب',
        'كيس',
        'مضرب'
    ];

    statusOptions = [
        { label: 'نشط', value: 'Active' },
        { label: 'غير نشط', value: 'Inactive' }
    ];

    constructor(
        private fb: FormBuilder,
        private medicineService: MedicineService,
        private categoryService: CategoryService,
        private messageService: MessageService,
        private uploadService: UploadService
    ) {
        this.medicineForm = this.fb.group({
            internalCode: [''], // Hidden in UI, auto-generated
            name: ['', Validators.required],
            scientificName: [''], // Optional
            activeIngredient: [''],
            categoryId: [null, Validators.required], // Required now
            manufacturer: ['غير محدد'], // Hidden in UI
            defaultBarcode: [''],
            defaultPurchasePrice: [0, [Validators.required, Validators.min(0)]],
            defaultSalePrice: [0, [Validators.required, Validators.min(0)]],
            minAlertQuantity: [5, Validators.min(0)],
            reorderLevel: [10, Validators.min(0)],
            soldByUnit: [true],
            status: ['Active'],
            imageUrl: [''],
            notes: [''],
            baseUnitName: ['حبة', Validators.required],
            medicineUnits: this.fb.array([])
        }, { validators: this.priceValidator });

        this.newUnitForm = this.fb.group({
            id: [0],
            name: ['', Validators.required],
            conversionFactor: [1, [Validators.required, Validators.min(1)]],
            defaultPurchasePrice: [0, [Validators.required, Validators.min(0)]],
            defaultSalePrice: [0, [Validators.required, Validators.min(0)]],
            barcode: ['']
        });
    }

    get medicineUnits(): FormArray {
        return this.medicineForm.get('medicineUnits') as FormArray;
    }

    openUnitModal() {
        this.newUnitForm.reset({
            id: 0,
            conversionFactor: 1,
            defaultPurchasePrice: 0,
            defaultSalePrice: 0
        });
        this.unitModalVisible = true;
        setTimeout(() => {
            const el = document.getElementById('unitNameInput');
            if (el) el.focus();
        }, 100);
    }

    saveUnit() {
        if (this.newUnitForm.invalid) return;
        this.medicineUnits.push(this.fb.group(this.newUnitForm.value));
        this.unitModalVisible = false;
    }

    focusNextUnit(nextId: string, event?: Event) {
        if (event) event.preventDefault();
        setTimeout(() => {
            const el = document.getElementById(nextId);
            if (el) {
                const input = el.querySelector('input') || el;
                (input as HTMLElement).focus();
                if (typeof (input as HTMLInputElement).select === 'function') {
                    (input as HTMLInputElement).select();
                }
            } else {
                this.saveUnit();
            }
        }, 50);
    }

    removeUnit(index: number) {
        this.medicineUnits.removeAt(index);
    }

    onFileSelected(event: any) {
        const file: File = event.target.files[0];
        if (!file) return;

        if (file.size > 2 * 1024 * 1024) {
            this.messageService.add({
                severity: 'warn',
                summary: 'تنبيه',
                detail: 'حجم الصورة كبير جداً، يرجى اختيار صورة أقل من 2 ميجابايت'
            });
            return;
        }

        this.uploading.set(true);
        this.uploadService.uploadMedicineImage(
            file,
            this.medicineForm.get('categoryId')?.value || 'other',
            'general',
            this.medicineForm.get('name')?.value || 'medicine'
        ).subscribe({
            next: (res: any) => {
                this.medicineForm.patchValue({ imageUrl: res.imageUrl });
                this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ مسار الصورة بنجاح' });
                this.uploading.set(false);
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الرفع، تأكد من تشغيل السيرفر' });
                this.uploading.set(false);
            }
        });
    }

    @HostListener('window:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        if (!this.visible) return;

        if (event.key === 'F2') {
            event.preventDefault();
            this.save();
        }

        if (event.key === 'F3') {
            event.preventDefault();
            this.openUnitModal();
        }

        // Esc is usually handled by p-dialog, but added for safety
        if (event.key === 'Escape') {
            if (this.unitModalVisible) {
                this.unitModalVisible = false;
            } else {
                this.close();
            }
        }
    }

    focusNext(nextElementId: string, event?: Event) {
        if (event) {
            event.preventDefault();
        }

        setTimeout(() => {
            const el = document.getElementById(nextElementId);
            if (el) {
                // Focus the native input if it's a PrimeNG component wrapper
                const input = el.querySelector('input') || el;
                (input as HTMLElement).focus();

                // Trigger select if it's a numeric input
                if (typeof (input as HTMLInputElement).select === 'function') {
                    (input as HTMLInputElement).select();
                }
            } else {
                // If the element doesn't exist (e.g. Save button), just focus the save button by id
                const saveBtn = document.getElementById('saveMedicineBtn');
                if (saveBtn) saveBtn.focus();
            }
        }, 50);
    }

    ngOnInit() {
        this.loadCategories();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['visible'] && this.visible) {
            this.medicineUnits.clear();
            if (this.medicine) {
                this.medicineForm.patchValue(this.medicine);
            } else {
                this.medicineForm.reset({
                    internalCode: this.generateCode(),
                    categoryId: this.categories().length > 0 ? this.categories()[0].id : null,
                    manufacturer: 'غير محدد',
                    defaultPurchasePrice: 0,
                    defaultSalePrice: 0,
                    minAlertQuantity: 5,
                    reorderLevel: 10,
                    soldByUnit: true,
                    status: 'Active',
                    baseUnitName: 'حبة'
                });
            }

            // Auto focus on Name after modal animation finishes
            setTimeout(() => {
                const nameInput = document.getElementById('medNameInput');
                if (nameInput) {
                    nameInput.focus();
                }
            }, 300);
        }
    }

    generateCode(): string {
        return 'MED-' + Math.floor(100000 + Math.random() * 900000).toString();
    }

    selectText(event: any) {
        const target = event?.target || event?.originalEvent?.target;
        if (target && typeof target.select === 'function') {
            target.select();
        }
    }

    loadCategories() {
        this.categoryService.getAllForDropdown().subscribe({
            next: (data) => {
                this.categories.set(data);
                if (!this.medicine && data.length > 0 && !this.medicineForm.get('categoryId')?.value) {
                    this.medicineForm.patchValue({ categoryId: data[0].id });
                }
            },
            error: () => console.error('Error loading categories')
        });
    }

    priceValidator(group: FormGroup) {
        const createPrice = group.get('defaultPurchasePrice')?.value;
        const salePrice = group.get('defaultSalePrice')?.value;
        return createPrice !== null && salePrice !== null && salePrice < createPrice
            ? { invalidPrice: true } : null;
    }

    private extractErrorMessage(err: any): string {
        if (typeof err.error === 'string') return err.error;
        if (err.error?.message) return err.error.message;
        if (err.error?.title) {
            let details = err.error.title;
            if (err.error.errors) {
                const msgs = Object.values(err.error.errors).flat();
                if (msgs.length > 0) details += ': ' + msgs.join(', ');
            }
            return details;
        }
        return err.message || 'حدث خطأ غير معروف';
    }

    close() {
        this.visible = false;
        this.unitModalVisible = false;
        this.visibleChange.emit(false);
    }

    save() {
        if (this.medicineForm.invalid) {
            this.medicineForm.markAllAsTouched();
            this.messageService.add({ severity: 'error', summary: 'تنبيه', detail: 'يرجى إكمال الحقول الأساسية المطلوبة' });
            return;
        }

        const formValue = this.medicineForm.value;

        // If category is string (user typed it in editable dropdown), try to match it to an existing ID
        if (typeof formValue.categoryId === 'string') {
            const found = this.categories().find(c => c.name === formValue.categoryId);
            if (found) {
                formValue.categoryId = found.id;
            } else {
                this.messageService.add({ severity: 'error', summary: 'تنبيه', detail: 'التصنيف المدخل غير موجود، يرجى اختيار تصنيف صالح' });
                return;
            }
        }

        this.loading.set(true);

        if (this.medicine) {
            const updateDto: UpdateMedicineDto = {
                id: this.medicine.id,
                ...formValue
            };
            this.medicineService.update(this.medicine.id, updateDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الدواء' });
                    this.onSave.emit();
                    this.close();
                    this.loading.set(false);
                },
                error: (err) => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: this.extractErrorMessage(err) });
                    this.loading.set(false);
                }
            });
        } else {
            const createDto: CreateMedicineDto = formValue;
            this.medicineService.create(createDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إضافة الدواء بنجاح' });
                    this.onSave.emit();
                    this.close();
                    this.loading.set(false);
                },
                error: (err) => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: this.extractErrorMessage(err) });
                    this.loading.set(false);
                }
            });
        }
    }
}

