import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AlertService } from '../../core/services/alert.service';
import { SettingsService } from '../../core/services/settings.service';
import { Subject, takeUntil, filter } from 'rxjs';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { environment } from '../../../environments/environment';

interface SidebarMenuItem {
    key?: string;
    label: string;
    route?: string;
    icon: string;
    exact?: boolean;
    badge?: string;
    alert?: boolean;
    children?: SidebarMenuItem[];
}

interface SidebarSection {
    key: string;
    label: string;
    icon: string;
    iconClass: string;
    match?: string;
    extraMatches?: string[];
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
    <div class="nav-section" *ngFor="let section of menuSections" [class.section-active]="isSectionActive(section)">
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
          <a *ngIf="!item.children" [routerLink]="item.route" routerLinkActive="active-route"
            [routerLinkActiveOptions]="{ exact: item.exact || false }" class="menu-item" [title]="item.label">
            <i [class]="item.icon"></i>
            <span>{{ item.label }}</span>
            <span class="new-badge" *ngIf="item.badge">{{ item.badge }}</span>
            <span class="alert-pill" *ngIf="item.alert && unreadAlertsCount > 0">{{ unreadAlertsCount }}</span>
          </a>

          <div class="nested-menu" *ngIf="item.children">
            <button type="button" class="menu-item nested-trigger" (click)="toggleMenu(item.key!)"
              [class.open]="isMenuOpen(item.key!)" [title]="item.label" [attr.aria-expanded]="isMenuOpen(item.key!)">
              <i [class]="item.icon"></i>
              <span>{{ item.label }}</span>
              <i class="pi pi-chevron-down nested-arrow"></i>
            </button>

            <div class="nested-panel" [class.open]="isMenuOpen(item.key!)">
              <a *ngFor="let child of item.children" [routerLink]="child.route" routerLinkActive="active-route"
                [routerLinkActiveOptions]="{ exact: child.exact || false }" class="submenu-item" [title]="child.label">
                <i [class]="child.icon"></i>
                <span>{{ child.label }}</span>
              </a>
            </div>
          </div>
        </ng-container>
      </div>
    </div>
  </nav>

  <div class="sidebar-footer">
    <div class="status-dot"></div>
    <span>متصل الآن</span>
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

