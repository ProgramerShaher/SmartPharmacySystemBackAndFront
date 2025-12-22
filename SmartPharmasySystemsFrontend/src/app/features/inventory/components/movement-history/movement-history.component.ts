import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { InventoryService } from '../../services/inventory.service';
import { InventoryMovement, InventoryMovementQueryDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FormsModule } from '@angular/forms';
import { MovementFormComponent } from './movement-form.component';
import { MovementDetailsComponent } from './movement-details.component';

@Component({
    selector: 'app-movement-history',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        CalendarModule,
        DropdownModule,
        TagModule,
        TooltipModule,
        DialogModule,
        ToastModule,
        ConfirmDialogModule,
        MovementFormComponent,
        MovementDetailsComponent
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './movement-history.component.html',
    styleUrl: './movement-history.component.scss'
})
export class MovementHistoryComponent implements OnInit, OnDestroy {
    movements: InventoryMovement[] = [];
    loading = true;
    totalRecords = 0;

    displayForm = false;
    displayDetails = false;
    selectedMovement: InventoryMovement | null = null;

    query: InventoryMovementQueryDto = {
        page: 1,
        pageSize: 10,
        search: ''
    };

    movementTypes = [
        { label: 'الكل', value: '' },
        { label: 'دخول (توريد)', value: 'IN' },
        { label: 'خروج (صرف)', value: 'OUT' },
        { label: 'مرتجع', value: 'RETURN' },
        { label: 'تالف', value: 'DAMAGE' },
        { label: 'تعديل', value: 'ADJUSTMENT' }
    ];

    private searchSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private inventoryService: InventoryService,
        private confirmationService: ConfirmationService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadMovements();

        this.searchSubject.pipe(
            debounceTime(400),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(searchText => {
            this.query.search = searchText;
            this.query.page = 1;
            this.loadMovements();
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(event: any) {
        const value = event.target.value;
        this.searchSubject.next(value);
    }

    onFilterChange() {
        this.query.page = 1;
        this.loadMovements();
    }

    loadMovements(event?: any) {
        this.loading = true;
        if (event) {
            this.query.page = (event.first / event.rows) + 1;
            this.query.pageSize = event.rows;
        }

        this.inventoryService.getAllMovements(this.query).subscribe({
            next: (data) => {
                this.movements = data.items || [];
                this.totalRecords = data.totalCount || 0;
                this.loading = false;
            },
            error: (e) => {
                console.error('Error loading movements:', e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل سجل الحركات المخزنية' });
                this.loading = false;
            }
        });
    }

    createMovement() {
        this.selectedMovement = null;
        this.displayForm = true;
    }

    editMovement(movement: InventoryMovement) {
        this.selectedMovement = { ...movement };
        this.displayForm = true;
    }

    viewDetails(movement: InventoryMovement) {
        this.selectedMovement = movement;
        this.displayDetails = true;
    }

    deleteMovement(movement: InventoryMovement) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف هذه الحركة لمادة "${movement.medicine?.name}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.inventoryService.deleteMovement(movement.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم الحذف بنجاح' });
                        this.loadMovements();
                    },
                    error: (e) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في عملية الحذف' })
                });
            }
        });
    }

    getMovementTypeSeverity(type: string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
        switch (type) {
            case 'IN': return 'success';
            case 'OUT': return 'info';
            case 'RETURN': return 'warning';
            case 'DAMAGE': return 'danger';
            case 'ADJUSTMENT': return 'secondary';
            default: return 'secondary';
        }
    }

    getMovementTypeLabel(type: string): string {
        return this.movementTypes.find(t => t.value === type)?.label || type;
    }

    onFormSave() {
        this.displayForm = false;
        this.loadMovements();
        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: this.selectedMovement ? 'تم التحديث بنجاح' : 'تمت الإضافة بنجاح' });
    }

    onFormCancel() {
        this.displayForm = false;
    }
}
