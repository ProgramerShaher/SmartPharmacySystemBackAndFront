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

    getReferenceTypeLabel(type: ReferenceType): string {
        const labels: Record<ReferenceType, string> = {
            [ReferenceType.Manual]: 'تسوية يدوية',
            [ReferenceType.SaleInvoice]: 'فاتورة مبيعات',
            [ReferenceType.PurchaseInvoice]: 'فاتورة شراء',
            [ReferenceType.Salary]: 'راتب',
            [ReferenceType.Expenses]: 'مصروفات',
            [ReferenceType.SaleReturn]: 'مردود مبيعات',
            [ReferenceType.PurchaseReturn]: 'مردود مشتريات'
        };
        return labels[type] || 'غير محدد';
    }

    getReferenceTypeSeverity(type: ReferenceType): any {
        const severities: Record<ReferenceType, string> = {
            [ReferenceType.Manual]: 'warning',
            [ReferenceType.SaleInvoice]: 'success',
            [ReferenceType.PurchaseInvoice]: 'info',
            [ReferenceType.Salary]: 'danger',
            [ReferenceType.Expenses]: 'danger',
            [ReferenceType.SaleReturn]: 'warning',
            [ReferenceType.PurchaseReturn]: 'secondary'
        };
        return severities[type] || 'info';
    }

    exportToExcel() {
        this.messageService.add({
            severity: 'info',
            summary: 'قريباً',
            detail: 'سيتم إضافة ميزة التصدير قريباً'
        });
    }
}
