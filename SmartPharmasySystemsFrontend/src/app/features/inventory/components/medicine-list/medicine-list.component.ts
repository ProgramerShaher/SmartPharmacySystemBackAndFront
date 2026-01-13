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
        BatchAddEditComponent
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

    // Filters
    searchTerm = signal('');
    categoryId = signal<number | undefined>(undefined);
    status = signal<string | undefined>(undefined);

    // UI State
    showMedicineDialog = signal(false);
    showBatchDialog = signal(false);

    selectedMedicine: Medicine | null = null;
    selectedMedicineIdForBatch = 0;
    selectedMedicineNameForBatch = '';

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
            this.loadMedicines(this.lastLazyEvent);
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
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'لا يمكن حذف الدواء لارتباطه بسجلات أخرى' });
                    }
                });
            }
        });
    }

    // --- Batch Actions (Inventory Setup) ---

    openAddBatch(medicine: Medicine) {
        this.selectedMedicineIdForBatch = medicine.id;
        this.selectedMedicineNameForBatch = medicine.name;
        this.showBatchDialog.set(true);
    }

    onBatchSaved() {
        // Maybe refresh list to show updated stock if backend updates it?
        // Or just show success. 
        // Ideally, reloading medicines might update the 'Stock' or 'TotalQuantity' column if computed by backend.
        this.loadMedicines(this.lastLazyEvent);
    }

    // --- View Details ---
    viewDetails(medicine: Medicine) {
        this.router.navigate(['/inventory/medicines/details', medicine.id]);
    }

    getStatusSeverity(status: string): 'success' | 'danger' | 'warning' | 'info' {
        return status === 'Active' ? 'success' : 'danger';
    }
}