import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FinanceService } from '../../services/finance.service';
import { Expense, ExpenseQueryDto, PagedResult } from '../../../../core/models';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-expense-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    CalendarModule,
    DropdownModule,
    ToastModule,
    ConfirmDialogModule,
    ProgressSpinnerModule,
    TooltipModule,
    TagModule,
  ],
  templateUrl: './expense-list.component.html',
  styleUrl: './expense-list.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ExpenseListComponent implements OnInit {
  expenses: Expense[] = [];
  loading: boolean = true;
  searchTerm: string = '';
  expenseTypeFilter: string = '';
  startDate: Date | null = null;
  endDate: Date | null = null;

  expenseTypeOptions = [
    { label: 'الكل', value: '' },
    { label: 'إيجار المحل', value: 'إيجار المحل' },
    { label: 'مرتبات', value: 'مرتبات' },
    { label: 'فواتير', value: 'فواتير' },
    { label: 'مصاريف تشغيلية', value: 'مصاريف تشغيلية' },
    { label: 'أخرى', value: 'أخرى' },
  ];

  totalRecords: number = 0;
  pageSize: number = 10;
  currentPage: number = 1;

  constructor(
    private financeService: FinanceService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    console.log('🚀 Expense List Component Initialized');
    this.loadExpenses();
  }

  loadExpenses(page: number = 1): void {
    this.loading = true;
    this.currentPage = page;
    console.log('⏳ Loading expenses list...');
    console.log(`🔍 Search term: "${this.searchTerm}"`);
    console.log(`📋 Expense type filter: "${this.expenseTypeFilter}"`);
    console.log(`📅 Date range: ${this.startDate} - ${this.endDate}`);

    const query: ExpenseQueryDto = {
      page: page,
      pageSize: this.pageSize,
    };

    if (this.searchTerm && this.searchTerm.trim() !== '') {
      query.search = this.searchTerm.trim();
    }
    if (this.expenseTypeFilter && this.expenseTypeFilter.trim() !== '') {
      query.expenseType = this.expenseTypeFilter.trim();
    }
    if (this.startDate) {
      query.startDate = this.startDate.toISOString().split('T')[0];
    }
    if (this.endDate) {
      query.endDate = this.endDate.toISOString().split('T')[0];
    }

    console.log('📤 Query being sent:', query);

    this.financeService
      .search(query)
      .pipe(
        catchError((error) => {
          console.error('❌ Failed to load expenses:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: error.error?.message || 'فشل في تحميل بيانات المصروفات',
          });
          this.expenses = [];
          return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 });
        }),
        finalize(() => {
          this.loading = false;
          console.log(
            `✅ Expenses list loaded: ${this.expenses.length} expenses found`
          );
        })
      )
      .subscribe({
        next: (result: PagedResult<Expense>) => {
          console.log('📥 Received result:', result);
          if (result && result.items) {
            this.expenses = result.items;
            this.totalRecords = result.totalCount;
            console.log(
              `📊 Loaded ${result.items.length} expenses out of ${result.totalCount} total`
            );
          } else {
            console.warn('⚠️ No items in result, setting empty array');
            this.expenses = [];
            this.totalRecords = 0;
          }
        }
      });
  }

  onSearch(): void {
    console.log(`🔍 Performing search with term: "${this.searchTerm}"`);
    this.loadExpenses(1); // Reset to first page
  }

  onExpenseTypeFilterChange(): void {
    console.log(`📋 Expense type filter changed to: "${this.expenseTypeFilter}"`);
    this.loadExpenses(1); // Reset to first page
  }

  onDateRangeChange(): void {
    console.log(`📅 Date range changed: ${this.startDate} - ${this.endDate}`);
    this.loadExpenses(1); // Reset to first page
  }

  onPageChange(event: any): void {
    console.log(`📄 Page changed to: ${event.page + 1}`);
    this.loadExpenses(event.page + 1);
  }

  viewDetails(expense: Expense): void {
    console.log(`👁️ Viewing details for expense ID: ${expense.id}`);
    // Could navigate to detail view if implemented
    this.messageService.add({
      severity: 'info',
      summary: 'معلومات',
      detail: `عرض تفاصيل المصروف: ${expense.expenseType}`,
    });
  }

  editExpense(expense: Expense): void {
    console.log(`✏️ Editing expense ID: ${expense.id}`);
    this.router.navigate(['finance', 'edit', expense.id]);
  }

  deleteExpense(event: Event, expense: Expense): void {
    console.log(
      `🗑️ Delete requested for expense: ${expense.expenseType} (ID: ${expense.id})`
    );

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المصروف "${expense.expenseType}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        console.log(`✅ User confirmed deletion of expense ID: ${expense.id}`);
        this.financeService.delete(expense.id).subscribe({
          next: () => {
            console.log(`🎉 Expense ID ${expense.id} deleted successfully`);
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف المصروف بنجاح',
            });
            console.log('🔄 Reloading expenses list after deletion');
            this.loadExpenses(this.currentPage);
          },
          error: (err) => {
            console.error(`❌ Failed to delete expense ID ${expense.id}:`, err);
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في عملية الحذف',
            });
          },
        });
      },
      reject: () => {
        console.log('❌ User cancelled deletion');
      },
    });
  }

  addNewExpense(): void {
    console.log('➕ Adding new expense');
    this.router.navigate(['finance', 'add']);
  }

  getPaymentMethodLabel(paymentMethod: string): string {
    switch (paymentMethod) {
      case 'نقدا':
        return 'نقدي';
      case 'شيك':
        return 'شيك';
      case 'تحويل مصرفي':
        return 'تحويل مصرفي';
      case 'بطاقة ائتمان':
        return 'بطاقة ائتمان';
      default:
        return paymentMethod;
    }
  }

  getPaymentMethodSeverity(paymentMethod: string): 'success' | 'info' | 'warning' | 'danger' {
    switch (paymentMethod) {
      case 'نقدا':
        return 'success';
      case 'شيك':
        return 'warning';
      case 'تحويل مصرفي':
        return 'info';
      case 'بطاقة ائتمان':
        return 'danger';
      default:
        return 'info';
    }
  }
}
