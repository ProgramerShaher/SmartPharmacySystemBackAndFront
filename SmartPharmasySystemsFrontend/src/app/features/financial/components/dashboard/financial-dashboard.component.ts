import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FinancialService } from '../../../../core/services/financial.service';
import { AccountingService } from '../../../../core/services/accounting.service';
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

    // Main Safe Ledger
    mainSafeLedger: any = null;
    loadingMainSafeLedger = true;
    mainSafeAccountId: number | null = null;
    ledgerDateFilter: Date[] | undefined;

    constructor(
        private financialService: FinancialService,
        private accountingService: AccountingService,
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
        
        // Find Main Safe Account ID (11101)
        this.accountingService.getAccountsTree().subscribe(tree => {
            const findAccount = (nodes: any[], code: string): any => {
                for (let node of nodes) {
                    if (node.code === code) return node;
                    if (node.children && node.children.length > 0) {
                        const found = findAccount(node.children, code);
                        if (found) return found;
                    }
                }
                return null;
            };
            const mainSafe = findAccount(tree, '11101');
            if (mainSafe) {
                this.mainSafeAccountId = mainSafe.id;
                this.loadMainSafeLedger();
            }
        });
    }

    refreshAll() {
        this.loadingBalance = true;
        this.loadingReport = true;
        this.loadingTransactions = true;

        this.financialService.refreshBalance();
        this.loadReport();
        this.loadTransactions();
        this.loadCharts();
        if (this.mainSafeAccountId) this.loadMainSafeLedger();
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

    loadMainSafeLedger() {
        if (!this.mainSafeAccountId) return;
        this.loadingMainSafeLedger = true;
        
        const today = new Date();
        let startDate = new Date(today.getFullYear(), today.getMonth(), 1).toISOString();
        let endDate = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString();

        if (this.ledgerDateFilter && this.ledgerDateFilter[0] && this.ledgerDateFilter[1]) {
            startDate = this.ledgerDateFilter[0].toISOString();
            endDate = this.ledgerDateFilter[1].toISOString();
        }

        this.accountingService.getAccountLedger(this.mainSafeAccountId, startDate, endDate).subscribe({
            next: (data) => {
                this.mainSafeLedger = data;
                this.loadingMainSafeLedger = false;
            },
            error: () => this.loadingMainSafeLedger = false
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
        switch (this.normalizeTransactionType(type)) {
            case 1: return 'وارد (Income)';
            case 2: return 'منصرف (Expense)';
            case 3: return 'تسوية';
            default: return 'غير معروف';
        }
    }

    getTransactionSeverity(type: number): "success" | "danger" | "warning" | "info" {
        switch (this.normalizeTransactionType(type)) {
            case 1: return 'success';
            case 2: return 'danger';
            case 3: return 'warning';
            default: return 'info';
        }
    }

    isIncome(type: any): boolean {
        return this.normalizeTransactionType(type) === 1;
    }

    isExpense(type: any): boolean {
        return this.normalizeTransactionType(type) === 2;
    }

    private normalizeTransactionType(type: any): number {
        // Backend may serialize enums as numbers (1/2) or strings ("Income"/"Expense").
        if (type === 1 || type === '1' || type === 'Income' || type === 'INCOME') return 1;
        if (type === 2 || type === '2' || type === 'Expense' || type === 'EXPENSE') return 2;
        if (type === 3 || type === '3' || type === 'Adjustment' || type === 'ADJUSTMENT') return 3;

        const n = Number(type);
        return Number.isFinite(n) ? n : 0;
    }
}
