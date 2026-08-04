import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
    private readonly apiUrl = `${environment.apiUrl}/Attendances`;

    constructor(private http: HttpClient) { }

    private formatDate(date: Date): string {
        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    markAbsent(employeeId: number, date?: Date): Observable<any> {
        let url = `${this.apiUrl}/mark-absent?employeeId=${employeeId}`;
        if (date) {
            url += `&date=${this.formatDate(date)}`;
        }
        return this.http.post<ApiResponse<any>>(url, {}).pipe(
            map(response => response.data)
        );
    }

    markPresent(employeeId: number, date?: Date): Observable<any> {
        let url = `${this.apiUrl}/mark-present?employeeId=${employeeId}`;
        if (date) {
            url += `&date=${this.formatDate(date)}`;
        }
        return this.http.post<ApiResponse<any>>(url, {}).pipe(
            map(response => response.data)
        );
    }

    getByEmployee(employeeId: number, from?: Date, to?: Date): Observable<any[]> {
        let url = `${this.apiUrl}/employee/${employeeId}`;
        const params: string[] = [];
        if (from) params.push(`from=${this.formatDate(from)}`);
        if (to) params.push(`to=${this.formatDate(to)}`);
        if (params.length > 0) url += `?${params.join('&')}`;
        
        return this.http.get<ApiResponse<any[]>>(url).pipe(
            map(response => response.data)
        );
    }
}
