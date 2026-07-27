import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AccountingService } from '../../../../core/services/accounting.service';
import { AccountDto, AccountType, TrialBalanceLineDto } from '../../../../core/models/accounting.interface';

import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext';

export interface AccountBalanceRow {
    id: number;
    code: string;
    name: string;
    accountType: AccountType;
    typeName: string;
    level: number;
    isMainAccount: boolean;
    isActive: boolean;
    openingBalance: number;  // رصيد افتتاحي
    currentBalance: number;  // رصيد حالي
}

@Component({
    selector: 'app-accounts-balances',
    standalone: true,
    imports: [
        CommonModule, 
        FormsModule,
        ButtonModule,
        TableModule,
        ProgressSpinnerModule,
        InputTextModule
    ],
    templateUrl: './accounts-balances.component.html',
    styleUrls: ['./accounts-balances.component.scss']
})
export class AccountsBalancesComponent implements OnInit {
    rows = signal<AccountBalanceRow[]>([]);
    loading = signal(true);
    searchQuery = signal('');
    selectedType = signal<string>('all');

    accountTypes = [
        { value: 'all',  label: 'جميع الأنواع' },
        { value: '1',    label: 'الأصول' },
        { value: '2',    label: 'الخصوم' },
        { value: '3',    label: 'حقوق الملكية' },
        { value: '4',    label: 'الإيرادات' },
        { value: '5',    label: 'المصاريف' },
    ];

    filteredRows = computed(() => {
        const q    = this.searchQuery().toLowerCase();
        const type = this.selectedType();
        return this.rows().filter(r => {
            const matchSearch = !q ||
                r.name.toLowerCase().includes(q) ||
                r.code?.toLowerCase().includes(q);
            const matchType = type === 'all' || r.accountType?.toString() === type;
            return matchSearch && matchType;
        });
    });

    totalCurrent = computed(() =>
        this.filteredRows().reduce((s, r) => s + r.currentBalance, 0)
    );
    totalOpening = computed(() =>
        this.filteredRows().reduce((s, r) => s + r.openingBalance, 0)
    );
    positiveCount = computed(() =>
        this.filteredRows().filter(r => r.currentBalance > 0).length
    );
    negativeCount = computed(() =>
        this.filteredRows().filter(r => r.currentBalance < 0).length
    );

    constructor(private accountingService: AccountingService) { }

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);

        // نجلب شجرة الحسابات وميزان المراجعة معاً
        const startOfYear = `${new Date().getFullYear()}-01-01`;
        const today       = new Date().toISOString().split('T')[0];

        forkJoin({
            accounts:     this.accountingService.getAccountsTree(),
            trialBalance: this.accountingService.getTrialBalance(startOfYear, today)
        }).subscribe({
            next: ({ accounts, trialBalance }) => {
                // بناء خريطة الرصيد الافتتاحي من ميزان المراجعة
                const openingMap = new Map<string, number>();
                (trialBalance?.lines ?? []).forEach((line: TrialBalanceLineDto) => {
                    const opening = (line.openingDebit ?? 0) - (line.openingCredit ?? 0);
                    openingMap.set(line.accountCode, opening);
                });

                // تسطيح الشجرة وبناء الصفوف
                const flat = this.flattenAccounts(accounts);
                this.rows.set(flat.map(a => ({
                    id:             a.id,
                    code:           a.code,
                    name:           a.name,
                    accountType:    a.accountType,
                    typeName:       a.type ?? this.getTypeName(a.accountType),
                    level:          a.level,
                    isMainAccount:  a.isMainAccount,
                    isActive:       a.isActive,
                    openingBalance: openingMap.get(a.code) ?? 0,
                    currentBalance: a.currentBalance ?? 0,
                })));
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
            }
        });
    }

    flattenAccounts(accounts: AccountDto[]): AccountDto[] {
        const result: AccountDto[] = [];
        const traverse = (list: AccountDto[]) => {
            for (const acc of list) {
                result.push(acc);
                if (acc.children?.length) traverse(acc.children);
            }
        };
        traverse(accounts);
        return result;
    }

    getTypeName(type: AccountType): string {
        switch (+type) {
            case AccountType.Asset:     return 'أصول';
            case AccountType.Liability: return 'خصوم';
            case AccountType.Equity:    return 'حقوق ملكية';
            case AccountType.Revenue:   return 'إيرادات';
            case AccountType.Expense:   return 'مصاريف';
            default: return '-';
        }
    }

    getTypeClass(type: AccountType): string {
        switch (+type) {
            case AccountType.Asset:     return 'badge-asset';
            case AccountType.Liability: return 'badge-liability';
            case AccountType.Equity:    return 'badge-equity';
            case AccountType.Revenue:   return 'badge-revenue';
            case AccountType.Expense:   return 'badge-expense';
            default: return '';
        }
    }

    getTypeIcon(type: AccountType): string {
        switch (+type) {
            case AccountType.Asset:     return 'pi pi-building';
            case AccountType.Liability: return 'pi pi-exclamation-triangle';
            case AccountType.Equity:    return 'pi pi-shield';
            case AccountType.Revenue:   return 'pi pi-arrow-circle-up';
            case AccountType.Expense:   return 'pi pi-arrow-circle-down';
            default: return 'pi pi-circle';
        }
    }

    printPage() {
        window.print();
    }
}
