import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

// PrimeNG Imports
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DividerModule } from 'primeng/divider';

// Services & Models
import { ReportService } from '../../../core/services/report.service';
import { UnifiedStatement, StatementLine } from '../../../core/models/reports.interface';

/**
 * كشف الحساب الموحد للعملاء والموردين
 * Unified Account Statement Component
 * Features: Angular Signals for instant filtering, Sticky Header, Color Coding
 */
@Component({
    selector: 'app-unified-statement',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        RouterModule,
        TableModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        CalendarModule,
        DropdownModule,
        TagModule,
        TooltipModule,
        ProgressSpinnerModule,
        DividerModule
    ],
    templateUrl: './unified-statement.component.html',
    styleUrls: ['./unified-statement.component.css']
})
export class UnifiedStatementComponent implements OnInit {
    // Injected Services
    private readonly reportService = inject(ReportService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);

    // ===================== Signals State =====================

    /** Statement data signal */
    statement = signal<UnifiedStatement | null>(null);

    /** Loading state */
    loading = signal<boolean>(false);

    /** Error message */
    error = signal<string | null>(null);

    /** Search term for instant filtering */
    searchTerm = signal<string>('');

    /** Entity type from route */
    entityType = signal<string>('Customer');

    /** Entity ID from route */
    entityId = signal<number>(0);

    /** Date range - regular properties for ngModel */
    fromDate: Date | null = null;
    toDate: Date | null = null;

    // ===================== Computed Signals =====================

    /** Filtered lines based on search term - INSTANT FILTER */
    filteredLines = computed(() => {
        const term = this.searchTerm().toLowerCase().trim();
        const lines = this.statement()?.lines || [];

        if (!term) return lines;

        return lines.filter(line =>
            line.referenceNumber.toLowerCase().includes(term) ||
            line.description.toLowerCase().includes(term) ||
            line.referenceType.toLowerCase().includes(term)
        );
    });

    /** Summary statistics */
    summary = computed(() => {
        const stmt = this.statement();
        return {
            openingBalance: stmt?.openingBalance || 0,
            totalDebit: stmt?.totalDebit || 0,
            totalCredit: stmt?.totalCredit || 0,
            currentBalance: stmt?.currentBalance || 0,
            transactionCount: stmt?.lines?.length || 0
        };
    });

    /** Entity type display name */
    entityTypeDisplay = computed(() => {
        return this.entityType() === 'Customer' ? 'عميل' : 'مورد';
    });

    // ===================== Lifecycle =====================

    ngOnInit(): void {
        // Get route params
        this.route.params.subscribe(params => {
            const type = params['entityType'] || 'Customer';
            const id = parseInt(params['entityId'], 10) || 0;

            this.entityType.set(type);
            this.entityId.set(id);

            if (id > 0) {
                this.loadStatement();
            }
        });
    }

    // ===================== Methods =====================

    /** Load statement from API */
    async loadStatement(): Promise<void> {
        this.loading.set(true);
        this.error.set(null);

        try {
            const statement = await this.reportService.getUnifiedStatement(
                this.entityType(),
                this.entityId(),
                this.fromDate || undefined,
                this.toDate || undefined
            ).toPromise();

            this.statement.set(statement || null);
        } catch (err: any) {
            this.error.set(err?.error?.message || 'حدث خطأ أثناء تحميل كشف الحساب');
            console.error('Statement load error:', err);
        } finally {
            this.loading.set(false);
        }
    }
    /** Apply date filter */
    applyDateFilter(): void {
        this.loadStatement();
    }

    /** Clear date filter */
    clearDateFilter(): void {
        this.fromDate = null;
        this.toDate = null;
        this.loadStatement();
    }

    /** Export to CSV */
    exportCsv(): void {
        this.reportService.exportStatementCsv(
            this.entityType(),
            this.entityId(),
            this.fromDate || undefined,
            this.toDate || undefined
        ).subscribe(blob => {
            const filename = `statement_${this.entityType()}_${this.entityId()}_${new Date().toISOString().split('T')[0]}.csv`;
            this.reportService.downloadExport(blob, filename);
        });
    }

    /** Print statement */
    printStatement(): void {
        window.print();
    }

    /** Navigate to reference */
    navigateToReference(line: StatementLine): void {
        if (!line.referenceId) return;

        let route = '';
        switch (line.referenceType) {
            case 'فاتورة مبيعات':
                route = `/sales/invoices/${line.referenceId}`;
                break;
            case 'فاتورة شراء':
                route = `/purchases/invoices/${line.referenceId}`;
                break;
            case 'سند قبض':
                route = `/customers/receipts/${line.referenceId}`;
                break;
            case 'سند صرف':
                route = `/suppliers/payments/${line.referenceId}`;
                break;
            case 'مرتجع مبيعات':
                route = `/sales/returns/${line.referenceId}`;
                break;
            case 'مرتجع مشتريات':
                route = `/purchases/returns/${line.referenceId}`;
                break;
        }

        if (route) {
            this.router.navigate([route]);
        }
    }

    /** Get row class based on transaction type */
    getRowClass(line: StatementLine): string {
        if (line.debit > 0) return 'row-debit';
        if (line.credit > 0) return 'row-credit';
        return '';
    }

    /** Get severity for tag */
    getBalanceSeverity(balance: number): 'success' | 'danger' | 'info' {
        if (balance > 0) return 'danger';
        if (balance < 0) return 'success';
        return 'info';
    }
}
