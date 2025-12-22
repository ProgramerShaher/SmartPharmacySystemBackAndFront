import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { InventoryService } from '../../services/inventory.service';
import { Medicine } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MedicineAddEditComponent } from '../medicine-add-edit/medicine-add-edit.component';

@Component({
    selector: 'app-medicine-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        ConfirmDialogModule,
        TagModule,
        TooltipModule,
        DialogModule,
        ToastModule,
        MedicineAddEditComponent
    ],
    templateUrl: './medicine-list.component.html'
})
export class MedicineListComponent implements OnInit, OnDestroy {
    medicines: Medicine[] = [];
    displayModal = false;
    selectedMedicineId: number | null = null;
    lastUpdate = new Date();

    private searchSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private inventoryService: InventoryService,
        private router: Router,
        private confirmationService: ConfirmationService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadMedicines();

        this.searchSubject.pipe(
            debounceTime(400),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(searchText => {
            this.inventoryService.searchMedicines({ search: searchText }).subscribe(data => {
                this.medicines = data.items || [];
            });
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        this.searchSubject.next(value);
    }

    loadMedicines() {
        this.inventoryService.searchMedicines({}).subscribe({
            next: (data) => {
                this.medicines = data.items || [];
                this.lastUpdate = new Date();
            },
            error: (e: any) => {
                console.error('❌ Error loading medicines:', e);
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل قائمة الأدوية'
                });
            }
        });
    }

    createMedicine() {
        this.selectedMedicineId = null;
        this.displayModal = true;
    }

    editMedicine(medicine: Medicine) {
        this.selectedMedicineId = medicine.id;
        this.displayModal = true;
    }

    viewMedicine(medicine: Medicine) {
        this.router.navigate(['/inventory/medicines/details', medicine.id]);
    }

    onModalSave() {
        this.displayModal = false;
        this.loadMedicines();
    }

    onModalCancel() {
        this.displayModal = false;
    }

    exportExcel() {
        import('xlsx').then((xlsx) => {
            const worksheet = xlsx.utils.json_to_sheet(this.medicines);
            const workbook = { Sheets: { data: worksheet }, SheetNames: ['data'] };
            const excelBuffer: any = xlsx.write(workbook, {
                bookType: 'xlsx',
                type: 'array',
            });
            this.saveAsExcelFile(excelBuffer, 'medicines');
        });
    }

    saveAsExcelFile(buffer: any, fileName: string): void {
        import('file-saver').then((FileSaver) => {
            let EXCEL_TYPE =
                'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
            let EXCEL_EXTENSION = '.xlsx';
            const data: Blob = new Blob([buffer], {
                type: EXCEL_TYPE,
            });
            FileSaver.saveAs(
                data,
                fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION
            );
        });
    }

    deleteMedicine(medicine: Medicine) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف ' + medicine.name + '؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.inventoryService.deleteMedicine(medicine.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'تم بنجاح',
                            detail: 'تم حذف الدواء بنجاح',
                            life: 3000
                        });
                        this.loadMedicines();
                    },
                    error: (e) => this.messageService.add({
                        severity: 'error',
                        summary: 'خطأ',
                        detail: 'لم يتم حذف الدواء'
                    })
                });
            }
        });
    }

    hasExpiredBatch(medicine: Medicine): boolean {
        if (!medicine.medicineBatches || medicine.medicineBatches.length === 0) return false;
        const today = new Date();
        return medicine.medicineBatches.some(batch => new Date(batch.expiryDate) < today);
    }

    getSeverity(medicine: Medicine): 'success' | 'warning' | 'danger' | undefined {
        const stock = medicine.stock ?? medicine.totalQuantity ?? 0;
        if (stock > 10) return 'success';
        if (stock > 0) return 'warning';
        return 'danger';
    }
}