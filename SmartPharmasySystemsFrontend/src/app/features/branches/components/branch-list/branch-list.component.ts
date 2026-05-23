import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { BranchService } from '../../services/branch.service';
import { BranchDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { BranchFormComponent } from '../branch-form/branch-form.component';

@Component({
    selector: 'app-branch-list',
    standalone: true,
    imports: [
        CommonModule, FormsModule, TableModule, ButtonModule, InputTextModule,
        TagModule, TooltipModule, ConfirmDialogModule, DropdownModule, ToastModule,
        BranchFormComponent
    ],
    templateUrl: './branch-list.component.html',
    styleUrls: ['./branch-list.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class BranchListComponent implements OnInit {
    branches = signal<BranchDto[]>([]);
    totalRecords = signal(0);
    loading = signal(false);

    // KPI
    activeCount = signal(0);
    inactiveCount = signal(0);
    mainCount = signal(0);
    subCount = signal(0);

    searchTerm = signal('');
    statusFilter = signal<string | undefined>(undefined);
    typeFilter = signal<number | undefined>(undefined);
    showFormDialog = signal(false);
    selectedBranch: BranchDto | null = null;
    isEditMode = false;

    statusOptions = [
        { label: 'الكل', value: undefined },
        { label: 'نشط', value: 'true' },
        { label: 'غير نشط', value: 'false' }
    ];

    typeOptions = [
        { label: 'الكل', value: undefined },
        { label: 'رئيسي', value: 1 },
        { label: 'فرعي', value: 2 }
    ];

    private searchSubject = new Subject<string>();

    constructor(
        private branchService: BranchService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.searchSubject.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => this.loadBranches());
    }

    ngOnInit() { this.loadBranches(); }

    loadBranches() {
        this.loading.set(true);
        this.branchService.getAll({
            search: this.searchTerm() || undefined,
            isActive: this.statusFilter() !== undefined ? this.statusFilter() === 'true' : undefined,
            branchType: this.typeFilter() || undefined
        }).subscribe({
            next: (data) => {
                this.branches.set(data);
                this.totalRecords.set(data.length);
                this.computeStats(data);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الفروع' });
                this.loading.set(false);
            }
        });
    }

    private computeStats(data: BranchDto[]) {
        this.activeCount.set(data.filter(b => b.isActive).length);
        this.inactiveCount.set(data.filter(b => !b.isActive).length);
        this.mainCount.set(data.filter(b => b.branchType === 1).length);
        this.subCount.set(data.filter(b => b.branchType === 2).length);
    }

    onSearch(value: string) {
        this.searchTerm.set(value);
        this.searchSubject.next(value);
    }

    onFilterChange() { this.loadBranches(); }

    resetFilters() {
        this.searchTerm.set('');
        this.statusFilter.set(undefined);
        this.typeFilter.set(undefined);
        this.loadBranches();
    }

    openAddDialog() {
        this.isEditMode = false;
        this.selectedBranch = null;
        this.showFormDialog.set(true);
    }

    openEditDialog(branch: BranchDto) {
        this.isEditMode = true;
        this.selectedBranch = { ...branch };
        this.showFormDialog.set(true);
    }

    onFormSaved() {
        this.showFormDialog.set(false);
        this.loadBranches();
    }

    onFormCancelled() { this.showFormDialog.set(false); }

    confirmDelete(id: number, name: string) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف الفرع "${name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.branchService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف الفرع بنجاح' });
                        this.loadBranches();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف الفرع' })
                });
            }
        });
    }

    getActiveSeverity(v: boolean) { return v ? 'success' : 'danger'; }
    getActiveLabel(v: boolean) { return v ? 'نشط' : 'غير نشط'; }
    getTypeLabel(t: number) { return t === 1 ? 'رئيسي' : t === 2 ? 'فرعي' : '--'; }
}
