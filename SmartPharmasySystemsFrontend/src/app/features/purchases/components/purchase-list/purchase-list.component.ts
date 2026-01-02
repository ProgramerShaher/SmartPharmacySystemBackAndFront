import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { PurchaseInvoice } from '../../../../core/models';

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
        ToastModule
    ],
    templateUrl: './purchase-list.component.html',
    styleUrls: ['./purchase-list.component.scss'],
    providers: [ConfirmationService, MessageService]
})
export class PurchaseInvoiceListComponent implements OnInit {
    invoices = signal<PurchaseInvoice[]>([]);
    loading = signal(false);
    showFilters = signal(false);

    // KPI Stats
    totalPurchases = signal(0);
    supplierDebts = signal(0);
    returnRate = signal(0);
    monthlyPurchases = signal(0);

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
        // Supplier Distribution Donut Chart
        this.supplierDistributionData = {
            labels: ['مورد أ', 'مورد ب', 'مورد ج', 'مورد د', 'آخرون'],
            datasets: [{
                data: [35, 25, 20, 12, 8],
                backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#6b7280'],
                hoverBackgroundColor: ['#059669', '#2563eb', '#d97706', '#7c3aed', '#4b5563']
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

        // Purchase Trend Sparkline
        this.purchaseTrendData = {
            labels: this.getLast7Days(),
            datasets: [{
                label: 'المشتريات اليومية',
                data: this.generateMockTrendData(7, 5000, 25000),
                borderColor: '#10b981',
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
        this.purchaseService.getAll().subscribe({
            next: (data) => {
              this.invoices.set(data);
              this.loading.set(false);
              this.calculateStats(data);
          },
          error: () => {
              this.messageService.add({
                  severity: 'error',
                  summary: 'خطأ',
                  detail: 'فشل في تحميل فواتير المشتريات'
              });
              this.loading.set(false);
          }
      });
    }

    loadKPIStats() {
        // Mock data - replace with real API calls
        this.totalPurchases.set(1250000);
        this.supplierDebts.set(350000);
        this.returnRate.set(2.5);
        this.monthlyPurchases.set(450000);
    }

    calculateStats(invoices: PurchaseInvoice[]) {
        const total = invoices
            .filter(inv => {
                const status = inv.status?.toString().toUpperCase();
                return status === 'APPROVED' || status === '1';
            })
            .reduce((sum, inv) => sum + (inv.totalAmount || 0), 0);
        this.totalPurchases.set(total);
    }

    toggleFilters() {
        this.showFilters.update(v => !v);
    }

    navigateToCreate() {
        this.router.navigate(['/purchases/create']);
    }

    viewDetails(id: number) {
        this.router.navigate(['/purchases', id]);
    }

    editInvoice(id: number) {
        this.router.navigate(['/purchases/edit', id]);
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

    isDraft(invoice: PurchaseInvoice): boolean {
        const statusNum = typeof invoice.status === 'number' ? invoice.status : parseInt((invoice.status as any)?.toString() || '0');
        return statusNum === 1; // Draft
    }

    isApproved(invoice: PurchaseInvoice): boolean {
        const statusNum = typeof invoice.status === 'number' ? invoice.status : parseInt((invoice.status as any)?.toString() || '0');
        return statusNum === 2; // Approved
    }

    isCancelled(invoice: PurchaseInvoice): boolean {
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
