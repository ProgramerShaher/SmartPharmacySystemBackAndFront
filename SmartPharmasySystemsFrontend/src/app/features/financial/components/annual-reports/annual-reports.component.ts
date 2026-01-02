import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService } from '../../../../core/services/financial.service';
import { AnnualFinancialReport, FinancialSummary } from '../../../../core/models/financial.models';
import { MessageService } from 'primeng/api';

// PrimeNG
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-annual-reports',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        CardModule,
        ButtonModule,
        DropdownModule,
        ChartModule,
        TableModule,
        ToastModule,
        ProgressSpinnerModule,
        TagModule
    ],
    providers: [MessageService],
    templateUrl: './annual-reports.component.html',
    styleUrls: ['./annual-reports.component.scss']
})
export class AnnualReportsComponent implements OnInit {
    // Expose Math to template
    Math = Math;

    // Signals
    selectedYear = signal(new Date().getFullYear());
    annualReport = signal<AnnualFinancialReport[]>([]);
    annualSummary = signal<FinancialSummary[]>([]);
    loading = signal(false);

    // Available years
    years = signal<number[]>([]);

    // Chart data (computed from signals)
    chartData = computed(() => {
        const summary = this.annualSummary();
        if (!summary || summary.length === 0) return null;

        return {
            labels: summary.map(s => s.categoryName),
            datasets: [
                {
                    label: 'المبلغ (ريال يمني)',
                    data: summary.map(s => s.totalAmount),
                    backgroundColor: [
                        'rgba(16, 185, 129, 0.8)',
                        'rgba(239, 68, 68, 0.8)',
                        'rgba(59, 130, 246, 0.8)',
                        'rgba(245, 158, 11, 0.8)',
                        'rgba(139, 92, 246, 0.8)',
                        'rgba(236, 72, 153, 0.8)'
                    ],
                    borderColor: [
                        'rgb(16, 185, 129)',
                        'rgb(239, 68, 68)',
                        'rgb(59, 130, 246)',
                        'rgb(245, 158, 11)',
                        'rgb(139, 92, 246)',
                        'rgb(236, 72, 153)'
                    ],
                    borderWidth: 2
                }
            ]
        };
    });

    chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom',
                labels: {
                    font: {
                        family: 'Cairo',
                        size: 12
                    },
                    usePointStyle: true,
                    padding: 20
                }
            },
            tooltip: {
                callbacks: {
                    label: (context: any) => {
                        const label = context.label || '';
                        const value = context.parsed || 0;
                        return `${label}: ${value.toLocaleString('ar-YE')} ر.ي`;
                    }
                }
            }
        }
    };

    // Computed totals
    totalIncome = computed(() => {
        const summary = this.annualSummary();
        return summary
            .filter(s => s.categoryName.includes('مبيعات') || s.categoryName.includes('دخل'))
            .reduce((sum, s) => sum + s.totalAmount, 0);
    });

    totalExpense = computed(() => {
        const summary = this.annualSummary();
        return summary
            .filter(s => s.categoryName.includes('مشتريات') || s.categoryName.includes('مصروف'))
            .reduce((sum, s) => sum + s.totalAmount, 0);
    });

    netProfit = computed(() => this.totalIncome() - this.totalExpense());

    constructor(
        private financialService: FinancialService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.initializeYears();
        this.loadReports();
    }

    initializeYears() {
        const currentYear = new Date().getFullYear();
        const yearsList = [];
        for (let i = 0; i < 5; i++) {
            yearsList.push(currentYear - i);
        }
        this.years.set(yearsList);
    }

    loadReports() {
        this.loading.set(true);
        const year = this.selectedYear();

        // Load both annual report and summary
        Promise.all([
            this.financialService.getAnnualReport(year).toPromise(),
            this.financialService.getAnnualSummary(year).toPromise()
        ])
            .then(([report, summary]) => {
                this.annualReport.set(report || []);
                this.annualSummary.set(summary || []);
                this.loading.set(false);
            })
            .catch((err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل التقارير السنوية'
                });
                this.loading.set(false);
            });
    }

    onYearChange() {
        this.loadReports();
    }

    exportReport() {
        this.messageService.add({
            severity: 'info',
            summary: 'قريباً',
            detail: 'سيتم إضافة ميزة التصدير قريباً'
        });
    }

    printReport() {
        window.print();
    }
}
