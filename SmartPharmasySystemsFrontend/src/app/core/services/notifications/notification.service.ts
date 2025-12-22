// src/app/core/services/notifications/notification.service.ts
import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export interface Notification {
  id: number;
  type: 'success' | 'error' | 'info' | 'warning';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private notifications = new Subject<Notification>();
  notifications$ = this.notifications.asObservable();
  private nextId = 1;

  success(title: string, message: string, duration: number = 5000): void {
    this.addNotification('success', title, message, duration);
  }

  error(title: string, message: string, duration: number = 8000): void {
    this.addNotification('error', title, message, duration);
  }

  info(title: string, message: string, duration: number = 5000): void {
    this.addNotification('info', title, message, duration);
  }

  warning(title: string, message: string, duration: number = 6000): void {
    this.addNotification('warning', title, message, duration);
  }

  private addNotification(type: Notification['type'], title: string, message: string, duration: number): void {
    const notification: Notification = {
      id: this.nextId++,
      type,
      title,
      message,
      duration,
      timestamp: new Date()
    };
    this.notifications.next(notification);
    
    // Log to console for debugging
    const consoleMethod = type === 'error' ? 'error' : 'info';
    console[consoleMethod](`[Notification ${type.toUpperCase()}] ${title}: ${message}`);
  }
}