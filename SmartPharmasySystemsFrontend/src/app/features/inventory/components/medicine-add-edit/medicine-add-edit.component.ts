import { Component, EventEmitter, Input, OnInit, Output, OnChanges, SimpleChanges, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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
    loading = signal(false);
    uploading = signal(false);
    categories = signal<CategoryDto[]>([]);

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
            internalCode: [''],
            name: ['', Validators.required],
            scientificName: [''],
            activeIngredient: [''],
            categoryId: [null],
            manufacturer: [''],
            defaultBarcode: [''],
            defaultPurchasePrice: [0, [Validators.required, Validators.min(0)]],
            defaultSalePrice: [0, [Validators.required, Validators.min(0)]],
            minAlertQuantity: [5, Validators.min(0)],
            reorderLevel: [10, Validators.min(0)],
            soldByUnit: [true],
            status: ['Active'],
            imageUrl: [''],
            notes: ['']
        }, { validators: this.priceValidator });
    }

    onFileSelected(event: any) {
        const file: File = event.target.files[0];
        if (!file) return;

        // التحقق من الحجم (الحد الأقصى 2 ميجابايت) لمنع تعليق الجهاز
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
                // نحن نخزن المسار فقط كما طلبت
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

    ngOnInit() {
        this.loadCategories();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['visible'] && this.visible) {
            if (this.medicine) {
                this.medicineForm.patchValue(this.medicine);
            } else {
                this.medicineForm.reset({
                    defaultPurchasePrice: 0,
                    defaultSalePrice: 0,
                    minAlertQuantity: 5,
                    reorderLevel: 10,
                    soldByUnit: true,
                    status: 'Active'
                });
            }
        }
    }

    loadCategories() {
        this.categoryService.getAllForDropdown().subscribe({
            next: (data) => this.categories.set(data),
            error: () => console.error('Error loading categories')
        });
    }

    priceValidator(group: FormGroup) {
        const createPrice = group.get('defaultPurchasePrice')?.value;
        const salePrice = group.get('defaultSalePrice')?.value;
        return createPrice !== null && salePrice !== null && salePrice < createPrice
            ? { invalidPrice: true } : null;
    }

    close() {
        this.visible = false;
        this.visibleChange.emit(false);
    }

    save() {
        if (this.medicineForm.invalid) {
            this.medicineForm.markAllAsTouched();
            return;
        }

        this.loading.set(true);
        const formValue = this.medicineForm.value;

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
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل التحديث' });
                    this.loading.set(false);
                }
            });
        } else {
            const createDto: CreateMedicineDto = formValue;
            this.medicineService.create(createDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إضافة الدواء' });
                    this.onSave.emit();
                    this.close();
                    this.loading.set(false);
                },
                error: (err) => {
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الإضافة' });
                    this.loading.set(false);
                }
            });
        }
    }
}
