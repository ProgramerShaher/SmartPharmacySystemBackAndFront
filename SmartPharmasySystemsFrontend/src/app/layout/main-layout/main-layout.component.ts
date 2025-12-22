import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, SidebarComponent, TopbarComponent],
    templateUrl: './main-layout.component.html',
    styleUrls: ['./main-layout.scss']
})
export class MainLayoutComponent implements OnInit {
    isSidebarCollapsed = false;
    isMobile = false;

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
}
