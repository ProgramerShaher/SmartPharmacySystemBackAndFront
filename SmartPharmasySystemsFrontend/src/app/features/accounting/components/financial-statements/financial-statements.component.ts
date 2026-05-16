import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountingService } from '../../../../core/services/accounting.service';
import { IncomeStatementDto, BalanceSheetDto } from '../../../../core/models/accounting.interface';
import { MessageService } from 'primeng/api';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-financial-statements',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        CalendarModule,
        ToastModule,
        ProgressSpinnerModule,
        TooltipModule
    ],
    providers: [MessageService],
    templateUrl: './financial-statements.component.html',
    styleUrls: ['./financial-statements.component.scss']
})
export class FinancialStatementsComponent implements OnInit {
    // Data Signals
    incomeStatement = signal<IncomeStatementDto | null>(null);
    balanceSheet = signal<BalanceSheetDto | null>(null);
    loading = signal(false);
    
    // Filter Signals
    dateRange = signal<Date[]>([
        new Date(new Date().getFullYear(), 0, 1), // Start of year
        new Date()
    ]);

    Math = Math;

    // Computed Totals for Balance Sheet
    totalLiabilitiesAndEquity = computed(() => {
        const bs = this.balanceSheet();
        if (!bs) return 0;
        return bs.totalLiabilities + bs.totalEquity;
    });

    isBalanced = computed(() => {
        const bs = this.balanceSheet();
        if (!bs) return true;
        return Math.abs(bs.totalAssets - this.totalLiabilitiesAndEquity()) < 1;
    });

    constructor(
        private accountingService: AccountingService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadStatements();
    }

    loadStatements() {
        if (!this.dateRange() || this.dateRange().length < 2 || !this.dateRange()[1]) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تحديد فترة زمنية صحيحة' });
            return;
        }

        this.loading.set(true);
        
        const formatDate = (date: Date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        const from = formatDate(this.dateRange()[0]);
        const to = formatDate(this.dateRange()[1]);

        forkJoin({
            income: this.accountingService.getIncomeStatement(from, to),
            balance: this.accountingService.getBalanceSheet(to)
        }).pipe(
            finalize(() => this.loading.set(false))
        ).subscribe({
            next: (data) => {
                this.incomeStatement.set(data.income);
                this.balanceSheet.set(data.balance);
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: err.error?.message || 'تعذر تحميل القوائم المالية'
                });
            }
        });
    }

    formatCurrency(value: number): string {
        if (value === 0 || value === null || value === undefined) return '0.00';
        return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    }

    printReport() {
        window.print();
    }

    exportToExcel() {
        this.messageService.add({ severity: 'info', summary: 'تصدير', detail: 'جاري تحضير القوائم المالية بصيغة Excel...' });
    }
}
