import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { ShiftModalComponent } from '../../shared/components/shift-modal/shift-modal.component';
import { ShiftService } from '../../core/services/shift.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, SidebarComponent, TopbarComponent, ShiftModalComponent],
    templateUrl: './main-layout.component.html',
    styleUrls: ['./main-layout.scss']
})
export class MainLayoutComponent implements OnInit {
    isSidebarCollapsed = false;
    isMobile = false;
    shiftModalVisible = false;
    shiftModalMode: 'open' | 'close' = 'open';
    private shiftSub?: Subscription;

    constructor(
        private shiftService: ShiftService,
        private authService: AuthService
    ) { }

    @HostListener('window:resize', ['$event'])
    onResize(event: Event) {
        this.checkMobile();
    }

    ngOnInit() {
        this.checkMobile();
        // Listen for toggle event from topbar
        window.addEventListener('toggleSidebar', () => {
            this.toggleSidebar();
        });

        // Listen for Shift Modal triggers
        this.shiftSub = this.shiftService.showModal$.subscribe(mode => {
            this.shiftModalMode = mode;
            this.shiftModalVisible = true;
        });

        // Check if there is an open shift on startup
        this.checkCurrentShift();
    }

    ngOnDestroy() {
        if (this.shiftSub) {
            this.shiftSub.unsubscribe();
        }
    }

    private checkCurrentShift() {
        this.shiftService.getCurrentShift().subscribe({
            next: (res) => {
                // If it fails or returns false/null, it means no open shift
                // Wait, our API returns 400 if no open shift. 
            },
            error: (err) => {
                // Assuming 400 Bad Request is returned when "No open shift found."
                // Force open shift modal
                this.shiftModalMode = 'open';
                this.shiftModalVisible = true;
            }
        });
    }

    onShiftProcessed(shift: any) {
        // If mode was close, proceed with logout
        if (this.shiftModalMode === 'close') {
            this.authService.logout();
        }
    }

    private checkMobile() {
        this.isMobile = window.innerWidth < 992;
        if (this.isMobile) {
            this.isSidebarCollapsed = true;
        }
    }

    toggleSidebar() {
        this.isSidebarCollapsed = !this.isSidebarCollapsed;
    }

    setSidebarCollapsed(value: boolean) {
        this.isSidebarCollapsed = value;
    }
}
