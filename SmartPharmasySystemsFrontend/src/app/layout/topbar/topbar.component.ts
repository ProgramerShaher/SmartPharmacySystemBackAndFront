// topbar.component.ts
import { Component, HostListener } from '@angular/core';
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
    styleUrls: ['./topbar.component.scss']
})
export class TopbarComponent {
    searchQuery = '';
    hasNotifications = true;
    userName = 'المسؤول';
    userRole = 'مدير النظام';
    userAvatar = 'https://ui-avatars.com/api/?name=Admin+User&background=0d9488&color=fff';

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
        public themeService: ThemeService
    ) { }

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
            accept: () => {
                // Implement logout logic
                this.router.navigate(['/login']);
            }
        });
    }

    @HostListener('document:keydown', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        // Ctrl + K للبحث
        if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
            event.preventDefault();
            const searchInput = document.querySelector('.search-wrapper input') as HTMLInputElement;
            searchInput?.focus();
        }
    }
}