import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models';

@Injectable({ providedIn: 'root' })
export class EmployeeLoanService {
    private readonly apiUrl = `${environment.apiUrl}/EmployeeLoans`;

    constructor(private http: HttpClient) { }

    getByEmployeeId(employeeId: number): Observable<any[]> {
        return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/employee/${employeeId}`).pipe(
            map(response => response.data)
        );
    }

    getRemaining(employeeId: number): Observable<number> {
        return this.http.get<ApiResponse<number>>(`${this.apiUrl}/employee/${employeeId}/remaining`).pipe(
            map(response => response.data)
        );
    }
}
