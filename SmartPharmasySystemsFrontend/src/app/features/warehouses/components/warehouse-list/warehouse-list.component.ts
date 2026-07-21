import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { WarehouseService } from '../../services/warehouse.service';
import { BranchService } from '../../../branches/services/branch.service';
import { WarehouseDto, WarehouseType, BranchDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { WarehouseFormComponent } from '../warehouse-form/warehouse-form.component';

@Component({
    selector: 'app-warehouse-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule,
        TagModule, TooltipModule, ConfirmDialogModule, DropdownModule, ToastModule,
        WarehouseFormComponent
    ],
    templateUrl: './warehouse-list.component.html',
    styleUrls: ['./warehouse-list.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class WarehouseListComponent implements OnInit {
    warehouses = signal<WarehouseDto[]>([]);
    branches = signal<BranchDto[]>([]);
    totalRecords = signal(0);
    loading = signal(false);

    // KPI counters
    totalCount = signal(0);
    mainCount = signal(0);
    branchCount = signal(0);
    damagedCount = signal(0);

    searchTerm = signal('');
    branchFilter = signal<number | undefined>(undefined);
    typeFilter = signal<number | undefined>(undefined);
    showFormDialog = signal(false);
    selectedWarehouse: WarehouseDto | null = null;
    isEditMode = false;

    typeOptions = [
        { label: 'الكل', value: undefined },
        { label: 'رئيسي', value: WarehouseType.Main },
        { label: 'فرعي', value: WarehouseType.Branch },
        { label: 'تالف', value: WarehouseType.Damaged }
    ];

    WarehouseType = WarehouseType;
    private searchSubject = new Subject<string>();

    constructor(
        private warehouseService: WarehouseService,
        private branchService: BranchService,
        private route: ActivatedRoute,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.searchSubject.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => this.loadWarehouses());
    }

    ngOnInit() {
        this.loadBranches();
        this.route.queryParams.subscribe(params => {
            if (params['branchId']) {
                this.branchFilter.set(+params['branchId']);
            }
            this.loadWarehouses();
        });
    }

    loadBranches() {
        this.branchService.getActive().subscribe(data => this.branches.set(data));
    }

    loadWarehouses() {
        this.loading.set(true);
        this.warehouseService.getAll({
            search: this.searchTerm() || undefined,
            branchId: this.branchFilter() || undefined,
            type: this.typeFilter() || undefined
        }).subscribe({
            next: (data) => {
                this.warehouses.set(data);
                this.totalRecords.set(data.length);
                this.computeStats(data);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المخازن' });
                this.loading.set(false);
            }
        });
    }

    private computeStats(data: WarehouseDto[]) {
        this.totalCount.set(data.length);
        this.mainCount.set(data.filter(w => w.type === WarehouseType.Main).length);
        this.branchCount.set(data.filter(w => w.type === WarehouseType.Branch).length);
        this.damagedCount.set(data.filter(w => w.type === WarehouseType.Damaged).length);
    }

    onSearch(value: string) {
        this.searchTerm.set(value);
        this.searchSubject.next(value);
    }

    onFilterChange() { this.loadWarehouses(); }

    resetFilters() {
        this.searchTerm.set('');
        this.branchFilter.set(undefined);
        this.typeFilter.set(undefined);
        this.loadWarehouses();
    }

    getBranchName(branchId: number): string {
        return this.branches().find(b => b.id === branchId)?.name || `#${branchId}`;
    }

    openAddDialog() {
        this.isEditMode = false;
        this.selectedWarehouse = null;
        this.showFormDialog.set(true);
    }

    openEditDialog(warehouse: WarehouseDto) {
        this.isEditMode = true;
        this.selectedWarehouse = { ...warehouse };
        this.showFormDialog.set(true);
    }

    onFormSaved() {
        this.showFormDialog.set(false);
        this.loadWarehouses();
    }

    onFormCancelled() { this.showFormDialog.set(false); }

    getTypeSeverity(t: WarehouseType) {
        switch (t) {
            case WarehouseType.Main: return 'info';
            case WarehouseType.Branch: return 'warning';
            case WarehouseType.Damaged: return 'danger';
            default: return 'secondary';
        }
    }

    getTypeIcon(t: WarehouseType) {
        switch (t) {
            case WarehouseType.Main: return 'pi pi-building';
            case WarehouseType.Branch: return 'pi pi-sitemap';
            case WarehouseType.Damaged: return 'pi pi-exclamation-triangle';
            default: return 'pi pi-question';
        }
    }

    confirmDelete(id: number, name: string) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف المخزن "${name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.warehouseService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المخزن بنجاح' });
                        this.loadWarehouses();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف المخزن' })
                });
            }
        });
    }
}
