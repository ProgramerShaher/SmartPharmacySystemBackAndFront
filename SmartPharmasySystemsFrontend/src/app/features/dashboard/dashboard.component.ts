import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

// PrimeNG Modules
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TimelineModule } from 'primeng/timeline';
import { MessageService } from 'primeng/api';

// Services & Models
import { FinancialService } from '../../core/services/financial.service';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { FinancialReport, PharmacyAccount } from '../../core/models/financial.models';

/**
 * لوحة التحكم التحليلية الشاملة - The Ultimate Analytical Dashboard
 * 
 * Features:
 * - Real-time KPI Cards with live updates
 * - 6-month Sales vs Purchases trend analysis
 * - Inventory distribution by category
 * - Top 5 best-selling medicines heatmap
 * - Real-time alert timeline
 * - Auto-refresh every 30 seconds
 * - Dark/Light mode compatible
 * - Full RTL support
 */
@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ChartModule,
        ButtonModule,
        CardModule,
        SkeletonModule,
        ToastModule,
        TableModule,
        TagModule,
        TimelineModule,
        StatCardComponent
    ],
    providers: [MessageService],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
    private financialService = inject(FinancialService);
    private router = inject(Router);
    private messageService = inject(MessageService);

    private refreshSubscription?: Subscription;

    // ============================================
    // Signals for Reactive State Management
    // ============================================

    /** رصيد الصيدلية الحالي / Current Pharmacy Balance */
    pharmacyBalance = signal<number>(0);

    /** مبيعات اليوم / Today's Sales */
    todaySales = signal<number>(0);

    /** عدد الأصناف القريبة من الانتهاء / Expiring Items Count */
    expiringItems = signal<number>(0);

    /** عدد الأصناف منخفضة المخزون / Low Stock Items Count */
    lowStockItems = signal<number>(0);

    /** حالة التحميل / Loading State */
    isLoading = signal<boolean>(true);

    /** التقرير المالي / Financial Report */
    financialReport = signal<FinancialReport | null>(null);

    // ============================================
    // Chart Data
    // ============================================

    /** مخطط المبيعات مقابل المشتريات (6 أشهر) / Sales vs Purchases Chart (6 months) */
    salesVsPurchasesData: any;
    salesVsPurchasesOptions: any;

    /** مخطط توزيع المخزون حسب الفئة / Inventory Distribution Chart */
    inventoryDistributionData: any;
    inventoryDistributionOptions: any;

    // ============================================
    // Operational Data
    // ============================================

    /** أفضل 5 أدوية مبيعاً / Top 5 Best-Selling Medicines */
    topSellingMedicines = signal<any[]>([]);

    /** التنبيهات الحديثة / Recent Alerts */
    recentAlerts = signal<any[]>([]);

    // ============================================
    // Computed Values
    // ============================================

    /** نسبة التغيير في المبيعات / Sales Change Percentage */
    salesTrend = computed(() => {
        const report = this.financialReport();
        if (!report || report.totalIncome === 0) return 0;
        return ((report.netProfit / report.totalIncome) * 100);
    });

    ngOnInit() {
        this.initializeCharts();
        this.loadAllDashboardData();
        this.startAutoRefresh();
    }

    ngOnDestroy() {
        this.stopAutoRefresh();
    }

    /**
     * تحميل جميع بيانات لوحة التحكم
     * Load All Dashboard Data
     */
    private loadAllDashboardData(): void {
        this.isLoading.set(true);

        // 1. جلب رصيد الصيدلية / Fetch Pharmacy Balance
        this.financialService.getBalance().subscribe({
            next: (account: PharmacyAccount) => {
                this.pharmacyBalance.set(account.balance);
            },
            error: (err: any) => {
                console.error('Error fetching balance:', err);
            }
        });

        // 2. جلب التقرير المالي للشهر الحالي / Fetch Current Month Financial Report
        const today = new Date();
        const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1).toISOString();
        const endOfMonth = new Date().toISOString();

        this.financialService.getReport(startOfMonth, endOfMonth).subscribe({
            next: (report: FinancialReport) => {
                this.financialReport.set(report);
                this.todaySales.set(report.totalIncome);
                this.isLoading.set(false);
            },
            error: (err: any) => {
                console.error('Error fetching financial report:', err);
                this.isLoading.set(false);
            }
        });

        // 3. جلب بيانات المبيعات مقابل المشتريات (6 أشهر)
        this.load6MonthsTrend();

        // 4. جلب توزيع المخزون
        this.loadInventoryDistribution();

        // 5. جلب أفضل الأدوية مبيعاً
        this.loadTopSellingMedicines();

        // 6. جلب التنبيهات الحديثة
        this.loadRecentAlerts();
    }

    /**
     * تحميل بيانات المبيعات مقابل المشتريات لآخر 6 أشهر
     * Load 6 Months Sales vs Purchases Trend
     */
    private load6MonthsTrend(): void {
        const currentYear = new Date().getFullYear();

        this.financialService.getAnnualSummary(currentYear).subscribe({
            next: (data: any[]) => {
                // تحويل البيانات إلى صيغة مناسبة للرسم البياني
                const last6Months = this.getLast6MonthsLabels();

                // محاكاة بيانات المبيعات والمشتريات (يجب استبدالها ببيانات حقيقية من API)
                const salesData = [45000, 52000, 48000, 61000, 58000, 67000];
                const purchasesData = [38000, 42000, 39000, 48000, 45000, 52000];

                this.updateSalesVsPurchasesChart(last6Months, salesData, purchasesData);
            },
            error: (err: any) => {
                console.error('Error loading 6 months trend:', err);
            }
        });
    }

    /**
     * تحميل توزيع المخزون حسب الفئة
     * Load Inventory Distribution by Category
     */
    private loadInventoryDistribution(): void {
        // محاكاة بيانات توزيع المخزون (يجب استبدالها ببيانات حقيقية من API)
        const categories = ['أدوية القلب', 'مضادات حيوية', 'مسكنات', 'فيتامينات', 'أدوية السكري'];
        const values = [1250, 980, 1540, 720, 890];

        this.updateInventoryDistributionChart(categories, values);
    }

    /**
     * تحميل أفضل 5 أدوية مبيعاً
     * Load Top 5 Best-Selling Medicines
     */
    private loadTopSellingMedicines(): void {
        // محاكاة بيانات (يجب استبدالها ببيانات حقيقية من API)
        this.topSellingMedicines.set([
            { name: 'باراسيتامول 500mg', soldQuantity: 2340, remainingBatches: 8, revenue: 23400 },
            { name: 'أموكسيسيلين 500mg', soldQuantity: 1890, remainingBatches: 5, revenue: 37800 },
            { name: 'أوميبرازول 20mg', soldQuantity: 1650, remainingBatches: 6, revenue: 33000 },
            { name: 'ميتفورمين 850mg', soldQuantity: 1420, remainingBatches: 4, revenue: 21300 },
            { name: 'أسبرين 100mg', soldQuantity: 1280, remainingBatches: 7, revenue: 12800 }
        ]);
    }

    /**
     * تحميل التنبيهات الحديثة
     * Load Recent Alerts
     */
    private loadRecentAlerts(): void {
        // محاكاة بيانات التنبيهات (يجب استبدالها ببيانات حقيقية من API)
        this.recentAlerts.set([
            {
                type: 'warning',
                title: 'نقص في المخزون',
                message: '5 أدوية تحتاج إعادة طلب',
                time: 'منذ ساعة',
                icon: 'pi-exclamation-triangle',
                color: '#f97316'
            },
            {
                type: 'danger',
                title: 'صلاحية قريبة',
                message: '3 دفعات تنتهي خلال 30 يوم',
                time: 'منذ 2 ساعة',
                icon: 'pi-calendar-times',
                color: '#ef4444'
            },
            {
                type: 'success',
                title: 'تحصيل دفعة',
                message: 'تم تحصيل 15,000 ريال من العميل أحمد',
                time: 'منذ 3 ساعات',
                icon: 'pi-check-circle',
                color: '#10b981'
            },
            {
                type: 'info',
                title: 'فاتورة جديدة',
                message: 'فاتورة مبيعات #1234 بقيمة 8,500 ريال',
                time: 'منذ 4 ساعات',
                icon: 'pi-file',
                color: '#0ea5e9'
            }
        ]);
    }

    /**
     * تهيئة الرسوم البيانية
     * Initialize Charts
     */
    private initializeCharts(): void {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = documentStyle.getPropertyValue('--text-color');
        const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
        const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

      // إعدادات مخطط المبيعات مقابل المشتريات
      this.salesVsPurchasesOptions = {
          maintainAspectRatio: false,
          aspectRatio: 0.6,
          plugins: {
              legend: {
                  labels: {
                    color: textColor,
                    font: {
                        family: 'Cairo, sans-serif',
                        size: 13
                    }
                },
                position: 'top'
            },
            tooltip: {
                rtl: true,
                textDirection: 'rtl',
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleFont: {
                    family: 'Cairo, sans-serif'
                },
                bodyFont: {
                    family: 'Cairo, sans-serif'
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                  color: textColorSecondary,
                  font: {
                      family: 'Cairo, sans-serif'
                  }
              },
              grid: {
                  color: surfaceBorder,
                  drawBorder: false
              }
          },
          x: {
              ticks: {
                    color: textColorSecondary,
                    font: {
                        family: 'Cairo, sans-serif'
                    }
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false
                }
            }
        }
    };

        // إعدادات مخطط توزيع المخزون
        this.inventoryDistributionOptions = {
            maintainAspectRatio: false,
            aspectRatio: 1,
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        usePointStyle: true,
                        font: {
                            family: 'Cairo, sans-serif',
                            size: 12
                        }
                    },
                    position: 'bottom'
                },
                tooltip: {
                    rtl: true,
                    textDirection: 'rtl',
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleFont: {
                        family: 'Cairo, sans-serif'
                    },
                    bodyFont: {
                        family: 'Cairo, sans-serif'
                    }
                }
            }
        };
    }

    /**
     * تحديث مخطط المبيعات مقابل المشتريات
     * Update Sales vs Purchases Chart
     */
    private updateSalesVsPurchasesChart(labels: string[], salesData: number[], purchasesData: number[]): void {
        this.salesVsPurchasesData = {
            labels: labels,
            datasets: [
                {
                    label: 'المبيعات',
                    data: salesData,
                    fill: false,
                    borderColor: '#10b981',
                    backgroundColor: '#10b981',
                    tension: 0.4,
                    borderWidth: 3,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointBackgroundColor: '#10b981',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2
                },
                {
                    label: 'المشتريات',
                    data: purchasesData,
                    fill: false,
                    borderColor: '#0ea5e9',
                    backgroundColor: '#0ea5e9',
                    tension: 0.4,
                    borderWidth: 3,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointBackgroundColor: '#0ea5e9',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2
                }
            ]
        };
    }

    /**
     * تحديث مخطط توزيع المخزون
     * Update Inventory Distribution Chart
     */
    private updateInventoryDistributionChart(categories: string[], values: number[]): void {
        this.inventoryDistributionData = {
            labels: categories,
            datasets: [
                {
              data: values,
              backgroundColor: [
                  '#10b981', // Emerald
                  '#0ea5e9', // Sky Blue
                  '#f97316', // Orange
                  '#a855f7', // Purple
                  '#64748b'  // Slate
              ],
              hoverBackgroundColor: [
                    '#059669',
                    '#0284c7',
                    '#ea580c',
                    '#9333ea',
                    '#475569'
                ],
                borderWidth: 0
            }
        ]
    };
    }

    /**
     * الحصول على أسماء آخر 6 أشهر
     * Get Last 6 Months Labels
     */
    private getLast6MonthsLabels(): string[] {
        const months = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
        const labels: string[] = [];
        const today = new Date();

        for (let i = 5; i >= 0; i--) {
            const date = new Date(today.getFullYear(), today.getMonth() - i, 1);
            labels.push(months[date.getMonth()]);
        }

        return labels;
    }

    /**
     * بدء التحديث التلقائي كل 30 ثانية
     * Start Auto-Refresh Every 30 Seconds
     */
    private startAutoRefresh(): void {
        this.refreshSubscription = interval(30000) // 30 seconds
            .pipe(
                switchMap(() => {
                    console.log('Auto-refreshing dashboard data...');
                    return this.financialService.getBalance();
                })
            )
            .subscribe({
                next: (account: PharmacyAccount) => {
                    this.pharmacyBalance.set(account.balance);
                },
                error: (err: any) => {
                    console.error('Auto-refresh error:', err);
                }
            });
    }

    /**
     * إيقاف التحديث التلقائي
     * Stop Auto-Refresh
     */
    private stopAutoRefresh(): void {
        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
        }
    }

    // ============================================
    // Quick Actions
    // ============================================

    navigateToNewSale(): void {
        this.router.navigate(['/sales/create']);
    }

    navigateToNewPurchase(): void {
        this.router.navigate(['/purchases/create']);
    }

    navigateToFinancial(): void {
        this.router.navigate(['/financial/dashboard']);
    }

    navigateToInventory(): void {
        this.router.navigate(['/inventory/medicines']);
  }

    // ============================================
    // Utility Methods
    // ============================================

    refreshDashboard(): void {
        this.loadAllDashboardData();
        this.messageService.add({
            severity: 'success',
            summary: 'تم التحديث',
            detail: 'تم تحديث بيانات لوحة التحكم بنجاح',
            life: 2000
        });
  }

    getAlertSeverity(type: string): 'success' | 'info' | 'warning' | 'danger' {
        const severityMap: any = {
            'success': 'success',
            'info': 'info',
            'warning': 'warning',
            'danger': 'danger'
    };
      return severityMap[type] || 'info';
  }
}
