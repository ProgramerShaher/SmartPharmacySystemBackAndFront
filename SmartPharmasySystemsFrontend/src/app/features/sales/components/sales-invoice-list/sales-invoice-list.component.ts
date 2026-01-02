import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SaleInvoiceService } from '../../services/sales-invoice.service';
import { SalesStatisticsService } from '../../services/sales-statistics.service';
import { SaleInvoice, DocumentStatus } from '../../../../core/models';
import { PrintService } from '../../../../core/services/print.service';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-sales-invoice-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        CardModule,
        ToolbarModule,
        ChartModule,
        DropdownModule,
        CalendarModule,
        ToastModule
    ],
    templateUrl: './sales-invoice-list.component.html',
    styleUrls: ['./sales-invoice-list.component.scss'],
    providers: [ConfirmationService, MessageService]
})
export class SalesInvoiceListComponent implements OnInit {
    invoices = signal<SaleInvoice[]>([]);
    loading = signal(false);
    showFilters = signal(false);

    DocumentStatus = DocumentStatus;

    // KPI Stats - Live Data من الباك إند
    totalSales = signal(0);
    totalProfit = signal(0);
    customerDebts = signal(0);
    totalReturns = signal(0);
    cashPercentage = signal(0);
    creditPercentage = signal(0);
    cashAmount = signal(0);
    creditAmount = signal(0);

    // Charts Data - Live من الباك إند
    customerDistributionData: any;
    customerDistributionOptions: any;
    salesTrendData: any;
    salesTrendOptions: any;

    // Filters
    statusFilter: string | null = null;
    startDate: Date | null = null;
    endDate: Date | null = null;

    statusOptions = [
        { label: 'الكل', value: null },
        { label: 'مسودة', value: 'Draft' },
        { label: 'معتمدة', value: 'Approved' },
        { label: 'ملغاة', value: 'Cancelled' }
    ];

    constructor(
        private salesService: SaleInvoiceService,
        private statsService: SalesStatisticsService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private printService: PrintService
    ) {
        this.initCharts();
    }

    ngOnInit() {
        this.loadInvoices();
        this.loadLiveKPIStats();
        this.loadLiveSalesFlow();
        this.loadLivePaymentDistribution();
    }

