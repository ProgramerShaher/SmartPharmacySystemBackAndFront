import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models';

@Injectable({ providedIn: 'root' })
export class MonthlySalaryService {
    private readonly apiUrl = `${environment.apiUrl}/MonthlySalaries`;

    constructor(private http: HttpClient) { }

    getByEmployeeMonthYear(employeeId: number, month: number, year: number): Observable<any> {
        let params = new HttpParams()
            .set('month', month.toString())
            .set('year', year.toString());
        return this.http.get<ApiResponse<any>>(`${this.apiUrl}/employee/${employeeId}/month`, { params }).pipe(
            map(response => response.data)
        );
    }

    getByBranchMonthYear(month: number, year: number, branchId?: number): Observable<any[]> {
        let params = new HttpParams()
            .set('month', month.toString())
            .set('year', year.toString());
        if (branchId) {
            params = params.set('branchId', branchId.toString());
        }
        return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/by-month`, { params }).pipe(
            map(response => response.data)
        );
    }

    create(dto: any): Observable<any> {
        return this.http.post<ApiResponse<any>>(this.apiUrl, dto).pipe(
            map(response => response.data)
        );
    }

    update(dto: any): Observable<any> {
        return this.http.put<ApiResponse<any>>(this.apiUrl, dto).pipe(
            map(response => response.data)
        );
    }

    delete(id: number): Observable<any> {
        return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`).pipe(
            map(response => response.data)
        );
    }

    pay(id: number): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/pay`, {}).pipe(
            map(response => response.data)
        );
    }

    payAll(month: number, year: number, branchId?: number): Observable<number> {
        let params = new HttpParams()
            .set('month', month.toString())
            .set('year', year.toString());
        if (branchId) {
            params = params.set('branchId', branchId.toString());
        }
        return this.http.post<ApiResponse<number>>(`${this.apiUrl}/pay-all`, {}, { params }).pipe(
            map(response => response.data)
        );
    }

    getSummary(month: number, year: number, branchId?: number): Observable<any> {
        let params = new HttpParams()
            .set('month', month.toString())
            .set('year', year.toString());
        if (branchId) {
            params = params.set('branchId', branchId.toString());
        }
        return this.http.get<ApiResponse<any>>(`${this.apiUrl}/summary`, { params }).pipe(
            map(response => response.data)
        );
    }
}
