export enum AccountType {
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

export interface AccountDto {
    id: number;
    code: string;
    name: string;
    accountType: AccountType;
    type: string; // Friendly name for AccountType
    currentBalance: number;
    parentId?: number;
    parentName?: string;
    isMainAccount: boolean;
    isActive: boolean;
    level: number;
    children?: AccountDto[];
}

export interface CreateAccountDto {
    code: string;
    name: string;
    accountType: AccountType;
    parentId?: number;
    description?: string;
}

export interface JournalEntryLineDto {
    id?: number;
    accountId: number;
    accountName?: string;
    accountCode?: string;
    code?: string;
    debit: number;
    credit: number;
    description: string;
}

export interface JournalEntryDto {
    id: number;
    entryDate: string;
    description: string;
    isPosted: boolean;
    totalDebit: number;
    totalCredit: number;
    createdBy?: number;
    createdByName?: string;
    lines: JournalEntryLineDto[];
}

export interface TrialBalanceLineDto {
    accountCode: string;
    accountName: string;
    accountType: string;
    openingDebit: number;
    openingCredit: number;
    periodDebit: number;
    periodCredit: number;
    closingDebit: number;
    closingCredit: number;
}

export interface TrialBalanceDto {
    lines: TrialBalanceLineDto[];
    totalOpeningDebit: number;
    totalOpeningCredit: number;
    totalPeriodDebit: number;
    totalPeriodCredit: number;
    totalClosingDebit: number;
    totalClosingCredit: number;
}

export interface IncomeStatementLineDto {
    accountName: string;
    amount: number;
}

export interface IncomeStatementDto {
    revenues: IncomeStatementLineDto[];
    expenses: IncomeStatementLineDto[];
    totalRevenue: number;
    totalExpense: number;
    netProfit: number;
}

export interface BalanceSheetLineDto {
    accountName: string;
    amount: number;
}

export interface BalanceSheetDto {
    assets: BalanceSheetLineDto[];
    liabilities: BalanceSheetLineDto[];
    equity: BalanceSheetLineDto[];
    totalAssets: number;
    totalLiabilities: number;
    totalEquity: number;
}

export interface LedgerEntryDto {
    date: string;
    voucherNumber: string;
    description: string;
    debit: number;
    credit: number;
    runningBalance: number;
    journalEntryId: number;
}

export interface LedgerReportDto {
    accountId: number;
    accountName: string;
    accountCode: string;
    startDate: string;
    endDate: string;
    openingBalance: number;
    totalDebit: number;
    totalCredit: number;
    closingBalance: number;
    entries: LedgerEntryDto[];
}
