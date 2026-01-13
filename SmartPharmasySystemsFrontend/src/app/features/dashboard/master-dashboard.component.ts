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
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';

// Services & Models
import { MasterDashboardService } from '../../core/services/master-dashboard.service';
import {
    MasterDashboardStats,
    FinancialIntelligence,
    InventoryIntelligence,
    OperationalPulse
} from '../../core/models/master-dashboard.models';

/**
 * Master Dashboard Component - Single Source of Truth
 * 
 * Features:
 * - Real-time KPIs with Angular Signals
 * - Financial Intelligence Hub (Net Profit, Liquidity, Net Debt)
 * - Inventory Intelligence (Value, Expiry Radar, Critical Stock)
 * - Operational Pulse (Activity Stream, Performance, Heat Map)
 * - Auto-refresh every 30 seconds
 * - Drill-down navigation
 * - Full RTL support
 * - Dark/Light mode compatible
 * - Compact UI (single viewport, no scrolling)
 */
@Component({
    selector: 'app-master-dashboard',
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
        ProgressBarModule
    ],
    providers: [MessageService],
    templateUrl: './master-dashboard.component.html',
    styleUrl: './master-dashboard.component.css'
})
export class MasterDashboardComponent implements OnInit, OnDestroy {
    private dashboardService = inject(MasterDashboardService);
    private router = inject(Router);
    private messageService = inject(MessageService);

    private refreshSubscription?: Subscription;

    // ============================================
    // Signals for Reactive State Management
    // ============================================

    /** حالة التحميل */
    isLoading = signal<boolean>(true);

    /** البيانات الكاملة */
    dashboardData = signal<MasterDashboardStats | null>(null);

    /** الذكاء المالي */
    financialData = computed(() => this.dashboardData()?.financialIntelligence);

    /** الذكاء المخزني */
    inventoryData = computed(() => this.dashboardData()?.inventoryIntelligence);

    /** النبض التشغيلي */
    operationalData = computed(() => this.dashboardData()?.operationalPulse);

    // ============================================
    // Chart Data
    // ============================================

    /** مخطط التدفقات النقدية (30 يوم) */
    cashFlowChartData: any;
    cashFlowChartOptions: any;

    /** مخطط توزيع المخزون */
    inventoryDistributionData: any;
    inventoryDistributionOptions: any;

    /** مخطط أداء الكاشيرات */
    cashierPerformanceData: any;
    cashierPerformanceOptions: any;

    /** مخطط الخريطة الحرارية */
    heatMapChartData: any;
    heatMapChartOptions: any;

    ngOnInit(): void {
        this.initializeCharts();
        this.loadDashboardData();
        this.startAutoRefresh();
    }

    ngOnDestroy(): void {
        this.stopAutoRefresh();
    }

    /**
     * تحميل بيانات لوحة التحكم
     */
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

