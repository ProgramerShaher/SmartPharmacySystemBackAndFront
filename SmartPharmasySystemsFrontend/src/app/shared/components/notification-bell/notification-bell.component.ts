import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AlertService } from '../../../core/services/alert.service';
import { Alert, AlertUtils } from '../../../core/models/alert.interface';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-notification-bell',
    standalone: true,
    imports: [
        CommonModule,
        ButtonModule,
        BadgeModule,
        OverlayPanelModule,
        ToastModule
    ],
    providers: [MessageService],
    templateUrl: './notification-bell.component.html',
    styleUrls: ['./notification-bell.component.scss']
})
export class NotificationBellComponent implements OnInit, OnDestroy {
    unreadAlerts: Alert[] = [];
    unreadCount = 0;
    private destroy$ = new Subject<void>();

    // Expose utility class to template
    AlertUtils = AlertUtils;

    constructor(
        public alertService: AlertService,
        private router: Router,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        // Subscribe to unread alerts
        this.alertService.unreadAlerts$
            .pipe(takeUntil(this.destroy$))
            .subscribe(alerts => {
                const previousCount = this.unreadCount;
                this.unreadAlerts = alerts;
                this.unreadCount = alerts.length;

                // Show toast for new alerts
                if (alerts.length > previousCount && previousCount > 0) {
                    const newAlert = alerts[0];
                    this.showNewAlertToast(newAlert);
                }
            });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    showNewAlertToast(alert: Alert) {
        this.messageService.add({
            severity: 'warn',
            summary: 'تنبيه جديد',
            detail: alert.message,
            life: 5000,
            sticky: false
        });
    }

    markAsRead(alert: Alert, event: Event) {
        event.stopPropagation();
        this.alertService.markAsRead(alert.id).subscribe({
            next: () => {
                this.alertService.refreshUnreadAlerts();
            }
        });
    }

    markAllAsRead() {
        this.alertService.markAllAsRead().subscribe({
            next: () => {
                this.messageService.add({
                    severity: 'success',
                    summary: 'تم',
                    detail: 'تم تعليم جميع التنبيهات كمقروءة'
                });
            }
        });
    }

    viewAllAlerts() {
        this.router.navigate(['/system-alerts']);
    }

    viewAlert(alert: Alert) {
        this.router.navigate(['/system-alerts', alert.id]);
    }

    getTimeAgo(date: string): string {
        const now = new Date();
        const alertDate = new Date(date);
        const diffMs = now.getTime() - alertDate.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'الآن';
        if (diffMins < 60) return `منذ ${diffMins} دقيقة`;
        if (diffHours < 24) return `منذ ${diffHours} ساعة`;
        return `منذ ${diffDays} يوم`;
    }
}
