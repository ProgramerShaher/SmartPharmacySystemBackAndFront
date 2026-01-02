import { Component, Input, OnInit, OnDestroy, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AlertService } from '../../core/services/alert.service';
import { Subject, takeUntil, filter } from 'rxjs';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, RouterModule, RouterLink, RouterLinkActive],
    templateUrl: './sidebar.component.html',
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
    @HostBinding('class.sidebar-visible') get visible() { return this.sidebarVisible; }

    unreadAlertsCount = 0;
    currentRoute = '';
    expenseMenuOpen = false; // Track expense submenu state
    private destroy$ = new Subject<void>();

    constructor(
        private alertService: AlertService,
        private router: Router
    ) { }

    ngOnInit() {
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
            // Auto-open expense menu if on expense route
            if (event.url.includes('/finance/expense')) {
                this.expenseMenuOpen = true;
            }
        });
    }

    toggleExpenseMenu() {
        this.expenseMenuOpen = !this.expenseMenuOpen;
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
}