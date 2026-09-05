import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../auth/services/auth.service';

export interface QuickAction {
  id: string;
  title: string;
  subtitle: string;
  icon: string;
  category: 'sales' | 'purchases' | 'financial' | 'inventory' | 'reports';
  categoryLabel: string;
  badge?: string;
  route: string;
  queryParams?: Record<string, any>;
  colorTheme: string; // CSS class for distinct theme
}

@Component({
  selector: 'app-home-hub',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    InputTextModule,
    ButtonModule,
    TooltipModule
  ],
  templateUrl: './home-hub.component.html',
  styleUrls: ['./home-hub.component.scss']
})
export class HomeHubComponent implements OnInit {
  userName = signal<string>('المستخدم');
  userRole = signal<string>('مدير النظام');
  branchName = signal<string>('الفرع الرئيسي');
  searchQuery = signal<string>('');

  allActions: QuickAction[] = [
    // 🟢 مبيعات وكاشير (كل صندوق لون خاص به)
    {
      id: 'pos',
      title: 'شاشة الكاشير (POS)',
      subtitle: 'بيع سريع وفوري',
      icon: 'pi pi-shopping-cart',
      category: 'sales',
      categoryLabel: 'عمليات البيع والكاشير',
      badge: 'سريع',
      route: '/sales/create',
      colorTheme: 'box-emerald'
    },
    {
      id: 'sales-list',
      title: 'فواتير المبيعات',
      subtitle: 'تصفح كافة الفواتير',
      icon: 'pi pi-receipt',
      category: 'sales',
      categoryLabel: 'عمليات البيع والكاشير',
      route: '/sales',
      colorTheme: 'box-cyan'
    },
    {
      id: 'sales-return',
      title: 'مرتجع المبيعات',
      subtitle: 'إرجاع أصناف مباعة',
      icon: 'pi pi-refresh',
      category: 'sales',
      categoryLabel: 'عمليات البيع والكاشير',
      route: '/sales/returns/create',
      colorTheme: 'box-rose'
    },
    {
      id: 'daily-closing',
      title: 'إغلاق الوردية واليومي',
      subtitle: 'تسوية الصندوق والجرد',
      icon: 'pi pi-lock',
      category: 'sales',
      categoryLabel: 'عمليات البيع والكاشير',
      badge: 'إغلاق',
      route: '/sales/daily-closing',
      colorTheme: 'box-amber'
    },

    // 🔵 مشتريات وتوريد
    {
      id: 'purchase-create',
      title: 'فاتورة توريد جديدة',
      subtitle: 'إدخال مشتريات جديدة',
      icon: 'pi pi-truck',
      category: 'purchases',
      categoryLabel: 'المشتريات والتوريد',
      badge: 'توريد',
      route: '/purchases/create',
      colorTheme: 'box-sky'
    },
    {
      id: 'purchase-list',
      title: 'فواتير المشتريات',
      subtitle: 'سجل التوريدات السابقة',
      icon: 'pi pi-shopping-bag',
      category: 'purchases',
      categoryLabel: 'المشتريات والتوريد',
      route: '/purchases',
      colorTheme: 'box-indigo'
    },
    {
      id: 'purchase-return',
      title: 'مرتجع المشتريات',
      subtitle: 'إعادة أصناف للمورد',
      icon: 'pi pi-replay',
      category: 'purchases',
      categoryLabel: 'المشتريات والتوريد',
      route: '/purchases/returns/create',
      colorTheme: 'box-purple'
    },

    // 🟣 السندات والمالية
    {
      id: 'payment-voucher',
      title: 'إنشاء سند صرف',
      subtitle: 'صرف مالي للموردين',
      icon: 'pi pi-arrow-up-right',
      category: 'financial',
      categoryLabel: 'السندات والمالية',
      badge: 'صرف',
      route: '/partners/suppliers/payments',
      queryParams: { create: 'true' },
      colorTheme: 'box-ruby'
    },
    {
      id: 'receipt-voucher',
      title: 'إنشاء سند قبض',
      subtitle: 'استلام مالي من عميل',
      icon: 'pi pi-arrow-down-left',
      category: 'financial',
      categoryLabel: 'السندات والمالية',
      badge: 'قبض',
      route: '/customers/receipts',
      queryParams: { create: 'true' },
      colorTheme: 'box-sapphire'
    },
    {
      id: 'expense-add',
      title: 'تسجيل مصروف',
      subtitle: 'المصروفات النثرية والعمومية',
      icon: 'pi pi-minus-circle',
      category: 'financial',
      categoryLabel: 'السندات والمالية',
      route: '/finance/expenses/add',
      colorTheme: 'box-fuchsia'
    },
    {
      id: 'financial-ledger',
      title: 'دفتر الأستاذ والسيولة',
      subtitle: 'حركة الصناديق والبنوك',
      icon: 'pi pi-wallet',
      category: 'financial',
      categoryLabel: 'السندات والمالية',
      route: '/financial/ledger',
      colorTheme: 'box-teal'
    },
    {
      id: 'chart-accounts',
      title: 'شجرة الحسابات',
      subtitle: 'دليل الحسابات المالي',
      icon: 'pi pi-sitemap',
      category: 'financial',
      categoryLabel: 'السندات والمالية',
      route: '/accounting/chart',
      colorTheme: 'box-forest'
    },

    // 🟠 المخزون والأدوية
    {
      id: 'medicines-list',
      title: 'الأدوية والمخزون',
      subtitle: 'دليل الأصناف والأسعار',
      icon: 'pi pi-box',
      category: 'inventory',
      categoryLabel: 'إدارة المخزون والأدوية',
      route: '/inventory/medicines',
      colorTheme: 'box-orange'
    },
    {
      id: 'inventory-valuation',
      title: 'تقييم المخزون',
      subtitle: 'تقرير قيمة الأصناف بالجملة',
      icon: 'pi pi-calculator',
      category: 'inventory',
      categoryLabel: 'إدارة المخزون والأدوية',
      route: '/reports/inventory-valuation',
      colorTheme: 'box-bronze'
    },
    {
      id: 'alerts-list',
      title: 'النواقص والصلاحية',
      subtitle: 'تنبيهات الانتهاء والحد الأدنى',
      icon: 'pi pi-exclamation-triangle',
      category: 'inventory',
      categoryLabel: 'إدارة المخزون والأدوية',
      badge: 'تنبيه',
      route: '/system-alerts',
      colorTheme: 'box-coral'
    },
    {
      id: 'stock-transfers',
      title: 'تحويلات المخازن',
      subtitle: 'نقل الأصناف بين الفروع',
      icon: 'pi pi-arrow-right-arrow-left',
      category: 'inventory',
      categoryLabel: 'إدارة المخزون والأدوية',
      route: '/warehouses/transfers',
      colorTheme: 'box-cyan-electric'
    },

    // 📊 التقارير والشركاء
    {
      id: 'daily-sales-report',
      title: 'المبيعات اليومية',
      subtitle: 'ملخص إيرادات اليوم',
      icon: 'pi pi-chart-line',
      category: 'reports',
      categoryLabel: 'التقارير والشركاء',
      route: '/reports/daily-sales',
      colorTheme: 'box-mint'
    },
    {
      id: 'net-profit-report',
      title: 'تقرير صافي الأرباح',
      subtitle: 'تحليل هامش الربح والسيولة',
      icon: 'pi pi-chart-bar',
      category: 'reports',
      categoryLabel: 'التقارير والشركاء',
      route: '/reports/net-profit',
      colorTheme: 'box-violet'
    },
    {
      id: 'customers-list',
      title: 'العملاء والديون',
      subtitle: 'سجلات العملاء والمديونيات',
      icon: 'pi pi-users',
      category: 'reports',
      categoryLabel: 'التقارير والشركاء',
      route: '/customers',
      colorTheme: 'box-cerulean'
    },
    {
      id: 'suppliers-list',
      title: 'الموردين والشركاء',
      subtitle: 'سجلات الموردين والمدفوعات',
      icon: 'pi pi-briefcase',
      category: 'reports',
      categoryLabel: 'التقارير والشركاء',
      route: '/partners/suppliers',
      colorTheme: 'box-slate'
    }
  ];

