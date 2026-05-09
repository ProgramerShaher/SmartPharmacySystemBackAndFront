import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { take } from 'rxjs/operators';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.models';
import { MessageService, ConfirmationService, MenuItem } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { MenuModule } from 'primeng/menu';
import { DialogModule } from 'primeng/dialog';
import { SupplierAddEditComponent } from '../supplier-add-edit/supplier-add-edit.component';
import { SupplierPaymentsComponent } from '../supplier-payments/supplier-payments.component';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    MenuModule,
    DialogModule,
    SupplierAddEditComponent,
    SupplierPaymentsComponent
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './supplier-list.component.html',
  styleUrls: ['./supplier-list.component.scss']
})
export class SupplierListComponent implements OnInit {
  suppliers = signal<Supplier[]>([]);
  loading = signal(false);
  totalDebt = signal(0);
  highDebtCount = computed(() => this.suppliers().filter(s => (s.Balance || 0) > 10000).length);

  totalRecords = signal(0);
  pageSize = signal(10);

  addEditDialogVisible = signal(false);
  paymentsDialogVisible = signal(false);
  editSupplierId = signal<number | null>(null);
  selectedSupplierIdForPayment = signal<number | null>(null);

  items: MenuItem[] = [];

  constructor(
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.supplierService.getTotalDebt().subscribe({
      next: (debt) => this.totalDebt.set(debt)
    });
  }

  loadSuppliers(event?: any) {
    this.loading.set(true);

    const page = event ? (event.first / event.rows) + 1 : 1;
    const pageSize = event ? event.rows : 10;
    const sortBy = event?.sortField || 'Name';
    const sortDir = event?.sortOrder === 1 ? 'asc' : 'desc';

    this.supplierService.getAll({ page, pageSize, sortBy, sortDir }).pipe(take(1)).subscribe({
      next: (res) => {
        this.suppliers.set(res.items);
        this.totalRecords.set(res.totalCount);
        this.pageSize.set(pageSize);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة الموردين' });
        this.loading.set(false);
      }
    });
  }

  openCreateSupplier() {
    this.editSupplierId.set(null);
    this.addEditDialogVisible.set(true);
  }

  openEditSupplier(supplier: Supplier) {
    this.editSupplierId.set(supplier.id);
    this.addEditDialogVisible.set(true);
  }

  closeAddEditDialog() {
    this.addEditDialogVisible.set(false);
  }

  onSupplierSaved() {
    this.loadSuppliers();
  }

  openPaymentDialog(supplier?: Supplier) {
    this.selectedSupplierIdForPayment.set(supplier?.id ?? null);
    this.paymentsDialogVisible.set(true);
  }

  closePaymentsDialog() {
    this.paymentsDialogVisible.set(false);
  }

  deleteSupplier(event: Event, supplier: Supplier) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المورد ${supplier.name}؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.supplierService.delete(supplier.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حذف المورد' });
            this.loadSuppliers();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حذف المورد' })
        });
      }
    });
  }

  getRandomColor(name: string): string {
    const colors = ['#2563eb', '#0ea5e9', '#22c55e', '#f59e0b'];
    return colors[(name?.charCodeAt(0) || 0) % colors.length];
  }

  showMenu(menu: any, event: any, supplier: Supplier) {
    this.items = [
      { label: 'الملف الشخصي', icon: 'pi pi-id-card', routerLink: ['/partners/suppliers/detail', supplier.id] },
      { label: 'كشف الحساب', icon: 'pi pi-file-pdf', routerLink: ['/partners/suppliers/statement', supplier.id] },
      { label: 'سند صرف جديد', icon: 'pi pi-wallet', command: () => this.openPaymentDialog(supplier) },
      { separator: true },
      { label: 'تعديل البيانات', icon: 'pi pi-pencil', command: () => this.openEditSupplier(supplier) },
      { label: 'حذف المورد', icon: 'pi pi-trash', styleClass: 'text-red-500', command: (e) => this.deleteSupplier(e.originalEvent!, supplier) }
    ];
    menu.toggle(event);
  }
}
