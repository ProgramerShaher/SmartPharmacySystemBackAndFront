import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { DashboardStatsService } from '../../../../core/services/dashboard-stats.service';
import { PurchaseInvoice, DocumentStatus } from '../../../../core/models';
import * as xlsx from 'xlsx';
import * as FileSaver from 'file-saver';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToolbarModule } from 'primeng/toolbar';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { PurchaseInvoiceDetailsComponent } from '../purchase-details/purchase-details.component';

@Component({
    selector: 'app-purchase-invoice-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        ToolbarModule,
        ChartModule,
        DropdownModule,
        CalendarModule,
        ToastModule,
        PurchaseInvoiceDetailsComponent
    ],
    templateUrl: './purchase-list.component.html',
    styleUrls: ['./purchase-list.component.scss'],
    providers: [ConfirmationService, MessageService]
})
export class PurchaseInvoiceListComponent implements OnInit {
    invoices = signal<PurchaseInvoice[]>([]);
    loading = signal(false);
    showFilters = signal(false);
    
    showDetailsDialog = false;
    selectedInvoiceId: number | null = null;

    // KPI Stats (from backend)
    totalPurchases = signal(0);
    monthlyPurchases = signal(0);
    supplierDebts = signal(0);
    totalReturnsAmount = signal(0);
    returnRate = signal(0);
    overdueCount = signal(0);

    DocumentStatus = DocumentStatus;

