import { Component, OnInit, signal } from '@angular/core';
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
import { SettingsService } from '../../../../core/services/settings.service';
import { PharmacySettings } from '../../../../core/models/settings/pharmacy-settings.interface';
import { environment } from '../../../../../environments/environment';
import {
  getPaymentMethodLabel,
  getPaymentMethodSeverity,
  getPaidStatusLabel,
  getPaidStatusSeverity,
  formatExpenseAmount,
  formatExpenseAmountNumber,
  formatDate,
  convertNumberToArabicWords
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
  selectedExpenseForPrint: ExpenseDto | null = null;

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

  pharmacySettings = signal<PharmacySettings>({
    id: 0,
    pharmacyName: 'صيدلية الشفاء العالمية',
    taxNumber: '300123456700003',
    phoneNumber: '011234567',
    address: 'الرياض، المملكة العربية السعودية',
    baseCurrency: 'ر.ي'
  });
  readonly serverUrl = environment.apiUrl.replace('/api', '');

  // Utility functions
  getPaymentMethodLabel = getPaymentMethodLabel;
  getPaymentMethodSeverity = getPaymentMethodSeverity;
  getPaidStatusLabel = getPaidStatusLabel;
  getPaidStatusSeverity = getPaidStatusSeverity;
  formatExpenseAmount = formatExpenseAmount;
  formatExpenseAmountNumber = formatExpenseAmountNumber;
  formatDate = formatDate;
  convertNumberToArabicWords = convertNumberToArabicWords;

  constructor(
    private expenseService: ExpenseService,
    private categoryService: ExpenseCategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router,
    private settingsService: SettingsService
  ) { }

  ngOnInit(): void {
    this.loadCategories();
    this.loadExpenses();
    this.settingsService.getSettings().subscribe({
      next: (settings) => {
        if (settings) {
          this.pharmacySettings.set(settings);
        }
      }
    });
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

  printVoucher(expense: ExpenseDto): void {
    const dateObj = new Date(expense.expenseDate);
    const formattedDate = dateObj.toLocaleDateString('ar-YE');
    const formattedTime = dateObj.getHours() === 0 && dateObj.getMinutes() === 0
      ? new Date().toLocaleTimeString('ar-YE', { hour: '2-digit', minute: '2-digit' })
      : dateObj.toLocaleTimeString('ar-YE', { hour: '2-digit', minute: '2-digit' });

    const translatedMethod = expense.paymentMethod ? 'نقدي (من الخزينة)' : 'آجل (قيد استحقاق)';
    const statusText = expense.isPaid ? 'مـدفـوع' : 'آجـل غير مدفوع';
    const amountWords = 'فقط ' + this.convertNumberToArabicWords(expense.amount) + ' ريال يمني لا غير.';

    const logoMarkup = this.pharmacySettings().logoUrl
      ? `<div class="pharmacy-logo"><img src="${this.serverUrl}${this.pharmacySettings().logoUrl}" alt="${this.pharmacySettings().pharmacyName}"></div>`
      : '';

    const printContents = `
      <div class="voucher-container expense-voucher" dir="rtl">
        <!-- Header -->
        <div class="voucher-header">
            <div class="pharmacy-info">
                ${logoMarkup}
                <h2>${this.pharmacySettings().pharmacyName}</h2>
                <p>الرقم الضريبي: ${this.pharmacySettings().taxNumber || '---'}</p>
                <p>الهاتف: ${this.pharmacySettings().phoneNumber || this.pharmacySettings().mobileNumber || '---'}</p>
                <p>العنوان: ${this.pharmacySettings().address || '---'}</p>
            </div>
            <div class="voucher-title">
                <h1>سند صرف مصروفات</h1>
                <span class="status-badge ${expense.isPaid ? 'paid' : 'unpaid'}">${statusText}</span>
            </div>
            <div class="meta-data">
                <p><b>رقم السند:</b> EXP-${expense.id}</p>
                <p><b>التاريخ:</b> ${formattedDate}</p>
                <p><b>الوقت:</b> ${formattedTime}</p>
            </div>
        </div>

        <!-- Body -->
        <div class="voucher-body">
            <div class="amount-box">
                <div>
                    <span class="field-label">المبلغ رقماً:</span>
                    <span class="amount-num">${expense.amount.toLocaleString()} ر.ي</span>
                </div>
                <div>
                    <span class="field-label">طريقة الصرف:</span>
                    <span class="field-value">${translatedMethod}</span>
                </div>
            </div>

            <div class="row-grid">
                <div class="field-group">
                    <span class="field-label">صرفنا للمكرم/ة:</span>
                    <span class="field-value">${expense.categoryName}</span>
                </div>
                <div class="field-group">
                    <span class="field-label">بند المصروف:</span>
                    <span class="field-value">EXP-CAT-${expense.categoryId || expense.id}</span>
                </div>
            </div>

            <div class="field-group" style="margin-bottom: 15px;">
                <span class="field-label">مبلغ وقدره كتابةً:</span>
                <span class="field-value">${amountWords}</span>
            </div>

            <div class="row-grid">
                <div class="field-group">
                    <span class="field-label">رقم العملية/المرجع:</span>
                    <span class="field-value">TRX-${expense.id}</span>
                </div>
                <div class="field-group">
                    <span class="field-label">مركز التكلفة:</span>
                    <span class="field-value">مركز المصروفات التشغيلية</span>
                </div>
            </div>

            <!-- Details Table -->
            <table class="details-table">
                <thead>
                    <tr>
                        <th style="width: 70%;">البيان / الغرض من الصرف والتسوية</th>
                        <th style="width: 30%;">المبلغ</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>${expense.notes || 'سداد وإثبات مصروفات تشغيلية للصيدلية'}</td>
                        <td>${expense.amount.toLocaleString()} ر.ي</td>
                    </tr>
                </tbody>
            </table>

            <div class="field-group">
                <span class="field-label">ملاحظات النظام:</span>
                <span class="field-value">تم قيد المصروف بنجاح وتحديث حركة الصندوق المالي آلياً.</span>
            </div>
        </div>

        <!-- Footer Signatures -->
        <div class="voucher-footer">
            <div class="signature-block">
                <div class="signature-title">المستلم (المستفيد)</div>
                <div class="signature-line"></div>
            </div>
            <div class="signature-block">
                <div class="signature-title">أمين الصندوق</div>
                <div class="signature-line">أمين الصندوق</div>
            </div>
            <div class="signature-block">
                <div class="signature-title">المحاسب المالي</div>
                <div class="signature-line"></div>
            </div>
            <div class="signature-block">
                <div class="signature-title">الاعتماد (المدير المالي)</div>
                <div class="signature-line"></div>
            </div>
        </div>
      </div>
    `;

    const popupWin = window.open('', '_blank', 'top=0,left=0,height=700,width=850');
    if (popupWin) {
      popupWin.document.open();
      popupWin.document.write(`
        <html>
          <head>
            <title>طباعة سند مصروف #${expense.id}</title>
            <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700&display=swap" rel="stylesheet">
            <style>
              :root {
                --primary-color: #b91c1c;
                --secondary-color: #2e7d32;
                --dark-color: #333333;
                --light-bg: #f8f9fa;
                --border-color: #cccccc;
              }
              body {
                font-family: 'Cairo', sans-serif;
                background-color: #ffffff;
                color: var(--dark-color);
                margin: 0;
                padding: 20px;
                font-size: 14px;
                direction: rtl;
              }
              .voucher-container {
                background: #ffffff;
                max-width: 800px;
                margin: 0 auto;
                padding: 30px;
                border: 2px solid var(--border-color);
                border-radius: 8px;
                position: relative;
              }
              .expense-voucher { border-top: 8px solid var(--primary-color); }
              .voucher-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                border-bottom: 2px dashed var(--border-color);
                padding-bottom: 15px;
                margin-bottom: 20px;
              }
              .pharmacy-info h2 {
                margin: 0 0 5px 0;
                color: var(--dark-color);
                font-size: 18px;
              }
              .pharmacy-logo {
                width: 64px;
                height: 64px;
                margin-bottom: 8px;
                border-radius: 14px;
                overflow: hidden;
                border: 1px solid #ddd;
                background: #fff;
              }
              .pharmacy-logo img {
                width: 100%;
                height: 100%;
                object-fit: cover;
                display: block;
              }
              .pharmacy-info p {
                margin: 2px 0;
                font-size: 11px;
                color: #666;
              }
              .voucher-title {
                text-align: center;
              }
              .voucher-title h1 {
                margin: 0;
                font-size: 22px;
                padding: 5px 25px;
                border: 2px solid var(--primary-color);
                border-radius: 4px;
                background-color: var(--light-bg);
                color: var(--primary-color);
              }
              .status-badge {
                display: inline-block;
                padding: 2px 10px;
                border-radius: 12px;
                font-size: 11px;
                font-weight: bold;
                margin-top: 5px;
              }
              .status-badge.paid {
                background: #fee2e2;
                color: var(--primary-color);
                border: 1px solid rgba(185,28,28,0.3);
              }
              .status-badge.unpaid {
                background: #fef3c7;
                color: #d97706;
                border: 1px solid rgba(217,119,6,0.3);
              }
              .meta-data p {
                margin: 4px 0;
                font-size: 12px;
              }
              .voucher-body {
                margin-bottom: 20px;
              }
              .row-grid {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 15px;
                margin-bottom: 15px;
              }
              .field-group {
                display: flex;
                align-items: center;
                border-bottom: 1px dashed #bbb;
                padding-bottom: 5px;
              }
              .field-label {
                font-weight: 600;
                min-width: 120px;
                color: #444;
              }
              .field-value {
                flex-grow: 1;
                font-weight: 400;
              }
              .amount-box {
                background-color: var(--light-bg);
                border: 1px solid var(--border-color);
                padding: 10px 15px;
                border-radius: 4px;
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 15px;
              }
              .amount-num {
                font-size: 18px;
                font-weight: 700;
                color: var(--primary-color);
              }
              .details-table {
                width: 100%;
                border-collapse: collapse;
                margin: 20px 0;
              }
              .details-table th, .details-table td {
                border: 1px solid var(--border-color);
                padding: 10px;
                text-align: right;
              }
              .details-table th {
                background-color: var(--light-bg);
                font-weight: 600;
              }
              .voucher-footer {
                margin-top: 40px;
                display: flex;
                justify-content: space-between;
                text-align: center;
              }
              .signature-block {
                width: 23%;
              }
              .signature-line {
                margin-top: 35px;
                border-top: 1px solid #999;
                font-size: 12px;
                color: #555;
              }
              @media print {
                body { padding: 0; }
                .voucher-container { border: 1px solid #000; }
              }
            </style>
          </head>
          <body onload="window.print();window.close()">
            ${printContents}
          </body>
        </html>
      `);
      popupWin.document.close();
    }
  }
}

