import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto, EmployeeQueryDto, BranchDto, DepartmentDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
    private readonly apiUrl = `${environment.apiUrl}/employees`;
    private readonly branchApiUrl = `${environment.apiUrl}/Branches`;
    private readonly departmentApiUrl = `${environment.apiUrl}/Departments`;

    constructor(private http: HttpClient) { }

    getAll(query?: EmployeeQueryDto): Observable<EmployeeDto[]> {
        let params = new HttpParams();
        if (query?.search) params = params.set('search', query.search);
        if (query?.branchId) params = params.set('branchId', query.branchId.toString());
        if (query?.departmentId) params = params.set('departmentId', query.departmentId.toString());
        if (query?.isActive !== undefined) params = params.set('isActive', query.isActive.toString());
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<any>>(this.apiUrl, { params })
            .pipe(map(response => {
                const data = response.data;
                if (data && Array.isArray(data.items)) return data.items;
                return Array.isArray(data) ? data : [];
            }));
    }

    getById(id: number): Observable<EmployeeDto> {
        return this.http.get<ApiResponse<EmployeeDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(response => response.data));
    }

    getByCode(code: string): Observable<EmployeeDto> {
        return this.http.get<ApiResponse<EmployeeDto>>(`${this.apiUrl}/code/${code}`)
            .pipe(map(response => response.data));
    }

    create(dto: CreateEmployeeDto): Observable<EmployeeDto> {
        return this.http.post<ApiResponse<EmployeeDto>>(this.apiUrl, dto)
            .pipe(map(response => response.data));
    }

    update(dto: UpdateEmployeeDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${dto.id}`, dto)
            .pipe(map(response => response.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(response => response.data));
    }

    getBranches(): Observable<BranchDto[]> {
        return this.http.get<ApiResponse<any>>(`${this.branchApiUrl}/active`)
            .pipe(map(response => {
                const data = response.data;
                return Array.isArray(data) ? data : [];
            }));
    }

    getDepartments(): Observable<DepartmentDto[]> {
        return this.http.get<ApiResponse<any>>(this.departmentApiUrl)
            .pipe(map(response => {
                const data = response.data;
                return Array.isArray(data) ? data : [];
            }));
    }

    getBranchDashboard(branchId?: number): Observable<any> {
        let url = `${this.apiUrl}/dashboard`;
        if (branchId) {
            url += `/${branchId}`;
        }
        return this.http.get<ApiResponse<any>>(url)
            .pipe(map(response => response.data));
    }
}
