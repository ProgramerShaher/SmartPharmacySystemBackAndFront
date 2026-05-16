import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountingService } from '../../../../core/services/accounting.service';
import { TrialBalanceDto } from '../../../../core/models/accounting.interface';
import { MessageService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-trial-balance',
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
    templateUrl: './trial-balance.component.html',
    styleUrls: ['./trial-balance.component.scss']
})
export class TrialBalanceComponent implements OnInit {
    // Data Signals
    trialBalance = signal<TrialBalanceDto | null>(null);
    loading = signal(false);
    
    // Filter Signals
    dateRange = signal<Date[]>([
        new Date(new Date().getFullYear(), 0, 1), // Start of year
        new Date()
    ]);

    Math = Math;

    constructor(
        private accountingService: AccountingService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadTrialBalance();
    }

    loadTrialBalance() {
        if (!this.dateRange() || this.dateRange().length < 2 || !this.dateRange()[1]) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تحديد فترة زمنية صحيحة' });
            return;
        }

        this.loading.set(true);
        
        // Format dates as YYYY-MM-DD
        const formatDate = (date: Date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        const from = formatDate(this.dateRange()[0]);
        const to = formatDate(this.dateRange()[1]);

        this.accountingService.getTrialBalance(from, to).subscribe({
            next: (data) => {
                this.trialBalance.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: err.error?.message || 'تعذر تحميل ميزان المراجعة'
                });
                this.loading.set(false);
            }
        });
    }

    formatCurrency(value: number): string {
        if (value === 0 || value === null || value === undefined) return '-';
        return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    }

    getAccountTypeClass(type: string): string {
        switch (type) {
            case 'Asset': return 'type-asset';
            case 'Liability': return 'type-liability';
            case 'Equity': return 'type-equity';
            case 'Revenue': return 'type-revenue';
            case 'Expense': return 'type-expense';
            default: return '';
        }
    }

    getTypeNameArabic(type: string): string {
        switch (type) {
            case 'Asset': return 'أصول';
            case 'Liability': return 'خصوم';
            case 'Equity': return 'حقوق ملكية';
            case 'Revenue': return 'إيرادات';
            case 'Expense': return 'مصاريف';
            default: return type;
        }
    }

    printReport() {
        window.print();
    }

    exportToExcel() {
        this.messageService.add({ severity: 'info', summary: 'تصدير', detail: 'جاري تحضير ميزان المراجعة بصيغة Excel...' });
    }
}
