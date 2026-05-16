import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TreeModule } from 'primeng/tree';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TreeNode, MessageService, ConfirmationService } from 'primeng/api';
import { AccountingService } from '../../../../core/services/accounting.service';
import { AccountDto, AccountType } from '../../../../core/models/accounting.interface';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-chart-of-accounts',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TreeModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    DropdownModule,
    TooltipModule,
    InputSwitchModule
  ],
  templateUrl: './chart-of-accounts.component.html',
  styleUrl: './chart-of-accounts.component.css',
  providers: [MessageService, ConfirmationService]
})
export class ChartOfAccountsComponent implements OnInit {
  accountsTree: TreeNode[] = [];
  loading: boolean = true;

  // Dialog state
  displayAddDialog: boolean = false;
  newAccount: any = {
    code: '',
    name: '',
    accountType: null,
    parentId: null
  };

  accountTypeOptions = [
    { label: 'أصول (Assets)', value: AccountType.Asset },
    { label: 'خصوم (Liability)', value: AccountType.Liability },
    { label: 'حقوق ملكية (Equity)', value: AccountType.Equity },
    { label: 'إيرادات (Revenue)', value: AccountType.Revenue },
    { label: 'مصروفات (Expense)', value: AccountType.Expense }
  ];

  constructor(
    private accountingService: AccountingService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.loading = true;
    this.accountingService.getAccountsTree()
      .pipe(
        catchError(err => {
          console.error('Failed to load accounts:', err);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل في تحميل شجرة الحسابات'
          });
          return of([]);
        }),
        finalize(() => this.loading = false)
      )
      .subscribe(accounts => {
        this.accountsTree = this.mapToTreeNodes(accounts);
      });
  }

  expandAll(): void {
    const temp = [...this.accountsTree];
    temp.forEach(node => {
      this.expandRecursive(node, true);
    });
    this.accountsTree = temp;
  }

  collapseAll(): void {
    const temp = [...this.accountsTree];
    temp.forEach(node => {
      this.expandRecursive(node, false);
    });
    this.accountsTree = temp;
  }

  private expandRecursive(node: TreeNode, isExpand: boolean): void {
    node.expanded = isExpand;
    if (node.children) {
      node.children.forEach(childNode => {
        this.expandRecursive(childNode, isExpand);
      });
    }
  }

  private mapToTreeNodes(accounts: AccountDto[]): TreeNode[] {
    return accounts.map(acc => ({
      label: acc.name,
      data: acc,
      expandedIcon: 'pi pi-folder-open',
      collapsedIcon: 'pi pi-folder',
      icon: acc.isMainAccount ? 'pi pi-folder' : 'pi pi-file',
      children: acc.children ? this.mapToTreeNodes(acc.children) : [],
      expanded: false
    }));
  }

  showAddDialog(parent?: any): void {
    this.newAccount = {
      code: '',
      name: '',
      accountType: parent ? parent.accountType : null,
      parentId: parent ? parent.id : null
    };
    this.displayAddDialog = true;
  }

  saveAccount(): void {
    if (!this.newAccount.name || !this.newAccount.code) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إكمال البيانات المطلوبة' });
      return;
    }

    this.accountingService.createAccount(this.newAccount).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم إضافة الحساب بنجاح' });
        this.displayAddDialog = false;
        this.loadAccounts();
      },
      error: (err: any) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في إضافة الحساب' });
      }
    });
  }

  formatCurrency(value: number): string {
    if (value === null || value === undefined) return '-';
    const num = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    return `${num} ر.ي.`;
  }

  getTypeNameArabic(type: string | number): string {
    if (type === null || type === undefined) return 'غير محدد';

    const typeStr = type.toString().toLowerCase();
    switch (typeStr) {
      case '1':
      case 'asset': return 'أصول';
      case '2':
      case 'liability': return 'خصوم';
      case '3':
      case 'equity': return 'حقوق ملكية';
      case '4':
      case 'revenue': return 'إيرادات';
      case '5':
      case 'expense': return 'مصروفات';
      default: return typeStr;
    }
  }

  toggleAccountStatus(rowData: any): void {
    const originalStatus = !rowData.isActive;
    this.accountingService.toggleAccountStatus(rowData.id, rowData.isActive).subscribe({
      next: (res: any) => {
        this.messageService.add({
          severity: 'success',
          summary: 'نجاح',
          detail: res.message || (rowData.isActive ? 'تم تفعيل الحساب' : 'تم تعطيل الحساب')
        });
      },
      error: (err: any) => {
        // إعادة الحالة للوضع الأصلي في حال فشل الـ Backend (مثلاً لوجود رصيد)
        rowData.isActive = originalStatus;
        const errMsg = err.error?.message || 'حدث خطأ أثناء تغيير حالة الحساب';
        this.messageService.add({ severity: 'error', summary: 'عذراً (حماية البيانات)', detail: errMsg });
      }
    });
  }

  deleteAccount(node: TreeNode): void {
    const account = node.data;
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الحساب "${account.name}"؟ لا يمكن التراجع عن هذه العملية إذا تمت.`,
      header: 'تأكيد الحذف النهائي',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، احذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.accountingService.deleteAccount(account.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف الحساب بنجاح من الشجرة' });
            this.loadAccounts();
          },
          error: (err: any) => {
            const errMsg = err.error?.message || 'فشل حذف الحساب. قد يكون مرتبطاً بحركات مالية أو حسابات فرعية.';
            this.messageService.add({ severity: 'error', summary: 'فشل الحذف', detail: errMsg });
          }
        });
      }
    });
  }
}