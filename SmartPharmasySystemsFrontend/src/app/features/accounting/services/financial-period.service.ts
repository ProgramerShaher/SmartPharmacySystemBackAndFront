import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FinancialPeriodService {
  private apiUrl = `${environment.apiUrl}/FinancialPeriods`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  create(period: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, period);
  }

  close(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/close`, {});
  }
}
