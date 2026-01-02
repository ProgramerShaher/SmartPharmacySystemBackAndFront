import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common'; // added DatePipe
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { Customer, CustomerTransaction } from '../../../../core/models/customer.models'; // Added CustomerTransaction
import { MessageService } from 'primeng/api';

// PrimeNG
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { ProgressBarModule } from 'primeng/progressbar';
import { ToastModule } from 'primeng/toast';
import { ChartModule } from 'primeng/chart'; // Added

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    DividerModule,
    TagModule,
    ProgressBarModule,
    ToastModule,
    ChartModule // Added
  ],
  providers: [MessageService, DatePipe], // Added DatePipe
  templateUrl: './customer-detail.component.html',
  styleUrls: ['./customer-detail.component.scss']
})
export class CustomerDetailComponent implements OnInit {
  customer = signal<Customer | null>(null);
  loading = signal(false);

  // Charts
  historyChartData: any;
  historyChartOptions: any;

  purchaseCategoryData: any;
  purchaseCategoryOptions: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    private messageService: MessageService,
    private datePipe: DatePipe
  ) { }

  ngOnInit() {
    this.initChartOptions();
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadCustomer(id);
    }
  }

  initChartOptions() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    this.historyChartOptions = {
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          mode: 'index',
          intersect: false,
          callbacks: {
            label: function (context: any) {
              return 'الرصيد: ' + new Intl.NumberFormat('en-US').format(context.parsed.y) + ' ر.ي';
            }
          }
        }
      },
      scales: {
        x: {
          ticks: {
            color: textColorSecondary,
            font: {
              size: 10
            }
          },
          grid: {
            color: surfaceBorder,
            drawBorder: false,
            display: false
          }
        },
        y: {
          display: false,
          grid: {
            display: false
          }
        }
      },
      interaction: {
        mode: 'nearest',
        axis: 'x',
        intersect: false
      },
      elements: {
        point: {
          radius: 0,
          hitRadius: 10
        },
        line: {
          tension: 0.4
        }
      },
      maintainAspectRatio: false
    };

    // Donut Chart Options
    this.purchaseCategoryOptions = {
      plugins: {
        legend: {
          display: false // Hide default legend to use custom clean layout if needed, or keep bottom
        }
      },
      cutout: '70%',
      maintainAspectRatio: false
    };
  }

  loadCustomer(id: number) {
    this.loading.set(true);
    this.customerService.getById(id).subscribe({
      next: (res) => {
        this.customer.set(res);
        this.loadTransactions(id);
        this.prepareMockCategoryData(); // Prepare mock data
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات العميل' });
        this.loading.set(false);
      }
    });
  }

  prepareMockCategoryData() {
    // Mock Data for "Purchase Categories"
    // In a real app, this would come from aggregating SaleInvoiceItems by Medicine Category
    this.purchaseCategoryData = {
      labels: ['أدوية مزمنة', 'مضادات حيوية', 'عناية شخصية', 'أطفال', 'عام'],
      datasets: [
        {
          data: [35, 25, 15, 15, 10], // Mock percentages
          backgroundColor: ['#6366f1', '#ec4899', '#10b981', '#f59e0b', '#cbd5e1'],
          hoverBackgroundColor: ['#4f46e5', '#db2777', '#059669', '#d97706', '#94a3b8'],
          borderWidth: 0
        }
      ]
    };
  }

  loadTransactions(id: number) {
    this.customerService.getTransactions(id).subscribe({
      next: (transactions) => {
        this.prepareHistoryChart(transactions);
      }
    });
  }

  prepareHistoryChart(transactions: CustomerTransaction[]) {
    if (!transactions || transactions.length === 0) return;

    // Sort by date ascending for chart
    const sorted = [...transactions].sort((a, b) => new Date(a.transactionDate).getTime() - new Date(b.transactionDate).getTime());

    // Take last 15 transactions for clarity
    const recent = sorted.slice(-15);

    const labels = recent.map(t => this.datePipe.transform(t.transactionDate, 'MM/dd'));
    const dataPoints = recent.map(t => t.runningBalance);

    this.historyChartData = {
      labels: labels,
      datasets: [
        {
          label: 'تطور الرصيد',
          data: dataPoints,
          fill: true,
          borderColor: '#6366f1',
          backgroundColor: 'rgba(99, 102, 241, 0.1)',
          borderWidth: 2,
          pointBackgroundColor: '#ffffff',
          pointBorderColor: '#6366f1',
          pointBorderWidth: 2
        }
      ]
    };
  }

  getDebtPercentage(): number {
    const c = this.customer();
    if (!c || !c.creditLimit || c.creditLimit === 0) return 0;
    const percentage = (c.balance / c.creditLimit) * 100;
    return Math.min(percentage, 100);
  }

  getDebtSeverity(): string {
    const p = this.getDebtPercentage();
    if (p > 90) return 'danger';
    if (p > 70) return 'warning';
    return 'success';
  }

  back() {
    this.router.navigate(['/customers']);
  }
}
