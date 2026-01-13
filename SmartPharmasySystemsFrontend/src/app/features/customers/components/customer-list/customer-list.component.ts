import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
import { SidebarModule } from 'primeng/sidebar'; // Added
import { CustomerAddEditComponent } from '../customer-add-edit/customer-add-edit.component'; // Added

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
    SidebarModule, // Added
    CustomerAddEditComponent // Added
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss']
})
export class CustomerListComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading = signal(false);

  // Side Drawer
  displayAddEditSidebar = signal(false);
  selectedCustomerId: number | null = null;
  sideBarHeader = signal('إضافة عميل جديد');

  // Stats
  totalDebt = computed(() => this.customers().reduce((sum, c) => sum + (c.balance || 0), 0));
  activeCustomersCount = computed(() => this.customers().filter(c => c.isActive).length);
  highDebtCustomersCount = computed(() => this.customers().filter(c => (c.balance || 0) > 5000).length);

  // Charts
  activeStatusData: any;
  activeStatusOptions: any;
  debtDistributionData: any;
  debtDistributionOptions: any;

  constructor(
    private customerService: CustomerService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit() {
    this.initChartOptions();
    this.loadCustomers();
    this.loadStatistics();
  }

  loadStatistics() {
    this.customerService.getStatistics().subscribe({
      next: (stats) => {
        this.updateCharts(stats);
        // Verify stats are loading
      }
    });
  }

  initChartOptions() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');

    this.activeStatusOptions = {
      cutout: '60%',
      plugins: {
        legend: {
          labels: {
            color: textColor
          }
        }
      }
    };

    this.debtDistributionOptions = {
      cutout: '60%', // Make it a doughnut
      plugins: {
        legend: {
          position: 'left', // Evaluate side legend for elegance
          labels: {
            color: textColor,
            usePointStyle: true, // Elegant dots
            font: {
              family: 'Cairo, sans-serif'
            }
          }
        }
      }
    };
  }

  totalRecords = signal(0);
  // loading is already defined at line 47, do not redefine it.

  // Keep track of last lazy load event for reloading
  // Note: TableLazyLoadEvent import should be at the top of the file, not here.
  lastTableEvent: any | null = null; // Using any temporarily if import is hard to move with replace, but best to use proper type if possible. 
  // actually, let's fix the import properly in a separate step or just use 'any' to stop the bleeding if the import is tricky, 
  // but wait, I can just use the type and assume I'll fix the import at the top.

  loadCustomers(event?: any) { // using any for TableLazyLoadEvent to avoid import issues for now, or I should have added the import at top.
    this.loading.set(true);
    this.lastTableEvent = event || this.lastTableEvent;

    // Default values if no event (initial load)
    const page = event ? (event.first! / event.rows!) + 1 : 1;
    const pageSize = event ? event.rows! : 10;
    const search = event?.globalFilter ? event.globalFilter.toString() : '';

    this.customerService.getAll({ page, pageSize, search }).subscribe({
      next: (res) => {
        this.customers.set(res.items);
        this.totalRecords.set(res.totalCount);
        this.loading.set(false);
        this.totalRecords.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة العملاء' });
        this.loading.set(false);
      }
    });
  }

  updateCharts(stats: CustomerStatistics) {
    this.activeStatusData = {
      labels: ['نشط', 'خامل'],
      datasets: [
        {
          data: [stats.activeCustomersCount, stats.inactiveCustomersCount],
          backgroundColor: ['#10b981', '#ef4444'],
          hoverBackgroundColor: ['#059669', '#dc2626']
        }
      ]
    };

    this.debtDistributionData = {
      labels: ['منخفضة (<1000)', 'متوسطة (<5000)', 'مرتفعة (>5000)'],
      datasets: [
        {
          data: [stats.lowDebtCount, stats.mediumDebtCount, stats.highDebtCustomersCount],
          backgroundColor: ['#3b82f6', '#f59e0b', '#ef4444'],
          hoverBackgroundColor: ['#2563eb', '#d97706', '#dc2626'],
          borderWidth: 0 // Cleaner look without borders for doughnut
        }
      ]
    };
  }

  getRandomColor(name: string): string {
    const colors = ['#6366f1', '#ec4899', '#f59e0b', '#10b981', '#3b82f6', '#8b5cf6'];
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
    this.loadCustomers(this.lastTableEvent || undefined); // Reload list with current state
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

  openReceiptDialog() {
    // Logic for opening receipt dialog or navigating to receipts
  }
}
