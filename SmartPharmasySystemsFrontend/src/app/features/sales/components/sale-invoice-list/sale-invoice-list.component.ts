import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SalesService } from '../../services/sales.service';
import { DashboardStatsService } from '../../../../core/services/dashboard-stats.service';
import { SaleInvoice, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ChartModule } from 'primeng/chart';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-sale-invoice-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ToolbarModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        ChartModule,
        ConfirmDialogModule,
        TooltipModule
    ],
    templateUrl: './sale-invoice-list.component.html',
    providers: [ConfirmationService]
})
export class SaleInvoiceListComponent implements OnInit {
    sales: SaleInvoice[] = [];
    loading = true;

    // Check if DocumentStatus is properly imported
    DocumentStatus = DocumentStatus;

    // KPI Stats (from backend)
    totalSales = signal(0);
    netProfit = signal(0);
    customersDebt = signal(0);
    totalReturnsAmount = signal(0);
    returnRate = signal(0);
    cashPercentage = signal(0);

    // Charts
    salesTrendData: any;
    salesTrendOptions: any;
    returnRateData: any;
    returnRateOptions: any;

    constructor(
        private salesService: SalesService,
        private dashboardStatsService: DashboardStatsService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.initCharts();
        this.loadSales();
        this.loadKPIStats();
    }

    initCharts() {
        this.salesTrendData = {
            labels: this.getLast7Days(),
            datasets: [{
                label: 'المبيعات اليومية',
                data: Array(7).fill(0),
                borderColor: '#10b981',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                fill: true
            }]
        };

        this.returnRateData = {
            labels: ['صافي المبيعات', 'المرتجعات'],
            datasets: [{
                data: [100, 0],
                backgroundColor: ['#3b82f6', '#ef4444']
            }]
        };
    }

    loadSales() {
        this.loading = true;
        this.salesService.getAll().subscribe({
            next: (data) => {
                this.sales = data;
                this.loading = false;
            },
            error: (e) => {
                console.error(e);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل فواتير المبيعات' });
                this.loading = false;
            }
        });
    }

    /**
     * Load KPI stats from dedicated backend endpoint
     * Single optimized call instead of 3 parallel frontend calculations
     */
    loadKPIStats() {
        console.log('[Sales KPI] Calling dashboard-stats API...');
        this.dashboardStatsService.getSalesDashboardStats().subscribe({
            next: (stats) => {
                console.log('[Sales KPI] API Response:', stats);
                this.totalSales.set(stats.todayTotalSales);
                this.netProfit.set(stats.todayNetProfit);
                this.customersDebt.set(stats.customerDebts);
                this.totalReturnsAmount.set(stats.todayReturnsAmount);
                this.returnRate.set(stats.returnRate);
                this.cashPercentage.set(stats.cashPercentage);

                // Update sparkline chart
                this.salesTrendData = {
                    ...this.salesTrendData,
                    datasets: [{
                        ...this.salesTrendData.datasets[0],
                        data: stats.last7DaysSales
                    }]
                };

                // Update return rate chart
                const netSales = stats.todayTotalSales - stats.todayReturnsAmount;
                this.returnRateData = {
                    labels: ['صافي المبيعات', 'المرتجعات'],
                    datasets: [{
                        data: [netSales, stats.todayReturnsAmount],
                        backgroundColor: ['#3b82f6', '#ef4444']
                    }]
                };
            },
            error: (e) => {
                console.error('[Sales KPI] API Error:', e);
            }
        });
    }

    private getLast7Days(): string[] {
        const days = [];
        for (let i = 6; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            days.push(date.toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' }));
        }
        return days;
    }

    createSale() {
        this.router.navigate(['/sales/invoices/create']);
    }

    viewSale(sale: SaleInvoice) {
        this.router.navigate(['/sales', sale.id]);
    }

    approveSale(sale: SaleInvoice) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من اعتماد الفاتورة رقم ${sale.saleInvoiceNumber || '#' + sale.id}؟ سيتم تحديث المخزون فوراً.`,
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.salesService.approve(sale.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الاعتماد', detail: 'تم اعتماد الفاتورة وتحديث المخزون بنجاح' });
                        this.loadSales();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في اعتماد الفاتورة' });
                    }
                });
            }
        });
    }

    cancelSale(sale: SaleInvoice) {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من إلغاء الفاتورة رقم ${sale.saleInvoiceNumber || '#' + sale.id}؟`,
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-exclamation-triangle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            accept: () => {
                this.salesService.cancel(sale.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء الفاتورة بنجاح' });
                        this.loadSales();
                    },
                    error: (err) => {
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في إلغاء الفاتورة' });
                    }
                });
            }
        });
    }

    // Helper for Severity - Fallback if frontend logic is preferred over backend color
    getStatusSeverity(status: number | string | DocumentStatus): 'success' | 'info' | 'warning' | 'danger' | undefined {
        // Backend sends Strings due to JsonStringEnumConverter, but Frontend Enum is Numbers.
        // We handle both to be safe.
        const statusStr = status?.toString();
        if (statusStr === 'Approved' || status === DocumentStatus.Approved) return 'success';
        if (statusStr === 'Draft' || status === DocumentStatus.Draft) return 'warning';
        if (statusStr === 'Cancelled' || status === DocumentStatus.Cancelled) return 'danger';
        if (statusStr === 'Returned' || status === DocumentStatus.Returned) return 'info';
        return 'info';
    }
}
