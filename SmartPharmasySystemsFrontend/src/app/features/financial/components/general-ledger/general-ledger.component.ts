import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinancialService } from '../../../../core/services/financial.service';
import { GeneralLedger, GeneralLedgerQueryDto, ReferenceType } from '../../../../core/models/financial.models';
import { MessageService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
    selector: 'app-general-ledger',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        ButtonModule,
        InputTextModule,
        CalendarModule,
        TagModule,
        ToastModule,
        CardModule,
        ProgressSpinnerModule
    ],
    providers: [MessageService],
    templateUrl: './general-ledger.component.html',
    styleUrls: ['./general-ledger.component.scss']
})
export class GeneralLedgerComponent implements OnInit {
    // Signals for reactive state
    ledgerEntries = signal<GeneralLedger[]>([]);
    loading = signal(false);
    totalRecords = signal(0);

    // Filter state
    dateRange = signal<Date[]>([]);
    currentPage = signal(1);
    pageSize = signal(20);

    constructor(
        private financialService: FinancialService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        this.loadLedger();
    }

    loadLedger() {
        this.loading.set(true);

        const query: GeneralLedgerQueryDto = {
            page: this.currentPage(),
            pageSize: this.pageSize()
        };

        // Add date filters if selected
        const dates = this.dateRange();
        if (dates && dates.length === 2) {
            query.start = dates[0].toISOString().split('T')[0];
            query.end = dates[1].toISOString().split('T')[0];
        }

        this.financialService.getGeneralLedger(query).subscribe({
            next: (result) => {
                this.ledgerEntries.set(result.items);
                this.totalRecords.set(result.totalCount);
                this.loading.set(false);
            },
            error: (err) => {
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل تحميل دفتر الأستاذ'
                });
                this.loading.set(false);
            }
        });
    }

    onPageChange(event: any) {
        this.currentPage.set(event.page + 1);
        this.loadLedger();
    }

    onDateFilter() {
        this.currentPage.set(1);
        this.loadLedger();
    }

    clearFilters() {
        this.dateRange.set([]);
        this.currentPage.set(1);
        this.loadLedger();
    }

    getReferenceTypeLabel(type: any): string {
        if (type === undefined || type === null) return 'غير محدد';
        
        const typeStr = type.toString();
        // Handle both numeric and string values
        if (typeStr === '0' || typeStr === 'Manual') return 'تسوية يدوية';
        if (typeStr === '1' || typeStr === 'SaleInvoice') return 'فاتورة مبيعات';
        if (typeStr === '2' || typeStr === 'PurchaseInvoice') return 'فاتورة شراء';
        if (typeStr === '3' || typeStr === 'Salary') return 'راتب';
        if (typeStr === '4' || typeStr === 'Expenses') return 'مصروفات';
        if (typeStr === '5' || typeStr === 'SaleReturn') return 'مردود مبيعات';
        if (typeStr === '6' || typeStr === 'PurchaseReturn') return 'مردود مشتريات';
        
        return 'غير محدد';
    }

    getReferenceTypeSeverity(type: any): string {
        const typeStr = type?.toString();
        if (typeStr === '1' || typeStr === 'SaleInvoice') return 'success';
        if (typeStr === '2' || typeStr === 'PurchaseInvoice') return 'info';
        if (typeStr === '5' || typeStr === 'SaleReturn') return 'warning';
        if (typeStr === '6' || typeStr === 'PurchaseReturn') return 'help';
        if (typeStr === '4' || typeStr === 'Expenses' || typeStr === '3' || typeStr === 'Salary') return 'danger';
        return 'secondary';
    }

    getTransactionNature(entry: GeneralLedger): string {
        if (entry.incoming > 0) return 'إيراد';
        if (entry.outgoing > 0) return 'مصروف';
        return 'أخرى';
    }

getTransactionNatureSeverity(entry: GeneralLedger): "success" | "info" | "warning" | "danger" | "secondary" | "contrast" | undefined {
        if (entry.incoming > 0) return 'success';
        if (entry.outgoing > 0) return 'danger';
        return 'warning';
    }

    exportToExcel() {
        this.messageService.add({
            severity: 'info',
            summary: 'قريباً',
            detail: 'سيتم إضافة ميزة التصدير قريباً'
        });
    }
}