    /**
     * تهيئة المخططات البيانية مع تأثيرات premium
     */
    private initializeCharts(): void {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = 'rgba(30, 27, 75, 0.8)';
        const textColorSecondary = 'rgba(30, 27, 75, 0.6)';
        const surfaceBorder = 'rgba(102, 126, 234, 0.1)';

        // 📊 مخطط التدفقات النقدية - Area Chart with Gradient
        this.cashFlowChartOptions = {
            maintainAspectRatio: false,
            aspectRatio: 0.6,
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        font: { family: 'Cairo, sans-serif', size: 12, weight: '600' },
                        usePointStyle: true,
                        padding: 15
                    },
                    position: 'top'
                },
                tooltip: {
                    rtl: true,
                    textDirection: 'rtl',
                    backgroundColor: 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColorSecondary,
                    borderColor: surfaceBorder,
                    borderWidth: 1,
                    titleFont: { family: 'Cairo, sans-serif', size: 14, weight: 'bold' },
                    bodyFont: { family: 'Cairo, sans-serif', size: 12 },
                    padding: 12,
                    boxPadding: 6,
                    cornerRadius: 8
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: textColorSecondary,
                        font: { family: 'Cairo, sans-serif', size: 11 }
                    },
                    grid: {
                        color: surfaceBorder,
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        color: textColorSecondary,
                        font: { family: 'Cairo, sans-serif', size: 11 }
                    },
                    grid: {
                        display: false
                    }
                }
            },
            interaction: {
                mode: 'index',
                intersect: false
            }
        };

        // 🍩 مخطط توزيع المخزون - Animated Donut with Center Label
        this.inventoryDistributionOptions = {
            maintainAspectRatio: false,
            aspectRatio: 1,
            cutout: '70%',
            plugins: {
                legend: {
                    labels: {
                        color: textColor,
                        usePointStyle: true,
                        font: { family: 'Cairo, sans-serif', size: 11, weight: '600' },
                        padding: 12
                    },
                    position: 'bottom'
                },
                tooltip: {
                    rtl: true,
                    textDirection: 'rtl',
                    backgroundColor: 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColorSecondary,
                    borderColor: surfaceBorder,
                    borderWidth: 1,
                    titleFont: { family: 'Cairo, sans-serif', size: 14, weight: 'bold' },
                    bodyFont: { family: 'Cairo, sans-serif', size: 12 },
                    padding: 12,
                    boxPadding: 6,
                    cornerRadius: 8
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1500,
                easing: 'easeInOutQuart'
            }
        };

        // 📊 مخطط أداء الكاشيرات - Gradient Horizontal Bar
        this.cashierPerformanceOptions = {
            indexAxis: 'y',
            maintainAspectRatio: false,
            aspectRatio: 0.8,
            plugins: {
                legend: { display: false },
                tooltip: {
                    rtl: true,
                    textDirection: 'rtl',
                    backgroundColor: 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColorSecondary,
                    borderColor: surfaceBorder,
                    borderWidth: 1,
                    titleFont: { family: 'Cairo, sans-serif', size: 14, weight: 'bold' },
                    bodyFont: { family: 'Cairo, sans-serif', size: 12 },
                    padding: 12,
                    cornerRadius: 8
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: {
                        color: textColorSecondary,
                        font: { family: 'Cairo, sans-serif', size: 11 }
                    },
                    grid: {
                        color: surfaceBorder,
                        drawBorder: false
                    }
                },
                y: {
                    ticks: {
                        color: textColor,
                        font: { family: 'Cairo, sans-serif', size: 11, weight: '600' }
                    },
                    grid: { display: false }
                }
            }
        };

        // 🔥 الخريطة الحرارية - Heat Map
        this.heatMapChartOptions = {
            maintainAspectRatio: false,
            aspectRatio: 0.8,
            plugins: {
                legend: { display: false },
                tooltip: {
                    rtl: true,
                    textDirection: 'rtl',
                    backgroundColor: 'rgba(255, 255, 255, 0.95)',
                    titleColor: textColor,
                    bodyColor: textColorSecondary,
                    borderColor: surfaceBorder,
                    borderWidth: 1,
                    titleFont: { family: 'Cairo, sans-serif', size: 14, weight: 'bold' },
                    bodyFont: { family: 'Cairo, sans-serif', size: 12 },
                    padding: 12,
                    cornerRadius: 8
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: textColorSecondary,
                        font: { family: 'Cairo, sans-serif', size: 11 }
                    },
                    grid: {
                        color: surfaceBorder,
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        color: textColor,
                        font: { family: 'Cairo, sans-serif', size: 11, weight: '600' }
                    },
                    grid: { display: false }
                }
            }
        };
    }

    /**
   * تحديث المخططات بالبيانات الجديدة مع تأثيرات gradient
   */
    private updateCharts(data: MasterDashboardStats): void {
        // تحديث مخطط التدفقات النقدية - Area Chart with Gradients
        if (data.financialIntelligence) {
            const labels = data.financialIntelligence.cashFlowInLast30Days.map(cf =>
                new Date(cf.date).toLocaleDateString('ar-SA', { month: 'short', day: 'numeric' })
            );

            this.cashFlowChartData = {
                labels: labels,
                datasets: [
                    {
                        label: 'الداخل',
                        data: data.financialIntelligence.cashFlowInLast30Days.map(cf => cf.amount),
                        backgroundColor: 'rgba(16, 185, 129, 0.2)',
                        borderColor: '#10b981',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointBackgroundColor: '#10b981',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointHoverRadius: 6,
                        pointHoverBackgroundColor: '#059669',
                        pointHoverBorderWidth: 3,
                        shadowOffsetX: 0,
                        shadowOffsetY: 4,
                        shadowBlur: 10,
                        shadowColor: 'rgba(16, 185, 129, 0.3)'
                    },
                    {
                        label: 'الخارج',
                        data: data.financialIntelligence.cashFlowOutLast30Days.map(cf => cf.amount),
                        backgroundColor: 'rgba(239, 68, 68, 0.2)',
                        borderColor: '#ef4444',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 4,
                        pointBackgroundColor: '#ef4444',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointHoverRadius: 6,
                        pointHoverBackgroundColor: '#dc2626',
                        pointHoverBorderWidth: 3,
                        shadowOffsetX: 0,
                        shadowOffsetY: 4,
                        shadowBlur: 10,
                        shadowColor: 'rgba(239, 68, 68, 0.3)'
                    }
                ]
            };
        }

        // تحديث مخطط توزيع المخزون - Donut with Gradients
        if (data.inventoryIntelligence?.inventoryBySupplier) {
            const supplierData = data.inventoryIntelligence.inventoryBySupplier.slice(0, 5); // Top 5

            this.inventoryDistributionData = {
                labels: supplierData.map(s => s.supplierName),
                datasets: [{
                    data: supplierData.map(s => s.inventoryValue),
                    backgroundColor: [
                        '#10b981', // Green
                        '#3b82f6', // Blue
                        '#f59e0b', // Amber
                        '#a855f7', // Purple
                        '#64748b'  // Slate
                    ],
                    hoverBackgroundColor: [
                        '#059669',
                        '#1d4ed8',
                        '#d97706',
                        '#9333ea',
                        '#475569'
                    ],
                    borderWidth: 0,
                    hoverBorderWidth: 3,
                    hoverBorderColor: '#ffffff'
                }]
            };
        }

        // تحديث مخطط أداء الكاشيرات - Gradient Bar
        if (data.operationalPulse?.cashierPerformance) {
            const perfData = data.operationalPulse.cashierPerformance.slice(0, 5); // Top 5

            this.cashierPerformanceData = {
                labels: perfData.map(p => p.username),
                datasets: [{
                    label: 'المبيعات',
                    data: perfData.map(p => p.totalSales),
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                    borderColor: '#667eea',
                    borderWidth: 2,
                    borderRadius: 8,
                    hoverBackgroundColor: 'rgba(118, 75, 162, 0.9)',
                    hoverBorderColor: '#764ba2',
                    hoverBorderWidth: 3
                }]
            };
        }

        // تحديث الخريطة الحرارية - Heat Map
        if (data.operationalPulse?.hourlyHeatMap) {
            const heatMap = data.operationalPulse.hourlyHeatMap;

            this.heatMapChartData = {
                labels: heatMap.map(h => `${h.hour}:00`),
                datasets: [{
                    label: 'المبيعات',
                    data: heatMap.map(h => h.totalSales),
                    backgroundColor: heatMap.map(h => {
                        if (h.totalSales > 5000) return 'rgba(239, 68, 68, 0.8)'; // Red (hot)
                        if (h.totalSales > 2000) return 'rgba(249, 115, 22, 0.8)'; // Orange
                        if (h.totalSales > 500) return 'rgba(234, 179, 8, 0.8)';  // Yellow
                        return 'rgba(16, 185, 129, 0.8)'; // Green (cool)
                    }),
                    borderColor: heatMap.map(h => {
                        if (h.totalSales > 5000) return '#dc2626';
                        if (h.totalSales > 2000) return '#ea580c';
                        if (h.totalSales > 500) return '#ca8a04';
                        return '#059669';
                    }),
                    borderWidth: 2,
                    borderRadius: 8,
                    hoverBorderWidth: 3
                }]
            };
        }
    }

    /**
     * بدء التحديث التلقائي كل 30 ثانية
     */
    private startAutoRefresh(): void {
        this.refreshSubscription = interval(30000) // 30 seconds
            .pipe(
                switchMap(() => this.dashboardService.getMasterDashboardStats())
            )
            .subscribe({
                next: (data) => {
                    this.dashboardData.set(data);
                    this.updateCharts(data);
                    console.log('Dashboard auto-refreshed');
                },
                error: (err) => {
                    console.error('Auto-refresh error:', err);
                }
            });
    }

    /**
     * إيقاف التحديث التلقائي
     */
    private stopAutoRefresh(): void {
        if (this.refreshSubscription) {
            this.refreshSubscription.unsubscribe();
        }
    }

    // ============================================
    // Drill-Down Navigation
    // ============================================

    navigateToSalesReport(): void {
        this.router.navigate(['/reports/sales']);
    }

    navigateToInventoryReport(): void {
        this.router.navigate(['/inventory/medicines']);
    }

    navigateToFinancialReport(): void {
        this.router.navigate(['/financial/dashboard']);
    }

    navigateToDocument(operationType: string, referenceId: number): void {
        const routes: Record<string, string> = {
            'SaleInvoice': '/sales',
            'PurchaseInvoice': '/purchases',
            'SalesReturn': '/sales-returns',
            'Payment': '/financial/transactions'
        };

        const basePath = routes[operationType] || '/';
        this.router.navigate([`${basePath}/${referenceId}`]);
    }

    // ============================================
    // Utility Methods
    // ============================================

    refreshDashboard(): void {
        this.loadDashboardData();
        this.messageService.add({
            severity: 'success',
            summary: 'تم التحديث',
            detail: 'تم تحديث بيانات لوحة التحكم',
            life: 2000
        });
    }

    getActivityIcon(type: string): string {
        const icons: Record<string, string> = {
            'SaleInvoice': 'pi-shopping-cart',
            'PurchaseInvoice': 'pi-shopping-bag',
            'SalesReturn': 'pi-undo',
            'Payment': 'pi-money-bill'
        };
        return icons[type] || 'pi-file';
    }

    getActivityColor(type: string): string {
        const colors: Record<string, string> = {
            'SaleInvoice': '#10b981',
            'PurchaseInvoice': '#0ea5e9',
            'SalesReturn': '#f97316',
            'Payment': '#a855f7'
        };
        return colors[type] || '#64748b';
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat('ar-SA', {
            style: 'currency',
            currency: 'SAR',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(value);
    }

    formatTime(date: Date | string): string {
        return new Date(date).toLocaleTimeString('ar-SA', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}
