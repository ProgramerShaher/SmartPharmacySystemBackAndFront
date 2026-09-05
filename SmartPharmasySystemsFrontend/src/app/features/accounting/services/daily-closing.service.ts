import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DailyClosingService {
  private apiUrl = `${environment.apiUrl}/DailyClosings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    // يجلب الإغلاقات للفرع الحالي — BranchId يُقرأ من الـ token في الـ backend
    return this.http.get<any>(`${this.apiUrl}`);
  }

  getByBranch(branchId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/branch/${branchId}`);
  }

  closeDay(date: Date): Observable<any> {
    // إرسال التاريخ فقط — جميع القيم المالية تُحسب تلقائياً من الـ backend
    const formattedDate = date.toISOString().split('T')[0];
    return this.http.post<any>(`${this.apiUrl}`, {
      closingDate: formattedDate
    });
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }
}