    // Charts
    supplierDistributionData: any;
    supplierDistributionOptions: any;
    purchaseTrendData: any;
    purchaseTrendOptions: any;

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
        private purchaseService: PurchaseInvoiceService,
        private dashboardStatsService: DashboardStatsService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.initCharts();
    }

    ngOnInit() {
        this.loadInvoices();
        this.loadKPIStats();
    }

    initCharts() {
        // Charts initialized with empty/loading state, populated in calculateStats
        this.supplierDistributionData = {
            labels: [],
            datasets: [{
                data: [],
                backgroundColor: [getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#6b7280'],
                hoverBackgroundColor: [getComputedStyle(document.documentElement).getPropertyValue('--primary-600').trim() || '#059669', '#2563eb', '#d97706', '#7c3aed', '#4b5563']
            }]
        };

        this.supplierDistributionOptions = {
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
                    callbacks: {
                        label: (context: any) => {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            return `${label}: ${value} ر.ي`;
                        }
                    }
                }
            },
            cutout: '65%'
        };

        this.purchaseTrendData = {
            labels: this.getLast7Days(),
            datasets: [{
                label: 'المشتريات اليومية',
                data: Array(7).fill(0),
                borderColor: getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 3,
                pointHoverRadius: 5
            }]
        };

        this.purchaseTrendOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { display: false, beginAtZero: true },
                x: { display: false }
            }
        };
    }

    loadInvoices() {
        this.loading.set(true);
        this.purchaseService.getAll().subscribe({
            next: (data: any) => {
                this.invoices.set(data);
                this.loading.set(false);
                // Stats are now loaded separately with full dataset
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل البيانات' });
                this.loading.set(false);
            }
        });
    }

    /**
     * Load KPI stats from dedicated backend endpoint
     * Single optimized call instead of 3 parallel frontend calculations
     */
    loadKPIStats() {
        this.dashboardStatsService.getPurchasesDashboardStats().subscribe({
            next: (stats) => {
                this.monthlyPurchases.set(stats.monthlyTotalPurchases);
                this.totalPurchases.set(stats.monthlyTotalPurchases); // Use monthly for KPI display
                this.supplierDebts.set(stats.supplierDebts);
                this.totalReturnsAmount.set(stats.monthlyReturnsAmount);
                this.returnRate.set(stats.returnRate);
                this.overdueCount.set(stats.overdueCount);

                // Update sparkline chart
                this.purchaseTrendData = {
                    ...this.purchaseTrendData,
                    datasets: [{
                        ...this.purchaseTrendData.datasets[0],
                        data: stats.last7DaysPurchases
                    }]
                };

                // Update supplier distribution donut chart
                const distLabels = stats.supplierDistribution.map(s => s.supplierName);
                const distData = stats.supplierDistribution.map(s => s.totalAmount);

                this.supplierDistributionData = {
                    labels: distLabels,
                    datasets: [{
                        ...this.supplierDistributionData.datasets[0],
                        data: distData
                    }]
                };
            },
            error: (e) => {
                console.error('Failed to load dashboard stats:', e);
            }
        });
    }

    toggleFilters() {
        this.showFilters.update(v => !v);
    }

    navigateToCreate() {
        this.router.navigate(['/purchases/create']);
    }

    viewDetails(id: number) {
        this.selectedInvoiceId = id;
        this.showDetailsDialog = true;
    }

    editInvoice(id: number) {
        this.router.navigate(['/purchases/edit', id]);
    }

    navigateToReturn(id: number) {
        this.router.navigate(['/purchases/returns/create'], { queryParams: { invoiceId: id } });
    }

    approveInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد التوريد؟ سيتم إضافة الأصناف للمخزون.',
            header: 'تأكيد التوريد',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، توريد',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-success',
            accept: () => {
                this.purchaseService.approve(id).subscribe({
                    next: () => {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'نجاح',
                        detail: 'تم اعتماد التوريد بنجاح'
                    });
                        this.loadInvoices();
                        this.loadKPIStats();
                },
                error: (err) => this.handleError(err)
            });
            }
        });
    }

    cancelInvoice(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء فاتورة التوريد؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptButtonStyleClass: 'p-button-danger',
            acceptLabel: 'نعم، إلغاء',
            rejectLabel: 'تراجع',
            accept: () => {
                this.purchaseService.cancel(id).subscribe({
                    next: () => {
                    this.messageService.add({
                        severity: 'info',
                        summary: 'تم الإلغاء',
                        detail: 'تم إلغاء الفاتورة'
                    });
                        this.loadInvoices();
                        this.loadKPIStats();
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
                this.purchaseService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'تم الحذف',
                            detail: 'تم حذف المسودة بنجاح'
                        });
                            this.loadInvoices();
                        this.loadKPIStats();
                    },
                    error: (err) => this.handleError(err)
                });
            }
        });
    }

    printInvoice(id: number) {
        this.messageService.add({
            severity: 'info',
            summary: 'طباعة',
            detail: 'جاري تجهيز الفاتورة للطباعة...'
        });
        // Implement print logic
    }

    exportPDF() {
        const printWindow = window.open('', '_blank');
        if (!printWindow) {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى السماح بالنوافذ المنبثقة (Popups)' });
            return;
        }

        const rows = this.invoices().map((inv, index) => `
            <tr>
                <td style="text-align: center;">${index + 1}</td>
                <td style="text-align: center;">${this.escapeHtml(inv.purchaseInvoiceNumber || '#' + inv.id)}</td>
                <td style="text-align: center;">${new Date(inv.purchaseDate).toLocaleDateString('ar-EG')}</td>
                <td>${this.escapeHtml(inv.supplierName || 'غير محدد')}</td>
                <td style="text-align: left; direction: ltr;">${(inv.totalAmount || 0).toLocaleString('en-US', {minimumFractionDigits: 2})}</td>
                <td style="text-align: center;">${this.getStatusLabel(inv.status)}</td>
            </tr>
        `).join('');

        const html = `
            <html lang="ar" dir="rtl">
            <head>
                <title>تقرير فواتير المشتريات</title>
                <style>
                    body { font-family: 'Cairo', Arial, sans-serif; padding: 20px; }
                    h1 { text-align: center; color: #111827; }
                    .report-date { text-align: center; color: #6b7280; margin-bottom: 20px; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th, td { border: 1px solid #d1d5db; padding: 8px 12px; }
                    th { background-color: #f3f4f6; color: #374151; font-weight: bold; }
                    @media print {
                        @page { size: A4; margin: 1cm; }
                        button { display: none; }
                    }
                </style>
            </head>
            <body>
                <h1>تقرير فواتير المشتريات</h1>
                <div class="report-date">تاريخ الإصدار: ${new Date().toLocaleString('ar-EG')}</div>
                <table>
                    <thead>
                        <tr>
                            <th>#</th>
                            <th>رقم الفاتورة</th>
                            <th>التاريخ</th>
                            <th>المورد</th>
                            <th>الإجمالي (ر.ي)</th>
                            <th>الحالة</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${rows || '<tr><td colspan="6" style="text-align:center;">لا توجد فواتير</td></tr>'}
                    </tbody>
                </table>
                <script>
                    window.onload = function() {
                        setTimeout(() => {
                            window.print();
                            window.close();
                        }, 500);
                    };
                </script>
            </body>
            </html>
        `;

        printWindow.document.open();
        printWindow.document.write(html);
        printWindow.document.close();
        
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تجهيز تقرير PDF للطباعة' });
    }

    exportExcel() {
        if (!this.invoices() || this.invoices().length === 0) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا توجد بيانات للتصدير' });
            return;
        }

        const exportData = this.invoices().map(inv => ({
            'رقم الفاتورة': inv.purchaseInvoiceNumber || '#' + inv.id,
            'التاريخ': new Date(inv.purchaseDate).toLocaleDateString('ar-EG'),
            'المورد': inv.supplierName || 'غير محدد',
            'الإجمالي (ر.ي)': inv.totalAmount || 0,
            'الحالة': this.getStatusLabel(inv.status)
        }));

        const worksheet = xlsx.utils.json_to_sheet(exportData);
        
        worksheet['!cols'] = [
            { wch: 15 }, // Invoice Number
            { wch: 15 }, // Date
            { wch: 25 }, // Supplier
            { wch: 15 }, // Total
            { wch: 15 }  // Status
        ];

        const workbook = { 
            Sheets: { 'Purchases': worksheet }, 
            SheetNames: ['Purchases'],
            Workbook: { Views: [{ RTL: true }] }
        };
        
        const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
        const EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
        const data: Blob = new Blob([excelBuffer], { type: EXCEL_TYPE });
        
        FileSaver.saveAs(data, `Purchase_Invoices_${new Date().getTime()}.xlsx`);
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تصدير الفواتير إلى Excel' });
    }

    private escapeHtml(value: string): string {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    handleError(err: any) {
        const detailMsg = err.error?.message || err.error?.Message || err.error?.detail || err.message || 'فشل الاتصال بالخادم';
        this.messageService.add({
            severity: 'error',
            summary: 'فشل العملية',
            detail: detailMsg
        });
    }

    getStatusSeverity(status: any): 'success' | 'warning' | 'danger' | 'info' {
        if (status === undefined || status === null) return 'info';
        // Handle both string and number
        if (status === 'Approved' || status === 2) return 'success';
        if (status === 'Draft' || status === 1) return 'warning';
        if (status === 'Cancelled' || status === 3) return 'danger';
        return 'info';
    }

    getStatusLabel(status: any): string {
        if (status === undefined || status === null) return 'غير محدد';

        if (status === 'Approved' || status === 2) return 'معتمدة';
        if (status === 'Draft' || status === 1) return 'مسودة';
        if (status === 'Cancelled' || status === 3) return 'ملغاة';

        return 'غير محدد';
    }

    isDraft(invoice: PurchaseInvoice): boolean {
        return (invoice.status as any) === 'Draft' || invoice.status === DocumentStatus.Draft || Number(invoice.status) === 1;
    }

    isApproved(invoice: PurchaseInvoice): boolean {
        return (invoice.status as any) === 'Approved' || invoice.status === DocumentStatus.Approved || Number(invoice.status) === 2;
    }

    isCancelled(invoice: PurchaseInvoice): boolean {
        return (invoice.status as any) === 'Cancelled' || invoice.status === DocumentStatus.Cancelled || Number(invoice.status) === 3;
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
}
