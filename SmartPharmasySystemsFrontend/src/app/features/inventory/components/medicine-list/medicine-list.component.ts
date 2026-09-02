import { Component, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService, LazyLoadEvent } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from "primeng/toast";
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { MedicineService } from '../../services/medicine.service';
import { Medicine, MedicineQueryDto } from '../../../../core/models/medicine.interface';
import { MedicineAddEditComponent } from '../medicine-add-edit/medicine-add-edit.component';
import { BatchAddEditComponent } from '../batch-add-edit/batch-add-edit.component';
import { MedicineDetailsModalComponent } from '../medicine-details-modal/medicine-details-modal.component';

@Component({
    selector: 'app-medicine-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        DropdownModule,
        TagModule,
        ToolbarModule,
        TooltipModule,
        ConfirmDialogModule,
        DialogModule,
        ToastModule,
        MedicineAddEditComponent,
        BatchAddEditComponent,
        MedicineDetailsModalComponent
    ],
    templateUrl: './medicine-list.component.html',
    styleUrls: ['./medicine-list.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class MedicineListComponent implements OnInit {
    // Signals & State
    medicines = signal<Medicine[]>([]);
    totalRecords = signal(0);
    loading = signal(true);
    isImporting = signal(false);

    // Filters
    searchTerm = signal('');
    categoryId = signal<number | undefined>(undefined);
    status = signal<string | undefined>(undefined);

    // UI State
    showMedicineDialog = signal(false);
    showBatchDialog = signal(false);
    showDetailsDialog = signal(false);

    selectedMedicine: Medicine | null = null;
    selectedMedicines: Medicine[] = [];
    selectedMedicineId = signal<number | null>(null);
    selectedMedicineIdForBatch = 0;
    selectedMedicineNameForBatch = '';

    @ViewChild('dt') dt!: Table;

    statusOptions = [
        { label: 'الكل', value: null },
        { label: 'نشط', value: 'Active' },
        { label: 'غير نشط', value: 'Inactive' }
    ];

    private searchSubject = new Subject<string>();
    private lastLazyEvent: any = { first: 0, rows: 10 };

    constructor(
        private medicineService: MedicineService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private router: Router
    ) { }

    ngOnInit() {
        // Setup Search Debounce
        this.searchSubject.pipe(
            debounceTime(500),
            distinctUntilChanged()
        ).subscribe(term => {
            this.searchTerm.set(term);
            if (this.dt) {
                this.dt.reset(); // This will trigger onLazyLoad with first: 0
            } else {
                this.lastLazyEvent.first = 0;
                this.loadMedicines(this.lastLazyEvent);
            }
        });
    }

    onSearch(value: string) {
        this.searchSubject.next(value);
    }

    loadMedicines(event: any) {
        this.loading.set(true);
        this.lastLazyEvent = event;

        const page = (event.first / event.rows) + 1;
        const pageSize = event.rows;
        const sortBy = event.sortField;
        const sortDescending = event.sortOrder === -1;

        const query: MedicineQueryDto = {
            page,
            pageSize,
            search: this.searchTerm(),
            categoryId: this.categoryId(),
            status: this.status(),
            sortBy,
            sortDescending
        };

        this.medicineService.getAll(query).subscribe({
            next: (result) => {
                this.medicines.set(result.items);
                this.totalRecords.set(result.totalCount);
                this.loading.set(false);
            },
            error: (err) => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الأدوية' });
            }
        });
    }

    // --- Medicines Actions ---

    openAddMedicine() {
        this.selectedMedicine = null;
        this.showMedicineDialog.set(true);
    }

    editMedicine(medicine: Medicine) {
        this.selectedMedicine = medicine;
        this.showMedicineDialog.set(true);
    }

    onMedicineSaved() {
        this.loadMedicines(this.lastLazyEvent);
    }

    deleteMedicine(medicine: Medicine) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف ${medicine.name}؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.medicineService.delete(medicine.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الدواء بنجاح' });
                        this.loadMedicines(this.lastLazyEvent);
                    },
                    error: (err) => {
                        // عرض رسالة الخطأ القادمة من الباك اند
                        const errorMsg = err.error?.message || 'لا يمكن حذف الدواء لارتباطه بسجلات أخرى أو وجود مخزون';
                        this.messageService.add({ severity: 'error', summary: 'خطأ في الحذف', detail: errorMsg });
                    }
                });
            }
        });
    }

    deleteSelectedMedicines() {
        if (!this.selectedMedicines || this.selectedMedicines.length === 0) return;

        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف ${this.selectedMedicines.length} دواء؟`,
            header: 'تأكيد الحذف الجماعي',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                const ids = this.selectedMedicines.map(m => m.id);
                this.medicineService.deleteBulk(ids).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الأدوية بنجاح' });
                        this.selectedMedicines = [];
                        this.loadMedicines(this.lastLazyEvent);
                    },
                    error: (err) => {
                        console.error('Delete Bulk Medicines Error:', err);
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء الحذف' });
                    }
                });
            }
        });
    }

    // --- Batch Actions (Inventory Setup) ---

    openAddBatch(medicine: Medicine) {
        this.router.navigate(['/purchases/create'], {
            queryParams: { medicineId: medicine.id, medicineName: medicine.name }
        });
    }

    onBatchSaved() {
        this.loadMedicines(this.lastLazyEvent);
    }

    quickSale(medicine: Medicine) {
        this.router.navigate(['/sales/create'], {
            queryParams: { quickSaleMedicineId: medicine.id }
        });
    }

    // --- View Details ---
    viewDetails(medicine: any) {
        this.selectedMedicineId.set(medicine.id);
        this.showDetailsDialog.set(true);
    }

    getStatusSeverity(status: string): 'success' | 'danger' | 'warning' | 'info' {
        return status === 'Active' ? 'success' : 'danger';
    }

    // --- Excel Import/Export ---
    downloadTemplate() {
        this.medicineService.downloadTemplate();
    }

    onFileSelected(event: any) {
        const file: File = event.target.files[0];
        if (file) {
            this.isImporting.set(true);
            this.medicineService.importMedicines(file).subscribe({
                next: (res: any) => {
                    this.isImporting.set(false);
                    // Clear the file input
                    event.target.value = '';
                    
                    let summary = `تم استيراد ${res.successCount} دواء بنجاح.`;
                    if (res.updatedCount > 0) summary += ` تم تحديث ${res.updatedCount} دواء.`;
                    if (res.failedCount > 0) summary += ` فشل استيراد ${res.failedCount} صفوف.`;
                    
                    this.messageService.add({
                        severity: res.failedCount > 0 ? 'warn' : 'success',
                        summary: 'نتيجة الاستيراد',
                        detail: summary,
                        life: 5000
                    });

                    if (res.errors && res.errors.length > 0) {
                        // عرض الأخطاء في رسالة منفصلة
                        setTimeout(() => {
                            this.messageService.add({
                                severity: 'error',
                                summary: 'أخطاء الاستيراد',
                                detail: res.errors.join('\n'),
                                life: 10000
                            });
                        }, 500);
                    }

                    if (res.successCount > 0 || res.updatedCount > 0) {
                        this.loadMedicines(this.lastLazyEvent);
                    }
                },
                error: (err) => {
                    this.isImporting.set(false);
                    event.target.value = '';
                    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في استيراد الملف' });
                }
            });
        }
    }
}
