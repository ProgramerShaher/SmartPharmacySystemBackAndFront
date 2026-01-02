import { Component, HostListener, HostBinding, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MenuItem } from 'primeng/api';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { MenuModule } from 'primeng/menu';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { ThemeService, Theme } from '../../core/services/theme.service';
import { AuthService } from '../../features/auth/services/auth.service';
import { AlertService } from '../../core/services/alert.service';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        OverlayPanelModule,
        MenuModule,
        ConfirmDialogModule,
        TooltipModule
    ],
    providers: [ConfirmationService],
    templateUrl: './topbar.component.html',
    styleUrls: ['./topbar.component.css']
})
export class TopbarComponent {
    searchQuery = '';
    isScrolled = false;
    userName = 'المسؤول';
    userRole = 'مدير النظام';
    userAvatar = 'https://ui-avatars.com/api/?name=Admin+User&background=0d9488&color=fff&rounded=true';

    @HostBinding('class.scrolled') get scrolled() {
        return this.isScrolled;
    }

    userMenuItems: MenuItem[] = [
        {
            label: 'الملف الشخصي',
            icon: 'pi pi-user',
            command: () => this.navigateTo('/profile')
        },
        {
            label: 'الإعدادات',
            icon: 'pi pi-cog',
            command: () => this.navigateTo('/settings')
        },
        {
            separator: true
        },
        {
            label: 'تسجيل الخروج',
            icon: 'pi pi-power-off',
            command: () => this.confirmLogout()
        }
    ];

    constructor(
        private confirmationService: ConfirmationService,
        private router: Router,
        public themeService: ThemeService,
        private authService: AuthService,
        public alertService: AlertService
    ) { }

    ngOnInit() {
        // Fetch real user data from AuthService
        const user = this.authService.currentUserValue;
        if (user) {
            this.userName = user.fullName || user.username;
            this.userRole = user.roleName || 'مستخدم';
            this.userAvatar = `https://ui-avatars.com/api/?name=${encodeURIComponent(this.userName)}&background=0d9488&color=fff&bold=true&rounded=true`;
        }

        // Optional: Listen to user changes
        this.authService.currentUser$.subscribe(u => {
            if (u) {
                this.userName = u.fullName || u.username;
                this.userRole = u.roleName || 'مستخدم';
                this.userAvatar = `https://ui-avatars.com/api/?name=${encodeURIComponent(this.userName)}&background=0d9488&color=fff&bold=true&rounded=true`;
            }
        });
    }

    changeTheme(theme: Theme) {
        this.themeService.setTheme(theme);
    }

    toggleSidebar() {
        // Emit event to parent component
        const event = new CustomEvent('toggleSidebar');
        window.dispatchEvent(event);
    }

    onSearch() {
        if (this.searchQuery.trim()) {
            console.log('Searching for:', this.searchQuery);
            // Implement search logic
            // Example: this.router.navigate(['/search'], { queryParams: { q: this.searchQuery } });
        }
    }

    navigateTo(path: string) {
        this.router.navigate([path]);
    }

    toggleNotifications() {
        // Implement notifications logic
    }

    toggleUserMenu(event: any, menu: any) {
        menu.toggle(event);
    }

    confirmLogout() {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد أنك تريد تسجيل الخروج؟',
            header: 'تأكيد الخروج',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم',
            rejectLabel: 'لا',
            acceptButtonStyleClass: 'p-button-danger',
            rejectButtonStyleClass: 'p-button-text',
            accept: () => {
                this.authService.logout();
            }
        });
    }

    // Alert Notification Methods
    viewAllAlerts() {
        this.router.navigate(['/system-alerts']);
    }

    viewAlertDetails(alertId: number, event?: Event) {
        if (event) {
            event.stopPropagation();
        }
        this.router.navigate(['/system-alerts']);
    }

    markAlertAsRead(alertId: number, event?: Event) {
        if (event) {
            event.stopPropagation();
        }

        this.alertService.markAsRead(alertId).subscribe({
            next: () => {
                // Alert service will automatically refresh unread count
                console.log('✅ Alert marked as read');
            },
            error: (err) => {
                console.error('❌ Failed to mark alert as read:', err);
            }
        });
    }

    getAlertIcon(alertType: any): string {
        const typeStr = alertType?.toString() || '';
        if (!typeStr) return 'pi-bell';
        if (typeStr.includes('OneWeek') || typeStr.includes('TwoWeeks')) {
            return 'pi-exclamation-triangle';
        }
        if (typeStr.includes('LowStock')) {
            return 'pi-box';
        }
        return 'pi-info-circle';
    }

    getAlertSeverity(alertType: any): 'success' | 'info' | 'warning' | 'danger' {
        const typeStr = alertType?.toString() || '';
        if (!typeStr) return 'info';
        if (typeStr.includes('OneWeek')) return 'danger';
        if (typeStr.includes('TwoWeeks')) return 'warning';
        if (typeStr.includes('LowStock')) return 'warning';
        return 'info';
    }

    hasCriticalAlerts(): boolean {
        // Access the current value from the BehaviorSubject
        const currentAlerts = (this.alertService as any).unreadAlertsSubject?.value || [];
        return currentAlerts.some((a: any) => {
            const alertType = a.alertType?.toString() || '';
            return alertType.includes('OneWeek');
        });
    }

    getTimeAgo(dateString: string): string {
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (seconds < 60) return 'الآن';
        if (seconds < 3600) return `منذ ${Math.floor(seconds / 60)} دقيقة`;
        if (seconds < 86400) return `منذ ${Math.floor(seconds / 3600)} ساعة`;
        if (seconds < 604800) return `منذ ${Math.floor(seconds / 86400)} يوم`;
        return `منذ ${Math.floor(seconds / 604800)} أسبوع`;
    }

    @HostListener('window:scroll', [])
    onWindowScroll() {
        this.isScrolled = window.scrollY > 20;
    }

    @HostListener('document:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        // Ctrl + K للبحث
        if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
            event.preventDefault();
            const searchInput = document.querySelector('.search-omnibox input') as HTMLInputElement;
            searchInput?.focus();
        }
    }
}