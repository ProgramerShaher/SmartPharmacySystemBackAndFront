import { Injectable, signal, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, of } from 'rxjs';
import { switchMap, tap, catchError, retry, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface SystemNotification {
  id: number;
  userId: number;
  userName: string;
  branchId?: number;
  branchName?: string;
  type: number;
  typeName: string;
  typeIcon: string;
  title: string;
  body: string;
  referenceId?: number;
  referenceType?: string;
  isRead: boolean;
  createdAt: string;
  timeAgo: string;
}

@Injectable({ providedIn: 'root' })
export class SystemNotificationService {
  private apiUrl = `${environment.apiUrl}/Notifications`;

  private unreadSubject = new BehaviorSubject<SystemNotification[]>([]);
  public unread$ = this.unreadSubject.asObservable();
  public unreadCount = signal(0);

  private isPollingActive = false;
  private currentUserId: number = 0;
  private currentBranchId?: number;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  startPolling(userId: number, branchId?: number) {
    this.currentUserId = userId;
    this.currentBranchId = branchId;

    if (this.isPollingActive || !isPlatformBrowser(this.platformId)) return;
    this.isPollingActive = true;

    this.fetchUnread().subscribe();

    interval(60000).pipe(
      switchMap(() => this.fetchUnread()),
      retry({ count: 3, delay: 5000 }),
      catchError(() => of([]))
    ).subscribe();
  }

  private fetchUnread(): Observable<SystemNotification[]> {
    if (!this.currentUserId) return of([]);
    let params = `?userId=${this.currentUserId}`;
    if (this.currentBranchId) params += `&branchId=${this.currentBranchId}`;

    return this.http.get<any>(`${this.apiUrl}/unread${params}`).pipe(
      map(res => res.data as SystemNotification[]),
      tap(notifications => {
        this.unreadSubject.next(notifications);
        this.unreadCount.set(notifications.length);
      }),
      catchError(() => of([]))
    );
  }

  refreshUnread(): void {
    this.fetchUnread().subscribe();
  }

  getUnreadNotifications(userId: number, branchId?: number): Observable<any> {
    let params = `?userId=${userId}`;
    if (branchId) {
      params += `&branchId=${branchId}`;
    }
    return this.http.get<any>(`${this.apiUrl}/unread${params}`);
  }

  markAsRead(id: number, userId: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/read?userId=${userId}`, {}).pipe(
      tap(() => this.refreshUnread())
    );
  }

  markAllAsRead(userId: number, branchId?: number): Observable<any> {
    let params = `?userId=${userId}`;
    if (branchId) {
      params += `&branchId=${branchId}`;
    }
    return this.http.put<any>(`${this.apiUrl}/read-all${params}`, {}).pipe(
      tap(() => this.refreshUnread())
    );
  }
}