import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AccountingService } from '../../../../core/services/accounting.service';
import { JournalEntryDto, JournalEntryLineDto, AccountDto } from '../../../../core/models/accounting.interface';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-journal-entry-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    DialogModule,
    TagModule,
    ToastModule,
    TooltipModule
  ],
  templateUrl: './journal-entry-list.component.html',
  styleUrl: './journal-entry-list.component.css',
  providers: [MessageService, ConfirmationService]
})
export class JournalEntryListComponent implements OnInit {
  entries: JournalEntryDto[] = [];
  loading: boolean = true;
  totalRecords: number = 0;
  Math = Math;

  // Dialog state
  displayAddDialog: boolean = false;
  displayViewDialog: boolean = false;
  selectedEntry: JournalEntryDto | null = null;
  accounts: AccountDto[] = [];
  newEntry: any = {
    description: '',
    entryDate: new Date(),
    lines: [
      { accountId: null, debit: 0, credit: 0, description: '' },
      { accountId: null, debit: 0, credit: 0, description: '' }
    ]
  };

  constructor(
    private accountingService: AccountingService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.loadEntries();
    this.loadAccounts();
  }

  loadEntries(page: number = 1): void {
    this.loading = true;
    this.accountingService.getJournalEntries(page, 10)
      .pipe(
        catchError(err => {
          console.error('Failed to load journal entries:', err);
          return of({ items: [], totalCount: 0 });
        }),
        finalize(() => this.loading = false)
      )
      .subscribe(res => {
        this.entries = res.items;
        this.totalRecords = res.totalCount;
      });
  }

  loadAccounts(): void {
    this.accountingService.getAccountsTree().subscribe(data => {
      // Flatten tree for dropdown
      this.accounts = this.flattenAccounts(data);
    });
  }

  private flattenAccounts(accounts: AccountDto[]): AccountDto[] {
    let result: AccountDto[] = [];
    accounts.forEach(acc => {
      result.push(acc);
      if (acc.children) {
        result = result.concat(this.flattenAccounts(acc.children));
      }
    });
    return result;
  }

  showAddDialog(): void {
    this.newEntry = {
      description: '',
      entryDate: new Date(),
      lines: [
        { accountId: null, debit: 0, credit: 0, description: '' },
        { accountId: null, debit: 0, credit: 0, description: '' }
      ]
    };
    this.displayAddDialog = true;
  }

  addLine(): void {
    this.newEntry.lines.push({ accountId: null, debit: 0, credit: 0, description: '' });
  }

  removeLine(index: number): void {
    if (this.newEntry.lines.length > 2) {
      this.newEntry.lines.splice(index, 1);
    }
  }

  getTotalDebit(): number {
    return this.newEntry.lines.reduce((sum: number, l: any) => sum + (l.debit || 0), 0);
  }

  getTotalCredit(): number {
    return this.newEntry.lines.reduce((sum: number, l: any) => sum + (l.credit || 0), 0);
  }

  saveEntry(): void {
    const totalDebit = this.getTotalDebit();
    const totalCredit = this.getTotalCredit();

    if (Math.abs(totalDebit - totalCredit) > 0.01) {
      this.messageService.add({ severity: 'error', summary: 'خطأ في التوازن', detail: 'مجموع المدين يجب أن يساوي مجموع الدائن' });
      return;
    }

    if (!this.newEntry.description) {
      this.messageService.add({ severity: 'warn', summary: 'نقص بيانات', detail: 'يرجى كتابة وصف للقيد' });
      return;
    }

    this.accountingService.createJournalEntry(this.newEntry).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم حفظ القيد بنجاح' });
        this.displayAddDialog = false;
        this.loadEntries();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في حفظ القيد' });
      }
    });
  }

  postEntry(id: number): void {
    this.accountingService.postJournalEntry(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم الترحيل', detail: 'تم ترحيل القيد للمحاسبة بنجاح' });
        this.loadEntries();
      }
    });
  }

  viewEntry(entry: JournalEntryDto): void {
    this.selectedEntry = entry;
    this.displayViewDialog = true;
  }

  formatCurrency(value: number): string {
    if (!value) return '-';
    const num = new Intl.NumberFormat('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(value);
    return `${num} ر.ي.`;
  }

  onFocus(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target) {
      target.select();
    }
  }
}
