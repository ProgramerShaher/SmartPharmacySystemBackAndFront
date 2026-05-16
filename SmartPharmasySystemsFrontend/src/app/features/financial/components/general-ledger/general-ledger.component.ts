import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountingService } from '../../../../core/services/accounting.service';
import { LedgerReportDto } from '../../../../core/models/accounting.interface';
import { MessageService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-general-ledger',
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
    templateUrl: './general-ledger.component.html',
    styleUrls: ['./general-ledger.component.scss']
})
export class GeneralLedgerComponent implements OnInit {
    // Data Signals
    allLedgers = signal<LedgerReportDto[]>([]);
    loading = signal(false);
    
    // Filter Signals
    dateRange = signal<Date[]>([
        new Date(new Date().getFullYear(), new Date().getMonth(), 1), // Start of month
        new Date()
    ]);

    constructor(
        private accountingService: AccountingService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadAllLedgers();
    }

    loadAllLedgers() {
        if (!this.dateRange() || this.dateRange().length < 2 || !this.dateRange()[1]) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى تحديد فترة زمنية صحيحة' });
            return;
        }

        this.loading.set(true);
        
        // تحويل التاريخ لصيغة YYYY-MM-DD لتجنب أخطاء الـ Binding في الـ Backend
        const formatDate = (date: Date) => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        const startDate = formatDate(this.dateRange()[0]);
        const endDate = formatDate(this.dateRange()[1]);

        this.accountingService.getAllLedgers(startDate, endDate).subscribe({
            next: (data) => {
                this.allLedgers.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: err.error?.message || 'تعذر تحميل كشوفات الحسابات'
                });
                this.loading.set(false);
            }
        });
    }

    formatCurrency(value: number): string {
        if (value === null || value === undefined) return '-';
        return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    }

    getAccountColor(index: number): string {
        const colors = ['#1e3c72', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#06b6d4', '#6366f1'];
        return colors[index % colors.length];
    }

    exportToExcel() {
        this.messageService.add({ severity: 'info', summary: 'تصدير', detail: 'جاري تحضير ملف Excel مضغوط...' });
    }

    onFilterChange() {
        if (this.dateRange() && this.dateRange().length === 2 && this.dateRange()[1]) {
            this.loadAllLedgers();
        }
    }

    clearFilters() {
        this.dateRange.set([
            new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            new Date()
        ]);
        this.loadAllLedgers();
    }
}
