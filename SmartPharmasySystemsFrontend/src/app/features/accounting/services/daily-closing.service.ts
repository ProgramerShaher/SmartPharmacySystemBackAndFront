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
    // Calling the endpoint for branch 1
    return this.http.get<any>(`${this.apiUrl}/branch/1`);
  }

  closeDay(date: Date, transferToMainSafe: boolean = false): Observable<any> {
    const formattedDate = date.toISOString().split('T')[0];
    return this.http.post<any>(`${this.apiUrl}`, { 
      branchId: 1, 
      closingDate: formattedDate, 
      openingCash: 0, 
      actualCash: 0, 
      transferToMainSafe 
    });
  }
}
