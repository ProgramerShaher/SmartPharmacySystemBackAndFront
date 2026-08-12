import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, HostBinding, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AlertService } from '../../core/services/alert.service';
import { SettingsService } from '../../core/services/settings.service';
import { AccountingService } from '../../core/services/accounting.service';
import { PermissionService } from '../../core/services/permission.service';
import { Subject, takeUntil, filter } from 'rxjs';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../features/auth/services/auth.service';
import { CurrentUserResponse } from '../../core/models/auth-response.interface';

interface SidebarMenuItem {
  key?: string;
  label: string;
  route?: string;
  icon: string;
  exact?: boolean;
  badge?: string;
  alert?: boolean;
  permission?: string;        // كود الصلاحية المطلوبة
  anyPermission?: string[];   // أي صلاحية من القائمة تكفي
  children?: SidebarMenuItem[];
}

interface SidebarSection {
  key: string;
  label: string;
  icon: string;
  iconClass: string;
  match?: string;
  extraMatches?: string[];
  permission?: string;        // صلاحية لإظهار القسم كاملاً
  anyPermission?: string[];   // القسم يظهر إذا كان أي من أبنائه مرئياً
  children: SidebarMenuItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterLink, RouterLinkActive],
  template: `
<div class="layout-sidebar" dir="rtl" [@slideIn]>
  <div class="sidebar-header">
    <div class="brand-mark" [title]="pharmacyName">
      <img *ngIf="pharmacyLogoUrl; else defaultSidebarLogo" [src]="pharmacyLogoUrl" [alt]="pharmacyName">
      <ng-template #defaultSidebarLogo>
        <i class="pi pi-bolt"></i>
      </ng-template>
    </div>

    <div class="brand-copy">
      <h2>{{ pharmacyName }}</h2>
      <span>نظام الإدارة الذكي</span>
    </div>

    <button type="button" class="collapse-btn" (click)="toggleCollapsed()"
      [attr.aria-label]="isCollapsed ? 'توسيع الشريط الجانبي' : 'طي الشريط الجانبي'"
      [title]="isCollapsed ? 'توسيع الشريط الجانبي' : 'طي الشريط الجانبي'">
      <i class="pi" [class.pi-angle-double-left]="!isCollapsed" [class.pi-angle-double-right]="isCollapsed"></i>
    </button>
  </div>

  <nav class="sidebar-content custom-scrollbar" aria-label="التنقل الرئيسي">
    <ng-container *ngFor="let section of menuSections">
      <!-- أظهر القسم فقط إذا كان له على الأقل عنصر واحد مرئي -->
      <div class="nav-section" *ngIf="hasSectionVisible(section)" [class.section-active]="isSectionActive(section)">
        <button type="button" class="section-trigger" (click)="toggleMenu(section.key)"
          [class.open]="isMenuOpen(section.key)" [title]="section.label" [attr.aria-expanded]="isMenuOpen(section.key)">
          <span class="item-icon" [ngClass]="section.iconClass">
            <i [class]="section.icon"></i>
          </span>
          <span class="section-label">{{ section.label }}</span>
          <i class="pi pi-chevron-down section-arrow"></i>
        </button>

        <div class="section-panel" [class.open]="isMenuOpen(section.key)">
          <ng-container *ngFor="let item of section.children">

            <!-- عنصر بدون أبناء -->
            <ng-container *ngIf="!item.children">
              <a *ngIf="canAccess(item)" [routerLink]="item.route" routerLinkActive="active-route"
                [routerLinkActiveOptions]="{ exact: item.exact || false }" class="menu-item" [title]="item.label">
                <i [class]="item.icon"></i>
                <span>{{ item.label }}</span>
                <span class="new-badge" *ngIf="item.badge">{{ item.badge }}</span>
                <span class="alert-pill" *ngIf="item.alert && unreadAlertsCount > 0">{{ unreadAlertsCount }}</span>
              </a>
            </ng-container>

            <!-- عنصر بأبناء (Nested Menu) -->
            <div class="nested-menu" *ngIf="item.children && hasNestedVisible(item)">
              <button type="button" class="menu-item nested-trigger" (click)="toggleMenu(item.key!)"
                [class.open]="isMenuOpen(item.key!)" [title]="item.label" [attr.aria-expanded]="isMenuOpen(item.key!)">
                <i [class]="item.icon"></i>
                <span>{{ item.label }}</span>
                <i class="pi pi-chevron-down nested-arrow"></i>
              </button>

              <div class="nested-panel" [class.open]="isMenuOpen(item.key!)">
                <a *ngFor="let child of item.children" [routerLink]="child.route" routerLinkActive="active-route"
                  [routerLinkActiveOptions]="{ exact: child.exact || false }" class="submenu-item" [title]="child.label"
                  [style.display]="canAccess(child) ? '' : 'none'">
                  <i [class]="child.icon"></i>
                  <span>{{ child.label }}</span>
                </a>
              </div>
            </div>

          </ng-container>
        </div>
      </div>
    </ng-container>
  </nav>

  <div class="sidebar-footer">
    <div class="status-dot"></div>
    <div class="user-info">
      <span class="user-name">{{ currentUser?.employeeName || currentUser?.fullName || 'مستخدم' }}</span>
      <span class="user-role">{{ currentUser?.roleName || 'مدير النظام' }}</span>
    </div>
  </div>
</div>
    `,
  styleUrls: ['./sidebar.component.css'],
  animations: [
    trigger('slideIn', [
      state('void', style({ transform: 'translateX(100%)', opacity: 0 })),
      state('*', style({ transform: 'translateX(0)', opacity: 1 })),
      transition('void => *', animate('300ms cubic-bezier(0.4, 0, 0.2, 1)')),
      transition('* => void', animate('250ms cubic-bezier(0.4, 0, 0.2, 1)'))
    ])
  ]
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() sidebarVisible: boolean = false;
  @Input() isCollapsed: boolean = false;
  @Output() collapseChange = new EventEmitter<boolean>();
  @HostBinding('class.sidebar-visible') get visible() { return this.sidebarVisible; }
  @HostBinding('class.sidebar-collapsed') get collapsed() { return this.isCollapsed; }

  private permissionService = inject(PermissionService);

  currentUser: CurrentUserResponse | null = null;
  unreadAlertsCount = 0;
  currentRoute = '';
  pharmacyName = 'الصيدلية الذكية';
  pharmacyLogoUrl: string | null = null;
  readonly serverUrl = environment.apiUrl.replace('/api', '');
  openMenus: Record<string, boolean> = {
    dashboard: true
  };

  // ================================================================
  // قائمة الشريط الجانبي — كل عنصر مرتبط بكود صلاحيته
  // ================================================================
  menuSections: SidebarSection[] = [
    {
      key: 'dashboard',
      label: 'الرئيسية',
      icon: 'pi pi-home',
      iconClass: 'icon-dashboard',
      children: [
        { label: 'لوحة التحكم', route: '/dashboard', icon: 'pi pi-chart-pie', exact: true },
        {
          label: 'لوحة التحكم الشاملة', route: '/dashboard/master', icon: 'pi pi-chart-line',
          permission: 'dashboard.master'
        }
      ]
    },
    {
      key: 'sales',
      label: 'نقاط البيع والمبيعات',
      icon: 'pi pi-shopping-cart',
      iconClass: 'icon-sales',
      match: '/sales',
      extraMatches: ['/online-orders', '/pos'],
      anyPermission: ['sales.invoices.view', 'sales.invoices.create', 'sales.create', 'sales.manage'],
      children: [
        {
          label: 'شاشة الكاشير (POS)', route: '/sales/create', icon: 'pi pi-desktop',
          anyPermission: ['sales.invoices.create', 'sales.create', 'sales.invoices.view', 'sales.manage']
        },
        {
          label: 'فواتير المبيعات', route: '/sales', icon: 'pi pi-list', exact: true,
          permission: 'sales.invoices.view'
        },
        {
          label: 'مرتجعات المبيعات', route: '/sales/returns', icon: 'pi pi-undo',
          permission: 'sales.returns.view'
        },
        {
          label: 'طلبات الأونلاين', route: '/online-orders', icon: 'pi pi-globe',
          permission: 'online_orders.manage'
        },
        {
          label: 'إغلاق الورديات', route: '/sales/daily-closing', icon: 'pi pi-lock',
          permission: 'sales.daily_closing.manage'
        }
      ]
    },
    {
      key: 'purchases',
      label: 'المشتريات',
      icon: 'pi pi-shopping-bag',
      iconClass: 'icon-purchases',
      match: '/purchases',
      children: [
        {
          label: 'إنشاء فاتورة شراء', route: '/purchases/create', icon: 'pi pi-plus',
          permission: 'purchases.invoices.create'
        },
        {
          label: 'فواتير المشتريات', route: '/purchases', icon: 'pi pi-list', exact: true,
          permission: 'purchases.invoices.view'
        },
        {
          label: 'مرتجعات الموردين', route: '/purchases/returns', icon: 'pi pi-undo',
          permission: 'purchases.returns.view'
        }
      ]
    },
    {
      key: 'inventory',
      label: 'الأدوية والمخزون',
      icon: 'pi pi-box',
      iconClass: 'icon-inventory',
      match: '/inventory',
      extraMatches: ['/warehouses'],
      children: [
        {
          label: 'دليل الأدوية', route: '/inventory/medicines', icon: 'pi pi-list',
          permission: 'inventory.medicines.view'
        },
        {
          label: 'تصنيفات الأدوية', route: '/inventory/categories', icon: 'pi pi-tags',
          permission: 'inventory.categories.manage'
        },
        {
          label: 'إدارة المخازن', route: '/warehouses', icon: 'pi pi-building', exact: true,
          permission: 'inventory.warehouses.view'
        },
        {
          label: 'التحويلات الداخلية', route: '/warehouses/transfers/internal', icon: 'pi pi-sync',
          permission: 'inventory.transfers.create'
        },
        {
          label: 'تحويلات الفروع', route: '/warehouses/transfers/external', icon: 'pi pi-globe',
          permission: 'inventory.transfers.create'
        },
        {
          label: 'الهوالك والتوالف', route: '/warehouses/damaged', icon: 'pi pi-exclamation-triangle',
          permission: 'inventory.damaged.manage'
        },
        {
          label: 'حركات المخزون', route: '/inventory/movements', icon: 'pi pi-history',
          permission: 'inventory.medicines.view'
        },
        {
          label: 'الدفعات والتواريخ', route: '/inventory/batches', icon: 'pi pi-calendar',
          permission: 'inventory.medicines.view'
        },
        {
          label: 'جرد المخزون', route: '/inventory/stock-counts', icon: 'pi pi-clipboard',
          permission: 'inventory.stock_counts.create'
        },
        {
          label: 'الجدولة الآلية للجرد', route: '/inventory/stock-counts-schedules', icon: 'pi pi-calendar-plus',
          permission: 'inventory.stock_counts.create'
        },
        {
          label: 'تحليل التدفق المخزني', route: '/inventory/flow-analysis', icon: 'pi pi-chart-bar',
          permission: 'inventory.medicines.view'
        },
        {
          label: 'الجرد الآلي الشامل', route: '/inventory/automated-audits', icon: 'pi pi-bolt',
          permission: 'inventory.stock_counts.create'
        }
      ]
    },
    {
      key: 'partners',
      label: 'العملاء والموردون',
      icon: 'pi pi-users',
      iconClass: 'icon-partners',
      match: '/partners',
      extraMatches: ['/customers', '/suppliers'],
      children: [
        {
          label: 'إدارة العملاء', route: '/customers', icon: 'pi pi-user',
          permission: 'partners.customers.view'
        },
        {
          label: 'إدارة الموردين', route: '/partners/suppliers', icon: 'pi pi-truck',
          permission: 'partners.suppliers.view'
        }
      ]
    },
    {
      key: 'finance',
      label: 'المالية والحسابات',
      icon: 'pi pi-wallet',
      iconClass: 'icon-finance',
      match: '/finance',
      extraMatches: ['/financial', '/accounting'],
      children: [
        {
          label: 'الخزينة والأرصدة', route: '/financial/dashboard', icon: 'pi pi-money-bill', exact: true,
          permission: 'finance.dashboard.view'
        },
        {
          label: 'سندات القبض', route: '/customers/receipts', icon: 'pi pi-arrow-down-left',
          permission: 'finance.receipts.create'
        },
        {
          label: 'سندات الصرف', route: '/partners/suppliers/payments', icon: 'pi pi-arrow-up-right',
          permission: 'finance.payments.create'
        },
        {
          label: 'المصروفات النثرية', route: '/finance/expenses', icon: 'pi pi-money-bill',
          permission: 'finance.expenses.view'
        },
        {
          label: 'دفتر الأستاذ', route: '/financial/ledger', icon: 'pi pi-book',
          permission: 'accounting.chart.view'
        },
        {
          label: 'شجرة الحسابات', route: '/accounting/chart', icon: 'pi pi-sitemap',
          permission: 'accounting.chart.view'
        },
        {
          label: 'قيود اليومية', route: '/accounting/journal', icon: 'pi pi-book',
          permission: 'accounting.journal.view'
        },
        {
          label: 'ميزان المراجعة', route: '/accounting/trial-balance', icon: 'pi pi-table',
          permission: 'accounting.statements.view'
        },
        {
          label: 'القوائم المالية', route: '/accounting/financial-statements', icon: 'pi pi-chart-pie',
          permission: 'accounting.statements.view'
        },
        {
          label: 'أرصدة الحسابات', route: '/accounting/balances', icon: 'pi pi-dollar',
          permission: 'accounting.statements.view'
        }
      ]
    },
    {
      key: 'hr',
      label: 'الموارد البشرية',
      icon: 'pi pi-id-card',
      iconClass: 'icon-hr',
      match: '/employees',
      children: [
        {
          label: 'شؤون الموظفين', route: '/employees', icon: 'pi pi-id-card', exact: true,
          permission: 'hr.employees.view'
        },
        {
          label: 'الحضور والانصراف', route: '/employees/attendance', icon: 'pi pi-calendar-clock', exact: true,
          permission: 'hr.employees.view'
        },
        {
          label: 'مسير الرواتب', route: '/employees/payroll', icon: 'pi pi-wallet', exact: true,
          permission: 'hr.employees.view'
        }
      ]
    },
    {
      key: 'reports',
      label: 'التقارير الشاملة',
      icon: 'pi pi-file',
      iconClass: 'icon-reports',
      match: '/reports',
      children: [
        {
          label: 'تقارير المبيعات', route: '/reports/daily-sales', icon: 'pi pi-chart-line',
          permission: 'reports.sales.view'
        },
        {
          label: 'الأدوية الأكثر مبيعاً', route: '/reports/best-selling', icon: 'pi pi-star',
          permission: 'reports.sales.view'
        },
        {
          label: 'تقارير الورديات', route: '/reports/shifts', icon: 'pi pi-clock',
          permission: 'reports.sales.view'
        },
        {
          label: 'أداء الموظفين', route: '/reports/employee-performance', icon: 'pi pi-users',
          permission: 'reports.sales.view'
        },
        {
          label: 'ديون العملاء', route: '/reports/customer-debts', icon: 'pi pi-user-minus',
          permission: 'reports.financial.view'
        },
        {
          label: 'ديون الموردين', route: '/reports/supplier-debts', icon: 'pi pi-truck',
          permission: 'reports.purchases.view'
        },
        {
          label: 'تقييم المخزون', route: '/reports/inventory-valuation', icon: 'pi pi-box',
          permission: 'reports.inventory.view'
        },
        {
          label: 'صافي الأرباح', route: '/reports/net-profit', icon: 'pi pi-dollar',
          permission: 'reports.financial.view'
        }
      ]
    },
    {
      key: 'settings',
      label: 'إعدادات النظام',
      icon: 'pi pi-cog',
      iconClass: 'icon-settings',
      match: '/settings',
      extraMatches: ['/users', '/roles', '/branches', '/departments', '/system-alerts'],
      children: [
        {
          label: 'المستخدمون', route: '/users', icon: 'pi pi-user-edit',
          permission: 'admin.users.view'
        },
        {
          label: 'الأدوار والصلاحيات', route: '/roles', icon: 'pi pi-shield',
          permission: 'access.roles.view'
        },
        {
          label: 'فروع الصيدلية', route: '/branches', icon: 'pi pi-building',
          permission: 'branches.manage'
        },
        {
          label: 'الأقسام', route: '/departments', icon: 'pi pi-th-large',
          permission: 'branches.manage'
        },
        {
          label: 'تنبيهات النظام', route: '/system-alerts', icon: 'pi pi-bell', alert: true,
          permission: 'admin.alerts.manage'
        },
        {
          label: 'الإعدادات العامة', route: '/settings', icon: 'pi pi-cog', exact: true,
          permission: 'settings.general.view'
        }
      ]
    }
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private alertService: AlertService,
    private settingsService: SettingsService,
    private router: Router,
    private authService: AuthService,
    private accountingService: AccountingService
  ) { }

  ngOnInit() {
    this.loadPharmacySettings();

    // Load current user for sidebar footer
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        // @ts-ignore (since AuthResponse vs CurrentUserResponse matching)
        this.currentUser = user;
      });

    this.alertService.unreadAlerts$
      .pipe(takeUntil(this.destroy$))
      .subscribe(alerts => {
        this.unreadAlertsCount = alerts.length;
      });

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe((event: any) => {
      this.currentRoute = event.url;
      this.openActiveMenus();
      
      // إغلاق القائمة الجانبية تلقائياً في الجوال عند الانتقال لصفحة جديدة
      if (window.innerWidth < 992) {
        this.collapseChange.emit(true);
      }
    });

    this.currentRoute = this.router.url;
    this.openActiveMenus();
  }

  // ================================================================
  // منطق الصلاحيات — الجوهر
  // ================================================================

  /**
   * هل يمكن رؤية عنصر معين؟
   * Admin → true دائماً
   * بدون permission → true (للجميع)
   * وإلا → تحقق من الصلاحية
   */
  canAccess(item: SidebarMenuItem): boolean {
    if (this.permissionService.isAdmin()) return true;
    if (!item.permission && !item.anyPermission) return true;
    if (item.anyPermission) return this.permissionService.hasAnyPermission(item.anyPermission);
    return this.permissionService.hasPermission(item.permission!);
  }

  /**
   * هل يوجد في القسم على الأقل عنصر واحد مرئي؟
   * إذا لا → أخفِ القسم كاملاً
   */
  hasSectionVisible(section: SidebarSection): boolean {
    if (this.permissionService.isAdmin()) return true;
    return section.children.some(item => {
      if (item.children) return this.hasNestedVisible(item);
      return this.canAccess(item);
    });
  }

  /**
   * هل في القائمة الداخلية (Nested) عنصر واحد مرئي؟
   */
  hasNestedVisible(item: SidebarMenuItem): boolean {
    if (!item.children) return this.canAccess(item);
    return item.children.some(child => this.canAccess(child));
  }

  // ================================================================
  // منطق التنقل
  // ================================================================

  toggleCollapsed() {
    this.collapseChange.emit(!this.isCollapsed);
  }

  toggleMenu(key: string) {
    if (this.isCollapsed) {
      this.collapseChange.emit(false);
    }
    this.openMenus[key] = !this.openMenus[key];
  }

  isMenuOpen(key: string): boolean {
    return !!this.openMenus[key] && !this.isCollapsed;
  }

  isSectionActive(section: SidebarSection): boolean {
    return this.routeMatches(section) || section.children?.some((item) => this.isItemActive(item));
  }

  isItemActive(item: SidebarMenuItem): boolean {
    if (item.route && this.currentRoute.startsWith(item.route)) {
      return true;
    }
    return item.children?.some((child) => this.isItemActive(child)) || false;
  }

  private openActiveMenus() {
    this.menuSections.forEach((section) => {
      if (this.isSectionActive(section)) {
        this.openMenus[section.key] = true;
      }
      section.children?.forEach((item) => {
        if (item.key && this.isItemActive(item)) {
          this.openMenus[item.key] = true;
        }
      });
    });
  }

  private routeMatches(section: SidebarSection): boolean {
    const matches = [section.match, ...(section.extraMatches || [])].filter((match): match is string => !!match);
    return matches.some((match: string) => this.currentRoute.startsWith(match));
  }

  private loadPharmacySettings(): void {
    this.settingsService.getSettings().subscribe({
      next: (settings) => {
        this.pharmacyName = settings.pharmacyName || this.pharmacyName;
        this.pharmacyLogoUrl = settings.logoUrl ? `${this.serverUrl}${settings.logoUrl}` : null;
      }
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
