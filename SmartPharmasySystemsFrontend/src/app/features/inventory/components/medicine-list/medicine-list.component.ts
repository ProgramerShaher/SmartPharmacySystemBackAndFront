import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { SidebarModule } from 'primeng/sidebar';
import { MenuModule } from 'primeng/menu';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService, LazyLoadEvent } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { MedicineService } from '../../services/medicine.service';
import { MedicineDto, MedicineQueryDto, MedicineBatchResponseDto } from '../../../../core/models';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { ToastModule } from "primeng/toast";

import { MedicineAddEditComponent } from '../medicine-add-edit/medicine-add-edit.component';

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
        SidebarModule,
        MenuModule,
        TooltipModule,
        ConfirmDialogModule,
        DialogModule,
        ToastModule,
        MedicineAddEditComponent
    ],
    templateUrl: './medicine-list.component.html',
    styleUrls: ['./medicine-list.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class MedicineListComponent implements OnInit {
    // Data
    medicines: MedicineDto[] = [];
    selectedMedicine: MedicineDto | null = null;
    fefoBatches: MedicineBatchResponseDto[] = [];

    // Pagination & Loading
    totalRecords = 0;
    loading = true;
    pageSize = 10;

    // Search
    private searchSubject = new Subject<string>();
    searchTerm = '';

    // Filters
    categoryId: number | undefined;
    manufacturer: string | undefined;
    status: string | undefined;

    // UI State
    showSidebar = false; // For Add/Edit
    isEditMode = false;

    statusOptions = [
        { label: 'الكل', value: null },
        { label: 'نشط', value: 'Active' },
        { label: 'غير نشط', value: 'Inactive' }
    ];

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
            this.searchTerm = term;
            this.loadMedicines({ first: 0, rows: this.pageSize });
        });
    }

    /**
     * Load medicines with server-side pagination
     */
    loadMedicines(event: any) {
        this.loading = true;
        const page = (event.first / event.rows) + 1;
        const pageSize = event.rows;

        // Sorting
        const sortBy = event.sortField;
        const sortDescending = event.sortOrder === -1;

        const query: MedicineQueryDto = {
            page,
            pageSize,
            search: this.searchTerm,
            categoryId: this.categoryId,
            manufacturer: this.manufacturer,
            status: this.status,
            sortBy,
            sortDescending
        };

        this.medicineService.search(query).subscribe({
            next: (result) => {
                this.medicines = result.items;
                this.totalRecords = result.totalCount;
                this.loading = false;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الأدوية' });
                this.loading = false;
            }
        });
    }

    /**
     * Trigger search
     */
    onSearch(value: string) {
        this.searchSubject.next(value);
    }

    /**
     * View details (Navigate to separate page)
     */
    viewDetails(medicine: MedicineDto) {
        this.router.navigate(['/inventory/medicines/details', medicine.id]);
    }

    /* 
    // Legacy Side Panel Logic - Removed as per user request
    loadBatches(id: number) { ... }
    */

    loadBatches(id: number) {
        this.medicineService.getFEFOBatches(id).subscribe({
            next: (batches) => {
                this.fefoBatches = batches;
            },
            error: (err) => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الدفعات' });
            }
        });
    }

    /**
     * Add/Edit Actions
     */
    addMedicine() {
        this.selectedMedicine = null;
        this.isEditMode = false;
        this.showSidebar = true;
    }

    editMedicine(medicine: MedicineDto) {
        this.selectedMedicine = medicine;
        this.isEditMode = true;
        this.showSidebar = true;
    }

    onFormSave() {
        this.showSidebar = false;
        this.loadMedicines({ first: 0, rows: this.pageSize });
    }

    deleteMedicine(medicine: MedicineDto) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف ${medicine.name}؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.medicineService.delete(medicine.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الدواء بنجاح' });
                        this.loadMedicines({ first: 0, rows: this.pageSize });
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'لا يمكن حذف الدواء لارتباطه بسجلات أخرى' });
                    }
                });
            }
        });
    }

    /**
     * UI Helpers
     */
    getStatusSeverity(status: string): 'success' | 'danger' | 'warning' {
        return status === 'Active' ? 'success' : 'danger';
    }

    getBatchExpirySeverity(batch: MedicineBatchResponseDto): 'success' | 'warning' | 'danger' {
        if (batch.isExpired) return 'danger';
        if (batch.isExpiringSoon) return 'warning';
        return 'success';
    }
}