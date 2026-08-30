import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { Customer, CustomerStatistics } from '../../../../core/models/customer.models';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG Modules
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ProgressBarModule } from 'primeng/progressbar';
import { ChartModule } from 'primeng/chart';
import { SidebarModule } from 'primeng/sidebar';
import { DialogModule } from 'primeng/dialog';
import { MenuModule } from 'primeng/menu';
import { CustomerAddEditComponent } from '../customer-add-edit/customer-add-edit.component';
import { CustomerReceiptsComponent } from '../customer-receipts/customer-receipts.component';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    ProgressBarModule,
    ChartModule,
    SidebarModule,
    DialogModule,
    MenuModule,
    CustomerAddEditComponent,
    CustomerReceiptsComponent
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './customer-list.component.html',
  styleUrls: ['../../../partners/components/supplier-list/supplier-list.component.scss']
})
export class CustomerListComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading = signal(false);

  // Side Drawer
  displayAddEditSidebar = signal(false);
  selectedCustomerId: number | null = null;
  sideBarHeader = signal('إضافة عميل جديد');

  // Receipts Dialog - using regular boolean for PrimeNG two-way binding
  receiptsDialogVisible = signal(false);
  receiptsVisible = false;  // regular bool for [(visible)]
  selectedCustomerIdForReceipt = signal<number | null>(null);

  // Stats
  totalDebt = computed(() => this.customers().reduce((sum, c) => sum + (c.balance || 0), 0));
  activeCustomersCount = computed(() => this.customers().filter(c => c.isActive).length);
  highDebtCustomersCount = computed(() => this.customers().filter(c => (c.balance || 0) > 5000).length);

  items: any[] = [];

  constructor(
    private customerService: CustomerService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadCustomers();
    this.loadStatistics();
  }

  loadStatistics() {
    this.customerService.getStatistics().subscribe({
      next: (stats) => {
        // Stats loaded successfully
      }
    });
  }

  totalRecords = signal(0);
  lastTableEvent: any | null = null;

  loadCustomers(event?: any) {
    this.loading.set(true);
    this.lastTableEvent = event || this.lastTableEvent;

    const page = event ? (event.first! / event.rows!) + 1 : 1;
    const pageSize = event ? event.rows! : 10;
    const search = event?.globalFilter ? event.globalFilter.toString() : '';

    this.customerService.getAll({ page, pageSize, search }).subscribe({
      next: (res) => {
        this.customers.set(res.items);
        this.totalRecords.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة العملاء' });
        this.loading.set(false);
      }
    });
  }

  getRandomColor(name: string): string {
    const colors = ['#6366f1', '#ec4899', '#f59e0b', getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981', '#3b82f6', '#8b5cf6'];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  // Sidebar Methods
  openAddCustomer() {
    this.selectedCustomerId = null;
    this.sideBarHeader.set('إضافة عميل جديد');
    this.displayAddEditSidebar.set(true);
  }

  openEditCustomer(id: number) {
    this.selectedCustomerId = id;
    this.sideBarHeader.set('تعديل بيانات العميل');
    this.displayAddEditSidebar.set(true);
  }

  onAddEditSave() {
    this.displayAddEditSidebar.set(false);
    this.loadCustomers(this.lastTableEvent || undefined);
  }

  onAddEditClose() {
    this.displayAddEditSidebar.set(false);
  }



  deleteCustomer(event: Event, customer: Customer) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف العميل ${customer.name}؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.customerService.delete(customer.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف العميل بنجاح' });
            this.loadCustomers();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في حذف العميل' })
        });
      }
    });
  }

  openReceiptDialog(customerId?: number) {
    this.selectedCustomerIdForReceipt.set(customerId ?? null);
    this.receiptsVisible = true;
    this.receiptsDialogVisible.set(true);
  }

  closeReceiptsDialog() {
    this.receiptsVisible = false;
    this.receiptsDialogVisible.set(false);
    this.loadCustomers(this.lastTableEvent || undefined);
  }
}
