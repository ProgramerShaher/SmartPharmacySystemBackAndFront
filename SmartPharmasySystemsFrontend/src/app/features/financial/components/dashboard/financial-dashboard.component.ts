import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FinancialService } from '../../../../core/services/financial.service';
import {
    FinancialTransaction,
    FinancialReport,
    FinancialTransactionType
} from '../../../../core/models/financial.models';

// PrimeNG
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ChartModule } from 'primeng/chart';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { SkeletonModule } from 'primeng/skeleton';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TabViewModule } from 'primeng/tabview';
import { ToolbarModule } from 'primeng/toolbar';

@Component({
    selector: 'app-financial-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        CardModule,
        TableModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        InputNumberModule,
        InputTextareaModule,
        DropdownModule,
        CalendarModule,
        ChartModule,
        TagModule,
        ConfirmDialogModule,
        ToastModule,
        SkeletonModule,
        TabViewModule,
        ToolbarModule
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './financial-dashboard.component.html',
    styleUrls: ['./financial-dashboard.component.scss']
})
export class FinancialDashboardComponent implements OnInit {
    currentBalance = 0;
    loadingBalance = true;
    loadingReport = true;
    loadingTransactions = true;

    report: FinancialReport | null = null;
    transactions: FinancialTransaction[] = [];
    dateFilter: Date[] | undefined;

    // Charts
    years = [{ label: '2025', value: 2025 }, { label: '2024', value: 2024 }];
    selectedYear = 2025;
    chartData: any;
    basicOptions: any;
    pieData: any;
    pieOptions: any;

    // Adjustment
    displayAdjustmentDialog = false;
    adjustmentForm: FormGroup;
    saving = false;

    constructor(
        private financialService: FinancialService,
        private fb: FormBuilder,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) {
        this.adjustmentForm = this.fb.group({
            amount: [null, [Validators.required]],
            description: ['', [Validators.required, Validators.minLength(5)]]
        });

        // Initialize chart options
        this.basicOptions = {
            plugins: {
                legend: { labels: { color: '#495057' } }
            },
            scales: {
                x: { ticks: { color: '#495057' }, grid: { color: '#ebedef' } },
                y: { ticks: { color: '#495057' }, grid: { color: '#ebedef' } }
            }
        };
        this.pieOptions = {
            plugins: {
                legend: { labels: { color: '#495057' } }
            }
        };
    }

    ngOnInit() {
        // Subscribe to real-time balance
        this.financialService.balance$.subscribe(bal => {
            this.currentBalance = bal;
            this.loadingBalance = false;
        });

        this.refreshAll();
    }

    refreshAll() {
        this.loadingBalance = true;
        this.loadingReport = true;
        this.loadingTransactions = true;

        this.financialService.refreshBalance();
        this.loadReport();
        this.loadTransactions();
        this.loadCharts();
    }

    loadReport() {
        const today = new Date();
        const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1).toISOString();
        const endOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString();

        this.financialService.getReport(startOfMonth, endOfMonth).subscribe({
            next: (data) => {
                this.report = data;
                this.loadingReport = false;
            },
            error: () => this.loadingReport = false
        });
    }

    loadTransactions() {
        const query: any = { page: 1, pageSize: 50 }; // Default latest 50
        if (this.dateFilter && this.dateFilter[0] && this.dateFilter[1]) {
            query.startDate = this.dateFilter[0].toISOString();
            query.endDate = this.dateFilter[1].toISOString();
        }

        this.financialService.getTransactions(query).subscribe({
            next: (data) => {
                this.transactions = data.items;
                this.loadingTransactions = false;
            },
            error: () => this.loadingTransactions = false
        });
    }

    loadCharts() {
        this.financialService.getAnnualSummary(this.selectedYear).subscribe(data => {
            const labels = data.map(d => d.categoryName);
            const values = data.map(d => d.totalAmount);

            this.chartData = {
                labels: labels,
                datasets: [
                    {
                        label: 'المصاريف والإيرادات',
                        data: values,
                        backgroundColor: ['#42A5F5', '#66BB6A', '#FFA726', '#FF7043', '#AB47BC'],
                    }
                ]
            };

            this.pieData = {
                labels: labels,
                datasets: [
                    {
                        data: values,
                        backgroundColor: ['#42A5F5', '#66BB6A', '#FFA726', '#FF7043', '#AB47BC'],
                    }
                ]
            };
        });
    }

    showAdjustmentDialog() {
        this.adjustmentForm.reset();
        this.displayAdjustmentDialog = true;
    }

    saveAdjustment() {
        if (this.adjustmentForm.invalid) return;

        this.confirmationService.confirm({
            message: 'هل أنت متأكد من تسجيل هذه التسوية المالية؟ سيتم تحديث الرصيد مباشرة.',
            header: 'تأكيد التسوية',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، اعتمد التسوية',
            rejectLabel: 'إلغاء',
            accept: () => {
                this.saving = true;
                const req = this.adjustmentForm.value;
                this.financialService.recordManualAdjustment(req).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم تسجيل التسوية المالية وتحديث الرصيد' });
                        this.displayAdjustmentDialog = false;
                        this.saving = false;
                        this.loadTransactions(); // Refresh table
                    },
                    error: (err) => {
                        this.saving = false;
                        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تسجيل التسوية' });
                    }
                });
            }
        });
    }

    getTransactionTypeLabel(type: number) {
        switch (type) {
            case 1: return 'وارد (Income)';
            case 2: return 'منصرف (Expense)';
            case 3: return 'تسوية';
            default: return 'غير معروف';
        }
    }

    getTransactionSeverity(type: number): "success" | "danger" | "warning" | "info" {
        switch (type) {
            case 1: return 'success';
            case 2: return 'danger';
            case 3: return 'warning';
            default: return 'info';
        }
    }
}