    initCharts() {
        // Customer Distribution Donut Chart
        this.customerDistributionData = {
            labels: ['نقدي', 'آجل', 'بطاقة', 'تأمين', 'أخرى'],
            datasets: [{
                data: [45, 25, 15, 10, 5],
                backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#6b7280'],
                hoverBackgroundColor: ['#059669', '#2563eb', '#d97706', '#7c3aed', '#4b5563']
            }]
        };

        this.customerDistributionOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: { family: 'Cairo, sans-serif', size: 11 },
                        padding: 10,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleFont: { family: 'Cairo, sans-serif' },
                    bodyFont: { family: 'Cairo, sans-serif' },
                    padding: 10,
                    cornerRadius: 8,
                    callbacks: {
                        label: (context: any) => {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            return `${label}: ${value}%`;
                        }
                    }
                }
            },
            cutout: '65%'
        };

        // Sales Trend Sparkline
        this.salesTrendData = {
            labels: this.getLast7Days(),
            datasets: [{
                label: 'المبيعات اليومية',
                data: this.generateMockTrendData(7, 8000, 35000),
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 3,
                pointHoverRadius: 5
            }]
        };

        this.salesTrendOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleFont: { family: 'Cairo, sans-serif' },
                    bodyFont: { family: 'Cairo, sans-serif' },
                    padding: 10,
                    cornerRadius: 8
                }
            },
            scales: {
                y: {
                    display: false,
                    beginAtZero: true
                },
                x: {
                    display: false
                }
            }
        };
    }

    loadInvoices() {
        this.loading.set(true);
        this.salesService.getAll().subscribe({
            next: (data) => {
                this.invoices.set(data);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل في تحميل فواتير المبيعات'
                });
                this.loading.set(false);
            }
        });
    }

    /**
     * 📊 Load Live KPI Statistics
     * تحميل مؤشرات الأداء الحية من الباك إند
     */
    loadLiveKPIStats() {
        this.statsService.getTodayKPIs().subscribe({
            next: (data) => {
                this.totalSales.set(data.totalSalesToday);
                this.totalProfit.set(data.totalProfitToday);
                this.customerDebts.set(data.totalDebts);
                this.totalReturns.set(data.totalReturnsToday);
                this.cashPercentage.set(data.cashSalesPercentage);
                this.creditPercentage.set(data.creditSalesPercentage);
                this.cashAmount.set(data.cashSalesAmount);
                this.creditAmount.set(data.creditSalesAmount);
            },
            error: (err) => {
                console.error('Failed to load KPI stats:', err);
                // Fallback to zero values - no mock data
            }
        });
    }

    /**
     * 📈 Load Live Sales Flow (Last 7 Days)
     * تحميل تدفق المبيعات الحي لآخر 7 أيام
     */
    loadLiveSalesFlow() {
        this.statsService.getSalesFlow(7).subscribe({
            next: (data) => {
                const labels = data.map(d => {
                    const date = new Date(d.date);
                    return date.toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' });
                });
                const salesData = data.map(d => d.sales);

                this.salesTrendData = {
                    labels: labels,
                    datasets: [{
                        label: 'المبيعات اليومية',
                        data: salesData,
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        tension: 0.4,
                        fill: true,
                        pointRadius: 3,
                        pointHoverRadius: 5,
                        pointBackgroundColor: '#28a745',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2
                    }]
                };
            },
            error: (err) => {
                console.error('Failed to load sales flow:', err);
            }
        });
    }

    /**
     * 🥧 Load Live Payment Distribution
     * تحميل توزيع طرق الدفع الحي
     */
    loadLivePaymentDistribution() {
        this.statsService.getPaymentDistribution().subscribe({
            next: (data) => {
                const labels = data.map(d => d.method);
                const amounts = data.map(d => d.percentage);
                const colors = this.getPaymentMethodColors(data.length);

                this.customerDistributionData = {
                    labels: labels,
                    datasets: [{
                        data: amounts,
                        backgroundColor: colors.bg,
                        hoverBackgroundColor: colors.hover,
                        borderWidth: 0
                    }]
                };
            },
            error: (err) => {
                console.error('Failed to load payment distribution:', err);
            }
        });
    }

    /**
     * 🎨 Get Payment Method Colors
     * الحصول على الألوان الفخمة لطرق الدفع
     */
    private getPaymentMethodColors(count: number) {
        const bgColors = ['#28a745', '#17a2b8', '#ffc107', '#6f42c1', '#6c757d'];
        const hoverColors = ['#218838', '#138496', '#e0a800', '#5a32a3', '#545b62'];

        return {
            bg: bgColors.slice(0, count),
            hover: hoverColors.slice(0, count)
        };
    }

    toggleFilters() {
        this.showFilters.update(v => !v);
    }

    navigateToCreate() {
        this.router.navigate(['/sales/create']);
    }

    viewDetails(id: number) {
        this.router.navigate(['/sales', id]);
    }

    editInvoice(id: number) {
        this.router.navigate(['/sales/edit', id]);
    }

    navigateToReturn(invoiceId: number) {
        this.router.navigate(['/sales/returns/create'], {
            queryParams: { invoiceId }
        });
    }

    approveInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد الفاتورة؟ سيتم خصم الأصناف من المخزون.',
            header: 'تأكيد البيع',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتماد',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-success',
            accept: () => {
                this.salesService.approve(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'نجاح',
                            detail: 'تم اعتماد الفاتورة بنجاح'
                        });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    cancelInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء فاتورة البيع؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، إلغاء',
            rejectLabel: 'تراجع',
            accept: () => {
                this.salesService.cancel(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'info',
                            summary: 'تم الإلغاء',
                            detail: 'تم إلغاء الفاتورة'
                        });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    deleteDraft(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف هذه المسودة نهائياً؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، حذف',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.salesService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'تم الحذف',
                            detail: 'تم حذف المسودة بنجاح'
                        });
                        this.loadInvoices();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    printInvoice(id: number) {
        this.printService.printInvoice(id);
    }

    exportPDF() {
        this.messageService.add({
            severity: 'info',
            summary: 'تصدير PDF',
            detail: 'جاري تجهيز الملف...'
        });
    }

    exportExcel() {
        this.messageService.add({
            severity: 'info',
            summary: 'تصدير Excel',
            detail: 'جاري تجهيز الملف...'
        });
    }

    handleError(err: any) {
        this.messageService.add({
            severity: 'error',
            summary: 'فشل العملية',
            detail: err.error?.message || 'فشل الاتصال بالخادم'
        });
    }

    getStatusSeverity(status: any): 'success' | 'warning' | 'danger' | 'info' {
        if (!status) return 'info';
        const statusNum = typeof status === 'number' ? status : parseInt(status.toString());

        switch (statusNum) {
            case 2: // Approved
                return 'success';
            case 1: // Draft
                return 'warning';
            case 3: // Cancelled
                return 'danger';
            default:
                return 'info';
        }
    }

    getStatusLabel(status: any): string {
        if (!status) return 'غير محدد';
        const statusNum = typeof status === 'number' ? status : parseInt(status.toString());

        switch (statusNum) {
            case 2: // Approved
                return 'معتمدة';
            case 1: // Draft
                return 'مسودة';
            case 3: // Cancelled
                return 'ملغاة';
            default:
                return 'غير محدد';
        }
    }

    isDraft(invoice: SaleInvoice): boolean {
        const statusNum = typeof invoice.status === 'number' ? invoice.status : parseInt((invoice.status as any)?.toString() || '0');
        return statusNum === 1; // Draft
    }

    isApproved(invoice: SaleInvoice): boolean {
        const statusNum = typeof invoice.status === 'number' ? invoice.status : parseInt((invoice.status as any)?.toString() || '0');
        return statusNum === 2; // Approved
    }

    isCancelled(invoice: SaleInvoice): boolean {
        const statusNum = typeof invoice.status === 'number' ? invoice.status : parseInt((invoice.status as any)?.toString() || '0');
        return statusNum === 3; // Cancelled
    }

    // Helper methods
    private getLast7Days(): string[] {
        const days = [];
        for (let i = 6; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            days.push(date.toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' }));
        }
        return days;
    }

    private generateMockTrendData(count: number, min: number, max: number): number[] {
        return Array.from({ length: count }, () =>
            Math.floor(Math.random() * (max - min + 1)) + min
        );
    }
}
