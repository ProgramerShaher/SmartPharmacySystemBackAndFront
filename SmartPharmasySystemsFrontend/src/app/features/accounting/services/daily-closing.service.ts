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
    return this.http.get<any>(this.apiUrl);
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  closeDay(date: Date, transferToMainSafe: boolean = false): Observable<any> {
    const formattedDate = date.toISOString().split('T')[0];
    return this.http.post<any>(`${this.apiUrl}/close-day`, { closingDate: formattedDate, transferToMainSafe });
  }
}
