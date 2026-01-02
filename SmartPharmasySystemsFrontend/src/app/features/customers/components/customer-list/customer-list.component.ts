import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../../../core/models/customer.models';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG Modules
import { TableModule } from 'primeng/table';
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
      plugins: {
        legend: {
          labels: {
            color: textColor
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            color: textColor
          },
          grid: {
            color: 'rgba(255, 255, 255, 0.1)'
          }
        },
        x: {
          ticks: {
            color: textColor
          },
          grid: {
            color: 'rgba(255, 255, 255, 0.1)'
          }
        }
      }
    };
  }

  loadCustomers() {
    this.loading.set(true);
    this.customerService.getAll({ pageSize: 1000 }).subscribe({
      next: (res) => {
        this.customers.set(res.items);
        this.updateCharts(res.items);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة العملاء' });
        this.loading.set(false);
      }
    });
  }

  updateCharts(customers: Customer[]) {
    const activeCount = customers.filter(c => c.isActive).length;
    const inactiveCount = customers.length - activeCount;

    this.activeStatusData = {
      labels: ['نشط', 'خامل'],
      datasets: [
        {
          data: [activeCount, inactiveCount],
          backgroundColor: ['#10b981', '#ef4444'],
          hoverBackgroundColor: ['#059669', '#dc2626']
        }
      ]
    };

    const lowDebt = customers.filter(c => c.balance <= 1000).length;
    const midDebt = customers.filter(c => c.balance > 1000 && c.balance <= 5000).length;
    const highDebt = customers.filter(c => c.balance > 5000).length;

    this.debtDistributionData = {
      labels: ['منخفضة', 'متوسطة', 'مرتفعة'],
      datasets: [
        {
          label: 'توزيع المديونية',
          data: [lowDebt, midDebt, highDebt],
          backgroundColor: ['#3b82f6', '#f59e0b', '#ef4444'],
          borderColor: ['#2563eb', '#d97706', '#dc2626'],
          borderWidth: 1
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
    this.loadCustomers(); // Reload list
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
