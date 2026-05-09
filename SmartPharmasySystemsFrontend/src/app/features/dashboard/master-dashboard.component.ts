import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ChartModule } from 'primeng/chart';
import { SkeletonModule } from 'primeng/skeleton';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';

import { MasterDashboardService } from '../../core/services/master-dashboard.service';
import {
  CriticalStockItem,
  MasterDashboardStats
} from '../../core/models/master-dashboard.models';

@Component({
  selector: 'app-master-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ChartModule,
    ButtonModule,
    SkeletonModule,
    ToastModule,
    TableModule
  ],
  providers: [MessageService],
  templateUrl: './master-dashboard.component.html',
  styleUrl: './master-dashboard.component.css'
})
export class MasterDashboardComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(MasterDashboardService);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);
  private readonly platformId = inject(PLATFORM_ID);

  private refreshSubscription?: Subscription;
  readonly isBrowser = isPlatformBrowser(this.platformId);

  isLoading = signal(true);
  dashboardData = signal<MasterDashboardStats | null>(null);

  financialData = computed(() => this.dashboardData()?.financialIntelligence ?? null);
  inventoryData = computed(() => this.dashboardData()?.inventoryIntelligence ?? null);
  operationalData = computed(() => this.dashboardData()?.operationalPulse ?? null);
  systemOverview = computed(() => this.dashboardData()?.systemOverview ?? null);

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('ar-YE', {
      style: 'currency',
      currency: 'YER',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value ?? 0).replace('YER', 'ر.ي');
  }

  topMetrics = computed(() => {
    const financial = this.financialData();
    const inventory = this.inventoryData();

    if (!financial || !inventory) {
      return [];
    }

    return [
      {
        title: 'مبيعات اليوم',
        value: this.formatCurrency(financial.todayApprovedSales),
        hint: `آخر 30 يوم ${this.formatCurrency(financial.totalSalesLast30Days)}`,
        icon: 'pi pi-shopping-bag',
        tone: 'indigo',
        route: '/sales'
      },
      {
        title: 'صافي الربح',
        value: this.formatCurrency(financial.todayNetProfit),
        hint: `30 يوم ${this.formatCurrency(financial.netProfitLast30Days)}`,
        icon: 'pi pi-chart-line',
        tone: 'emerald',
        route: '/reports/net-profit'
      },
      {
        title: 'السيولة الحالية',
        value: this.formatCurrency(financial.totalLiquidity),
        hint: `صندوق ${this.formatCurrency(financial.cashBalance)} • بنك ${this.formatCurrency(financial.bankBalance)}`,
        icon: 'pi pi-wallet',
        tone: 'cyan',
        route: '/financial/ledger'
      },
      {
        title: 'قيمة المخزون',
        value: this.formatCurrency(inventory.totalInventoryValue),
        hint: `${inventory.totalStockQuantity} وحدة • ${inventory.criticalStockItems.length} أصناف حرجة`,
        icon: 'pi pi-box',
        tone: 'gold',
        route: '/inventory/medicines'
      }
    ];
  });

  systemCards = computed(() => {
    const overview = this.systemOverview();
    const inventory = this.inventoryData();

    if (!overview || !inventory) {
      return [];
    }

    return [
      { title: 'فواتير البيع', value: overview.salesInvoicesCount, icon: 'pi pi-receipt', route: '/sales', tone: 'indigo' },
      { title: 'فواتير الشراء', value: overview.purchaseInvoicesCount, icon: 'pi pi-cart-plus', route: '/purchases', tone: 'cyan' },
      { title: 'مرتجعات البيع', value: overview.salesReturnsCount, icon: 'pi pi-undo', route: '/sales/returns', tone: 'rose' },
      { title: 'مرتجعات الشراء', value: overview.purchaseReturnsCount, icon: 'pi pi-replay', route: '/purchases/returns', tone: 'gold' },
      { title: 'الأدوية', value: overview.medicinesCount, icon: 'pi pi-box', route: '/inventory/medicines', tone: 'emerald' },
      { title: 'الدفعات النشطة', value: inventory.activeBatches, icon: 'pi pi-database', route: '/inventory/batches', tone: 'cyan' },
      { title: 'العملاء', value: overview.customersCount, icon: 'pi pi-users', route: '/customers', tone: 'indigo' },
      { title: 'الموردين', value: overview.suppliersCount, icon: 'pi pi-truck', route: '/partners/suppliers', tone: 'gold' },
      { title: 'التنبيهات', value: overview.activeAlertsCount, icon: 'pi pi-bell', route: '/system-alerts', tone: 'rose' },
      { title: 'وثائق اليوم', value: overview.todayDocumentsCount, icon: 'pi pi-calendar-clock', route: '/reports/daily-sales', tone: 'emerald' }
    ];
  });

  financeSnapshot = computed(() => {
    const financial = this.financialData();

    if (!financial) {
      return [];
    }

    return [
      {
        label: 'لنا عند العملاء',
        value: this.formatCurrency(financial.customerReceivables),
        tone: 'emerald'
      },
      {
        label: 'علينا للموردين',
        value: this.formatCurrency(financial.supplierPayables),
        tone: 'rose'
      },
      {
        label: 'المديونية الصافية',
        value: this.formatCurrency(financial.netDebt),
        tone: financial.netDebt > 0 ? 'cyan' : 'gold'
      }
    ];
  });

  inventoryHealth = computed(() => {
    const inventory = this.inventoryData();

    if (!inventory) {
      return [];
    }

    return [
      {
        label: 'أقل من 3 أشهر',
        value: `${inventory.expiryRadar.percentageLessThan3Months}%`,
        tone: 'rose'
      },
      {
        label: '3 إلى 6 أشهر',
        value: `${inventory.expiryRadar.percentage3To6Months}%`,
        tone: 'gold'
      },
      {
        label: '6 إلى 12 شهر',
        value: `${inventory.expiryRadar.percentage6To12Months}%`,
        tone: 'indigo'
      },
      {
        label: 'أكثر من سنة',
        value: `${inventory.expiryRadar.percentageMoreThan12Months}%`,
        tone: 'emerald'
      }
    ];
  });

  activityPreview = computed(() => this.operationalData()?.activityStream.slice(0, 6) ?? []);
  criticalStockPreview = computed(() => this.inventoryData()?.criticalStockItems.slice(0, 6) ?? []);
  fullActivity = computed(() => this.operationalData()?.activityStream ?? []);

  cashFlowChartData: any;
  cashFlowChartOptions: any;
  cashierPerformanceChartData: any;
  cashierPerformanceChartOptions: any;
  inventoryDistributionData: any;
  inventoryDistributionOptions: any;
  hourlyActivityChartData: any;
  hourlyActivityChartOptions: any;

  ngOnInit(): void {
    if (!this.isBrowser) {
      this.isLoading.set(false);
      return;
    }

    this.initializeCharts();
    this.loadDashboardData();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  refreshDashboard(): void {
    this.loadDashboardData();
    this.messageService.add({
      severity: 'success',
      summary: 'تم التحديث',
      detail: 'تم تحديث بيانات لوحة التحكم',
      life: 2000
    });
  }

  navigateToFinancialReport(): void {
    this.router.navigate(['/financial/dashboard']);
  }

  navigateToInventoryReport(): void {
    this.router.navigate(['/inventory/medicines']);
  }

  navigateToRoute(route: string): void {
    if (route) {
      this.router.navigateByUrl(route);
    }
  }

  navigateToDocument(operationType: string, referenceId: number): void {
    const routes: Record<string, string> = {
      SaleInvoice: '/sales',
      PurchaseInvoice: '/purchases',
      SalesReturn: '/sales/returns',
      PurchaseReturn: '/purchases/returns',
      SupplierPayment: '/partners/suppliers/payments',
      CustomerReceipt: '/customers/receipts',
      Expense: '/finance/expenses/edit',
      InventoryMovement: '/inventory/movements',
      Payment: '/financial/ledger'
    };

    const basePath = routes[operationType] || '/';
    this.router.navigate([`${basePath}/${referenceId}`]);
  }

  getActivityIcon(type: string): string {
    const icons: Record<string, string> = {
      SaleInvoice: 'pi-shopping-cart',
      PurchaseInvoice: 'pi-shopping-bag',
      SalesReturn: 'pi-replay',
      PurchaseReturn: 'pi-arrow-circle-left',
      SupplierPayment: 'pi-wallet',
      CustomerReceipt: 'pi-money-bill',
      Expense: 'pi-minus-circle',
      InventoryMovement: 'pi-sync',
      Payment: 'pi-credit-card'
    };

    return icons[type] || 'pi-file';
  }

  getActivityColor(type: string): string {
    const colors: Record<string, string> = {
      SaleInvoice: '#10b981',
      PurchaseInvoice: '#6366f1',
      SalesReturn: '#f43f5e',
      PurchaseReturn: '#f59e0b',
      SupplierPayment: '#e11d48',
      CustomerReceipt: '#0ea5e9',
      Expense: '#7c3aed',
      InventoryMovement: '#64748b',
      Payment: '#4f46e5'
    };

    return colors[type] || '#64748b';
  }

  formatTime(date: Date | string): string {
    return new Date(date).toLocaleTimeString('ar-SA', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getStockSeverity(item: CriticalStockItem): string {
    return item.currentStock <= Math.max(1, Math.floor(item.reorderPoint * 0.4)) ? 'حرج' : 'منخفض';
  }

  stockSeverityClass(item: CriticalStockItem): string {
    return this.getStockSeverity(item) === 'حرج' ? 'severity-critical' : 'severity-low';
  }

  private loadDashboardData(): void {
    this.isLoading.set(true);

    this.dashboardService.getMasterDashboardStats().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
        this.updateCharts(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading dashboard data:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل بيانات لوحة التحكم',
          life: 3000
        });
        this.isLoading.set(false);
      }
    });
  }

  private initializeCharts(): void {
    const labelFont = { family: 'Cairo, sans-serif', size: 11, weight: '700' as const };
    const axisColor = '#64748b';
    const gridColor = 'rgba(148, 163, 184, 0.08)';

    this.cashFlowChartOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          align: 'end',
          labels: { color: '#0f172a', font: labelFont, usePointStyle: true }
        },
        tooltip: this.buildTooltipOptions()
      },
      scales: {
        x: { ticks: { color: axisColor, font: labelFont }, grid: { display: false } },
        y: { ticks: { color: axisColor, font: labelFont }, grid: { color: gridColor } }
      }
    };

    this.cashierPerformanceChartOptions = {
      indexAxis: 'y',
      maintainAspectRatio: false,
      plugins: { legend: { display: false }, tooltip: this.buildTooltipOptions('currency') },
      scales: {
        x: { ticks: { color: axisColor, font: labelFont }, grid: { color: gridColor } },
        y: { ticks: { color: '#0f172a', font: labelFont }, grid: { display: false } }
      }
    };

    this.inventoryDistributionOptions = {
      maintainAspectRatio: false,
      cutout: '75%',
      plugins: {
        legend: { position: 'bottom', labels: { color: '#0f172a', font: labelFont, usePointStyle: true, padding: 20 } },
        tooltip: this.buildTooltipOptions('currency')
      }
    };

    this.hourlyActivityChartOptions = {
      maintainAspectRatio: false,
      plugins: { legend: { display: false }, tooltip: this.buildTooltipOptions('count') },
      scales: {
        x: { ticks: { color: axisColor, font: labelFont }, grid: { display: false } },
        y: { ticks: { color: axisColor, font: labelFont }, grid: { color: gridColor } }
      }
    };
  }

  private updateCharts(data: MasterDashboardStats): void {
    const financial = data.financialIntelligence;
    const inventory = data.inventoryIntelligence;
    const operational = data.operationalPulse;

    const cashFlow = this.normalizeCashFlowSeries(
      financial?.cashFlowInLast30Days ?? [],
      financial?.cashFlowOutLast30Days ?? []
    );

    this.cashFlowChartData = {
      labels: cashFlow.labels,
      datasets: [
        {
          label: 'التدفق الداخل',
          data: cashFlow.inbound,
          borderColor: '#10b981',
          backgroundColor: 'rgba(16, 185, 129, 0.08)',
          fill: true,
          tension: 0.4,
          borderWidth: 3,
          pointRadius: 0,
          pointHoverRadius: 6
        },
        {
          label: 'التدفق الخارج',
          data: cashFlow.outbound,
          borderColor: '#6366f1',
          backgroundColor: 'rgba(99, 102, 241, 0.08)',
          fill: true,
          tension: 0.4,
          borderWidth: 3,
          pointRadius: 0,
          pointHoverRadius: 6
        }
      ]
    };

    const cashierPerformance = (operational?.cashierPerformance ?? [])
      .slice()
      .sort((a, b) => b.totalSales - a.totalSales)
      .slice(0, 6);

    this.cashierPerformanceChartData = {
      labels: cashierPerformance.map((c) => c.username),
      datasets: [
        {
          data: cashierPerformance.map((c) => this.toNumber(c.totalSales)),
          borderRadius: 8,
          maxBarThickness: 20,
          backgroundColor: ['#6366f1', '#10b981', '#f59e0b', '#06b6d4', '#f43f5e', '#8b5cf6']
        }
      ]
    };

    const supplierData = (inventory?.inventoryBySupplier ?? [])
      .slice()
      .sort((a, b) => b.inventoryValue - a.inventoryValue)
      .slice(0, 5);
    this.inventoryDistributionData = {
      labels: supplierData.map((s) => s.supplierName),
      datasets: [
        {
          data: supplierData.map((s) => this.toNumber(s.inventoryValue)),
          backgroundColor: ['#10b981', '#6366f1', '#f59e0b', '#06b6d4', '#8b5cf6'],
          hoverOffset: 12,
          borderWidth: 0
        }
      ]
    };

    const hourlyHeatMap = (operational?.hourlyHeatMap ?? []).slice().sort((a, b) => a.hour - b.hour);
    this.hourlyActivityChartData = {
      labels: hourlyHeatMap.map((h) => `${h.hour}:00`),
      datasets: [
        {
          data: hourlyHeatMap.map((h) => this.toNumber(h.transactionCount)),
          borderRadius: 8,
          maxBarThickness: 16,
          backgroundColor: '#6366f1'
        }
      ]
    };
  }

  private startAutoRefresh(): void {
    this.refreshSubscription = interval(30000)
      .pipe(switchMap(() => this.dashboardService.getMasterDashboardStats()))
      .subscribe({
        next: (data) => {
          this.dashboardData.set(data);
          this.updateCharts(data);
        },
        error: (err) => console.error('Auto-refresh error:', err)
      });
  }

  private buildTooltipOptions(type: 'currency' | 'count' = 'currency'): any {
    return {
      rtl: true,
      backgroundColor: '#0f172a',
      titleColor: '#f8fafc',
      bodyColor: '#cbd5e1',
      cornerRadius: 12,
      padding: 12,
      titleFont: { family: 'Cairo, sans-serif', size: 13, weight: '900' },
      bodyFont: { family: 'Cairo, sans-serif', size: 12 },
      callbacks: {
        label: (context: any) => {
          const value = this.toNumber(context.parsed?.y ?? context.parsed?.x ?? context.raw);
          return type === 'count' ? `${value} عملية` : this.formatCurrency(value);
        }
      }
    };
  }

  private normalizeCashFlowSeries(
    inbound: Array<{ date: Date | string; amount: number }>,
    outbound: Array<{ date: Date | string; amount: number }>
  ): { labels: string[]; inbound: number[]; outbound: number[] } {
    const inboundByDate = this.toAmountMap(inbound);
    const outboundByDate = this.toAmountMap(outbound);
    const dateKeys = Array.from(new Set([...inboundByDate.keys(), ...outboundByDate.keys()]))
      .sort((a, b) => new Date(a).getTime() - new Date(b).getTime());

    return {
      labels: dateKeys.map((date) =>
        new Date(date).toLocaleDateString('ar-SA', { month: 'short', day: 'numeric' })
      ),
      inbound: dateKeys.map((date) => inboundByDate.get(date) ?? 0),
      outbound: dateKeys.map((date) => outboundByDate.get(date) ?? 0)
    };
  }

  private toAmountMap(items: Array<{ date: Date | string; amount: number }>): Map<string, number> {
    return items.reduce((map, item) => {
      const date = new Date(item.date);

      if (Number.isNaN(date.getTime())) {
        return map;
      }

      const key = date.toISOString().slice(0, 10);
      map.set(key, (map.get(key) ?? 0) + this.toNumber(item.amount));
      return map;
    }, new Map<string, number>());
  }

  private toNumber(value: number | string | null | undefined): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }
}
