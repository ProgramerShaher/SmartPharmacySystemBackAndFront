import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DividerModule } from 'primeng/divider';

// Services & Models
import { ReportService } from '../../../core/services/report.service';
import { NetProfitReport, ExpenseBreakdown } from '../../../core/models/reports.interface';

/**
 * تقرير صافي الأرباح الدوري
 * Periodic Net Profit Report Component
 * معادلة: صافي الربح = (إجمالي المبيعات - المرتجعات - الخصومات) - تكلفة البضاعة المباعة - المصروفات
 */
@Component({
    selector: 'app-net-profit',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        ButtonModule,
        CalendarModule,
        ChartModule,
        TableModule,
        TagModule,
        ProgressSpinnerModule,
        DividerModule
    ],
    templateUrl: './net-profit.component.html',
    styleUrls: ['./net-profit.component.css']
})
export class NetProfitComponent implements OnInit {
    // Injected Services
    private readonly reportService = inject(ReportService);

    // ===================== Signals State =====================

    /** Report data signal */
    report = signal<NetProfitReport | null>(null);

    /** Loading state */
    loading = signal<boolean>(false);

    /** Error message */
    error = signal<string | null>(null);

    /** Date range - regular properties for ngModel */
    fromDate: Date = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    toDate: Date = new Date();

    /** Chart data for expenses */
    expenseChartData = signal<any>(null);
    expenseChartOptions = signal<any>(null);

    // ===================== Computed Signals =====================

    /** Is profitable */
    isProfitable = computed(() => (this.report()?.netProfit || 0) >= 0);

    /** Profit percentage formatted */
    profitPercentage = computed(() => {
        const margin = this.report()?.netProfitMargin || 0;
        return margin.toFixed(2);
    });

    // ===================== Lifecycle =====================

    ngOnInit(): void {
        this.initChartOptions();
        this.loadReport();
    }

    // ===================== Methods =====================

    /** Initialize chart options */
    initChartOptions(): void {
        this.expenseChartOptions.set({
            plugins: {
                legend: {
                    labels: {
                        usePointStyle: true,
                        font: { family: 'Cairo, sans-serif' }
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: false
        });
    }

    /** Load report from API */
    async loadReport(): Promise<void> {
        this.loading.set(true);
        this.error.set(null);

        try {
            const report = await this.reportService.getNetProfitReport(
                this.fromDate,
                this.toDate,
                true
            ).toPromise();

            this.report.set(report || null);
            this.updateExpenseChart(report?.expensesByCategory || []);
        } catch (err: any) {
            this.error.set(err?.error?.message || 'حدث خطأ أثناء تحميل تقرير الأرباح');
            console.error('Report load error:', err);
        } finally {
            this.loading.set(false);
        }
    }

    /** Update expense pie chart */
    updateExpenseChart(expenses: ExpenseBreakdown[]): void {
        if (!expenses.length) {
            this.expenseChartData.set(null);
            return;
        }

        const colors = [
            '#42A5F5', '#66BB6A', '#FFA726', '#AB47BC', '#26C6DA',
            '#7E57C2', '#EC407A', '#78909C', '#8D6E63'
        ];

        this.expenseChartData.set({
            labels: expenses.map(e => e.categoryName),
            datasets: [{
                data: expenses.map(e => e.amount),
                backgroundColor: colors.slice(0, expenses.length),
                hoverBackgroundColor: colors.slice(0, expenses.length).map(c => c + 'DD')
            }]
        });
    }

    /** Apply date filter */
    applyDateFilter(): void {
        this.loadReport();
    }

    /** Preset: Current Month */
    setCurrentMonth(): void {
        const now = new Date();
        this.fromDate = new Date(now.getFullYear(), now.getMonth(), 1);
        this.toDate = now;
        this.loadReport();
    }

    /** Preset: Last Month */
    setLastMonth(): void {
        const now = new Date();
        this.fromDate = new Date(now.getFullYear(), now.getMonth() - 1, 1);
        this.toDate = new Date(now.getFullYear(), now.getMonth(), 0);
        this.loadReport();
    }

    /** Preset: Current Year */
    setCurrentYear(): void {
        const now = new Date();
        this.fromDate = new Date(now.getFullYear(), 0, 1);
        this.toDate = now;
        this.loadReport();
    }

    /** Print report */
    printReport(): void {
        window.print();
    }

    formatCurrency(value: number): string {
        return value.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    }
}
