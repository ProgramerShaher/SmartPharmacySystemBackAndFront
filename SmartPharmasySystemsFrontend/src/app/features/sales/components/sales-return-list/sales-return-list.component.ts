import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SalesReturnService } from '../../services/sales-return.service';
import { SalesStatisticsService } from '../../services/sales-statistics.service'; // Import Statistics Service
import { SalesReturn, DocumentStatus } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ToolbarModule } from 'primeng/toolbar';
import { ChartModule } from 'primeng/chart'; // Import ChartModule

@Component({
    selector: 'app-sales-return-list',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        TagModule,
        TooltipModule,
        ConfirmDialogModule,
        CardModule,
        ToolbarModule,
        ChartModule
    ],
    templateUrl: './sales-return-list.component.html',
    styleUrls: ['./sales-return-list.component.scss'],
    providers: [ConfirmationService]
})
export class SalesReturnListComponent implements OnInit {
    returns: SalesReturn[] = [];
    loading = true;

    // KPI Signals
    totalReturnsValue = signal(0);
    returnsCount = signal(0);
    todayReturnsCount = signal(0);
    returnsPercentage = signal(0); // Added for the new KPI card

    // Charts
    returnsTrendData: any;
    returnsTrendOptions: any;
    returnsReasonData: any;
    returnsReasonOptions: any;

    DocumentStatus = DocumentStatus;

    constructor(
        private salesReturnService: SalesReturnService,
        private statsService: SalesStatisticsService,
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.initCharts();
    }

    ngOnInit() {
        this.loadReturns();
        this.loadStats();
    }

    loadReturns() {
        this.loading = true;
        this.salesReturnService.getAll().subscribe({
            next: (data) => {
                this.returns = data;
                this.loading = false;
                this.calculateLocalStats(data);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل المردودات' });
                this.loading = false;
            }
        });
    }

    loadStats() {
        // Load live trend data
        this.statsService.getSalesFlow(7).subscribe({
            next: (data) => {
                const labels = data.map(d => {
                    const date = new Date(d.date);
                    return date.toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' });
                });
                const returnsData = data.map(d => d.returns);

                this.returnsTrendData = {
                    labels: labels,
                    datasets: [{
                        label: 'المرتجعات اليومية',
                        data: returnsData,
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        tension: 0.4,
                        fill: true,
                        pointRadius: 3,
                        pointHoverRadius: 5
                    }]
                };
            }
        });
    }

    calculateLocalStats(data: SalesReturn[]) {
        this.returnsCount.set(data.length);
        const total = data.reduce((sum, r) => sum + r.totalAmount, 0);
        this.totalReturnsValue.set(total);
        this.returnsPercentage.set(5.2); // Setting a realistic mock percentage for now

        // Mock Reasons Distribution based on filtered Reasons (or just use status for now if reason not available)
        // Here assuming we want to show Status Distribution as "Returns Analysis"
        const drafts = data.filter(r => r.status === DocumentStatus.Draft).length;
        const approved = data.filter(r => r.status === DocumentStatus.Approved).length;

        this.returnsReasonData = {
            labels: ['معتمد', 'مسودة'],
            datasets: [{
                data: [approved, drafts],
                backgroundColor: ['#dc3545', '#ffc107'],
                hoverBackgroundColor: ['#c82333', '#e0a800']
            }]
        };
    }

    initCharts() {
        this.returnsTrendOptions = {
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
                y: { display: false },
                x: { display: false }
            }
        };

        this.returnsReasonOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { family: 'Cairo, sans-serif', size: 11 }, usePointStyle: true }
                }
            },
            cutout: '65%'
        };
    }

    navigateToCreate() {
        this.router.navigate(['/sales/returns/create']);
    }

    viewDetails(id: number) {
        this.router.navigate(['/sales/returns', id]); // Or reuse create view in read mode
    }

    getStatusSeverity(status: any): 'success' | 'warning' | 'danger' | 'info' {
        if (status === undefined || status === null) return 'info';
        const statusNum = Number(status);

        switch (statusNum) {
            case DocumentStatus.Approved: return 'success';
            case DocumentStatus.Draft: return 'warning';
            case DocumentStatus.Cancelled: return 'danger';
            default: return 'info';
        }
    }

    getStatusLabel(status: any): string {
        if (status === undefined || status === null) return 'غير محدد';
        const statusNum = Number(status);

        switch (statusNum) {
            case DocumentStatus.Approved: return 'معتمد';
            case DocumentStatus.Draft: return 'مسودة';
            case DocumentStatus.Cancelled: return 'ملغى';
            default: return 'غير معروف';
        }
    }

    approveReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من اعتماد هذا المرتجع؟ سيتم إضافة الكميات للمخزون.',
            header: 'تأكيد الاعتماد',
            icon: 'pi pi-check-circle',
            acceptLabel: 'نعم، اعتمد',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-success',
            accept: () => {
                this.salesReturnService.approve(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الاعتماد' })
                });
            }
        });
    }

    cancelReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من إلغاء هذا المرتجع؟',
            header: 'تأكيد الإلغاء',
            icon: 'pi pi-times-circle',
            acceptLabel: 'نعم، ألغِ',
            rejectLabel: 'تراجع',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.salesReturnService.cancel(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم الإلغاء', detail: 'تم إلغاء المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الإلغاء' })
                });
            }
        });
    }

    deleteReturn(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من حذف هذه المسودة؟',
            header: 'تأكيد الحذف',
            icon: 'pi pi-trash',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'تراجع',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.salesReturnService.delete(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المرتجع' });
                        this.loadReturns();
                    },
                    error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' })
                });
            }
        });
    }
}