    unreadAlertsCount = 0;
    currentRoute = '';
    pharmacyName = 'الصيدلية الذكية';
    pharmacyLogoUrl: string | null = null;
    readonly serverUrl = environment.apiUrl.replace('/api', '');
    openMenus: Record<string, boolean> = {
        dashboard: true
    };
    menuSections: SidebarSection[] = [
        {
            key: 'dashboard',
            label: 'لوحات التحكم',
            icon: 'pi pi-chart-pie',
            iconClass: 'icon-dashboard',
            children: [
                { label: 'لوحة التحكم', route: '/dashboard', icon: 'pi pi-chart-pie', exact: true },
                { label: 'لوحة التحكم الرئيسية', route: '/dashboard/master', icon: 'pi pi-chart-line', badge: 'جديد' }
            ]
        },
        {
            key: 'inventory',
            label: 'المخزون والمنتجات',
            icon: 'pi pi-box',
            iconClass: 'icon-inventory',
            match: '/inventory',
            children: [
                { label: 'الأدوية والمنتجات', route: '/inventory/medicines', icon: 'pi pi-box' },
                { label: 'التصنيفات', route: '/inventory/categories', icon: 'pi pi-tags' },
                { label: 'حركات المخزون', route: '/inventory/movements', icon: 'pi pi-history' },
                { label: 'الدفعات', route: '/inventory/batches', icon: 'pi pi-list' }
            ]
        },
        {
            key: 'sales',
            label: 'المبيعات',
            icon: 'pi pi-shopping-cart',
            iconClass: 'icon-sales',
            match: '/sales',
            children: [
                { label: 'فواتير المبيعات', route: '/sales', icon: 'pi pi-shopping-cart', exact: true },
                { label: 'مرتجع المبيعات', route: '/sales/returns', icon: 'pi pi-replay' },
                { label: 'طلبات الأونلاين', route: '/online-orders', icon: 'pi pi-globe', badge: 'جديد' }
            ]
        },
        {
            key: 'purchases',
            label: 'المشتريات',
            icon: 'pi pi-truck',
            iconClass: 'icon-purchases',
            match: '/purchases',
            children: [
                { label: 'فواتير الشراء', route: '/purchases', icon: 'pi pi-truck' },
                { label: 'مرتجع المشتريات', route: '/purchases/returns', icon: 'pi pi-refresh' }
            ]
        },
        {
            key: 'warehouses',
            label: 'المخازن',
            icon: 'pi pi-warehouse',
            iconClass: 'icon-warehouses',
            match: '/warehouses',
            children: [
                { label: 'جميع المخازن', route: '/warehouses', icon: 'pi pi-warehouse', exact: true }
            ]
        },
        {
            key: 'employees',
            label: 'الموظفين',
            icon: 'pi pi-id-card',
            iconClass: 'icon-employees',
            match: '/employees',
            extraMatches: ['/branches', '/departments'],
            children: [
                { label: 'جميع الموظفين', route: '/employees', icon: 'pi pi-users', exact: true },
                { label: 'الفروع', route: '/branches', icon: 'pi pi-sitemap', exact: true },
                { label: 'الأقسام', route: '/departments', icon: 'pi pi-th-large', exact: true }
            ]
        },
        {
            key: 'partners',
            label: 'الشركاء',
            icon: 'pi pi-users',
            iconClass: 'icon-partners',
            match: '/partners',
            extraMatches: ['/customers'],
            children: [
                { label: 'الموردين', route: '/partners', icon: 'pi pi-users', exact: true },
                { label: 'العملاء', route: '/customers', icon: 'pi pi-user-plus' }
            ]
        },
        {
            key: 'financial',
            label: 'المالية',
            icon: 'pi pi-wallet',
            iconClass: 'icon-financial',
            match: '/financial',
            extraMatches: ['/finance'],
            children: [
                { label: 'الخزينة والأرصدة', route: '/financial/dashboard', icon: 'pi pi-wallet' },
                { label: 'دفتر الأستاذ', route: '/financial/ledger', icon: 'pi pi-book' },
                {
                    key: 'expenses',
                    label: 'المصروفات',
                    icon: 'pi pi-money-bill',
                    children: [
                        { label: 'قائمة المصروفات', route: '/finance/expenses', icon: 'pi pi-list', exact: true },
                        { label: 'إضافة مصروف', route: '/finance/expenses/add', icon: 'pi pi-plus-circle' },
                        { label: 'فئات المصروفات', route: '/finance/expense-categories', icon: 'pi pi-tag' }
                    ]
                }
            ]
        },
        {
            key: 'accounting',
            label: 'المحاسبة العمومية',
            icon: 'pi pi-sitemap',
            iconClass: 'icon-accounting',
            match: '/accounting',
            children: [
                { label: 'شجرة الحسابات', route: '/accounting/chart', icon: 'pi pi-list' },
                { label: 'القيود اليومية', route: '/accounting/journal', icon: 'pi pi-pencil' },
                { label: 'ميزان المراجعة', route: '/accounting/trial-balance', icon: 'pi pi-balance-scale' },
                { label: 'القوائم المالية', route: '/accounting/financial-statements', icon: 'pi pi-file-pdf' }
            ]
        },
        {
            key: 'reports',
            label: 'تقارير مالية',
            icon: 'pi pi-chart-bar',
            iconClass: 'icon-reports',
            match: '/reports',
            children: [
                { label: 'المبيعات اليومية', route: '/reports/daily-sales', icon: 'pi pi-calendar' },
                { label: 'تقرير أداء الموظفين', route: '/reports/employee-performance', icon: 'pi pi-id-card' },
                { label: 'الأكثر مبيعاً', route: '/reports/best-selling', icon: 'pi pi-star' },
                { label: 'ديون العملاء', route: '/reports/customer-debts', icon: 'pi pi-users' },
                { label: 'ديون الموردين', route: '/reports/supplier-debts', icon: 'pi pi-truck' },
                { label: 'صافي الأرباح', route: '/reports/net-profit', icon: 'pi pi-dollar' },
                { label: 'تقييم المخزون', route: '/reports/inventory-valuation', icon: 'pi pi-box' }
            ]
        },
        {
            key: 'admin',
            label: 'الإدارة',
            icon: 'pi pi-cog',
            iconClass: 'icon-users',
            match: '/users',
            extraMatches: ['/system-alerts'],
            children: [
                { label: 'المستخدمين والصلاحيات', route: '/users', icon: 'pi pi-id-card' },
                { label: 'التنبيهات', route: '/system-alerts', icon: 'pi pi-bell', alert: true },
                { label: 'إعدادات الصيدلية', route: '/settings', icon: 'pi pi-cog' }
            ]
        }
    ];
    private destroy$ = new Subject<void>();

    constructor(
        private alertService: AlertService,
        private settingsService: SettingsService,
        private router: Router
    ) { }

    ngOnInit() {
        this.loadPharmacySettings();

        // Subscribe to alerts
        this.alertService.unreadAlerts$
            .pipe(takeUntil(this.destroy$))
            .subscribe(alerts => {
                this.unreadAlertsCount = alerts.length;
            });

        // Track current route
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            takeUntil(this.destroy$)
        ).subscribe((event: any) => {
            this.currentRoute = event.url;
            this.openActiveMenus();
        });

        this.currentRoute = this.router.url;
        this.openActiveMenus();
    }

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