  filteredActions = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    if (!query) return this.allActions;
    return this.allActions.filter(a =>
      a.title.toLowerCase().includes(query) ||
      a.subtitle.toLowerCase().includes(query) ||
      a.categoryLabel.toLowerCase().includes(query)
    );
  });

  categories = [
    { key: 'sales', title: 'عمليات البيع والكاشير', icon: 'pi pi-shopping-bag' },
    { key: 'purchases', title: 'المشتريات والتوريد', icon: 'pi pi-truck' },
    { key: 'financial', title: 'السندات والمالية', icon: 'pi pi-wallet' },
    { key: 'inventory', title: 'المخزون والأدوية', icon: 'pi pi-box' },
    { key: 'reports', title: 'التقارير والشركاء', icon: 'pi pi-chart-line' }
  ];

  constructor(
    public router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const user = this.authService.currentUserValue;
    if (user) {
      this.userName.set(user.fullName || user.username);
      this.userRole.set(user.roleName || 'مدير النظام');
      this.branchName.set(user.branchName || 'الفرع الرئيسي');
    }
  }

  navigateToAnalytics(): void {
    this.router.navigate(['/dashboard/analytics']);
  }

  getActionsByCategory(catKey: string): QuickAction[] {
    return this.filteredActions().filter(a => a.category === catKey);
  }

  navigateToAction(action: QuickAction): void {
    if (action.queryParams) {
      this.router.navigate([action.route], { queryParams: action.queryParams });
    } else {
      this.router.navigate([action.route]);
    }
  }

  getCurrentDateFormatted(): string {
    return new Date().toLocaleDateString('ar-YE', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
