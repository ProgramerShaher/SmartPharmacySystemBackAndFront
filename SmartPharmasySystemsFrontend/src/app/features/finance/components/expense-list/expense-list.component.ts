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
import { CardModule } from 'primeng/card';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ExpenseService } from '../../services/expense.service';
import { ExpenseCategoryService } from '../../services/expense-category.service';
import { ExpenseDto, ExpenseQueryDto, PagedResult, ExpenseCategoryDto, PaymentType } from '../../../../core/models';
import {
  getPaymentMethodLabel,
  getPaymentMethodSeverity,
  getPaidStatusLabel,
  getPaidStatusSeverity,
  formatExpenseAmount,
  formatDate
} from '../../utils/expense.utils';
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
    CardModule,
    SelectButtonModule
  ],
  templateUrl: './expense-list.component.html',
  styleUrl: './expense-list.component.css',
  providers: [MessageService, ConfirmationService]
})
export class ExpenseListComponent implements OnInit {
  expenses: ExpenseDto[] = [];
  categories: ExpenseCategoryDto[] = [];
  loading: boolean = true;

  // Filters
  searchTerm: string = '';
  selectedCategory: number | null = null;
  selectedPaymentMethod: PaymentType | null = null;
  selectedPaidStatus: boolean | null = null;
  startDate: Date | null = null;
  endDate: Date | null = null;

  // Dropdown options
  categoryOptions: any[] = [];
  paymentMethodOptions = [
    { label: 'الكل', value: null },
    { label: 'نقدي', value: PaymentType.Cash },
    { label: 'آجل', value: PaymentType.Credit }
  ];
  paidStatusOptions = [
    { label: 'الكل', value: null },
    { label: 'مدفوع', value: true },
    { label: 'غير مدفوع', value: false }
  ];

  // Pagination
  totalRecords: number = 0;
  pageSize: number = 10;
  currentPage: number = 1;

  // Utility functions
  getPaymentMethodLabel = getPaymentMethodLabel;
  getPaymentMethodSeverity = getPaymentMethodSeverity;
  getPaidStatusLabel = getPaidStatusLabel;
  getPaidStatusSeverity = getPaidStatusSeverity;
  formatExpenseAmount = formatExpenseAmount;
  formatDate = formatDate;

  constructor(
    private expenseService: ExpenseService,
    private categoryService: ExpenseCategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadCategories();
    this.loadExpenses();
  }

  loadCategories(): void {
    this.categoryService.getAll()
      .pipe(
        catchError(() => {
          console.error('Failed to load categories');
          return of([]);
        })
      )
      .subscribe(categories => {
        this.categories = categories;
        this.categoryOptions = [
          { label: 'كل الفئات', value: null },
          ...categories.map(c => ({ label: c.name, value: c.id }))
        ];
      });
  }

  loadExpenses(page: number = 1): void {
    this.loading = true;
    this.currentPage = page;

    const query: ExpenseQueryDto = {
      page: page,
      pageSize: this.pageSize,
      search: this.searchTerm || undefined,
      categoryId: this.selectedCategory || undefined,
      paymentMethod: this.selectedPaymentMethod !== null ? this.selectedPaymentMethod : undefined,
      isPaid: this.selectedPaidStatus !== null ? this.selectedPaidStatus : undefined,
      fromDate: this.startDate ? this.startDate.toISOString().split('T')[0] : undefined,
      toDate: this.endDate ? this.endDate.toISOString().split('T')[0] : undefined
    };

    this.expenseService.search(query)
      .pipe(
        catchError((error) => {
          console.error('Failed to load expenses:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل في تحميل بيانات المصروفات'
          });
          return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 } as PagedResult<ExpenseDto>);
        }),
        finalize(() => this.loading = false)
      )
      .subscribe((result: PagedResult<ExpenseDto>) => {
        this.expenses = result.items;
        this.totalRecords = result.totalCount;
      });
  }

  onSearch(): void {
    this.loadExpenses(1);
  }

  onFilterChange(): void {
    this.loadExpenses(1);
  }

  onPageChange(event: any): void {
    this.loadExpenses(event.page + 1);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = null;
    this.selectedPaymentMethod = null;
    this.selectedPaidStatus = null;
    this.startDate = null;
    this.endDate = null;
    this.loadExpenses(1);
  }

  addNewExpense(): void {
    this.router.navigate(['/finance/expenses/add']);
  }

  editExpense(expense: ExpenseDto): void {
    this.router.navigate(['/finance/expenses/edit', expense.id]);
  }

  viewDetails(expense: ExpenseDto): void {
    this.router.navigate(['/finance/expenses', expense.id]);
  }

  deleteExpense(event: Event, expense: ExpenseDto): void {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المصروف "${expense.categoryName}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.expenseService.delete(expense.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف المصروف بنجاح'
            });
            this.loadExpenses(this.currentPage);
          },
          error: (err) => {
            console.error('Failed to delete expense:', err);
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في عملية الحذف'
            });
          }
        });
      }
    });
  }

  getTotalExpenses(): number {
    return this.expenses.reduce((sum, exp) => sum + exp.amount, 0);
  }

  getPaidExpenses(): number {
    return this.expenses.filter(exp => exp.isPaid).reduce((sum, exp) => sum + exp.amount, 0);
  }

  getUnpaidExpenses(): number {
    return this.expenses.filter(exp => !exp.isPaid).reduce((sum, exp) => sum + exp.amount, 0);
  }
}
