import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
    private readonly apiUrl = `${environment.apiUrl}/Departments`;

    constructor(private http: HttpClient) { }

    getAll(search?: string): Observable<DepartmentDto[]> {
        let params = new HttpParams();
        if (search) params = params.set('search', search);

        return this.http.get<ApiResponse<any>>(this.apiUrl, { params })
            .pipe(map(r => Array.isArray(r.data) ? r.data : []));
    }

    getById(id: number): Observable<DepartmentDto> {
        return this.http.get<ApiResponse<DepartmentDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

    getByName(name: string): Observable<DepartmentDto> {
        return this.http.get<ApiResponse<DepartmentDto>>(`${this.apiUrl}/name/${name}`)
            .pipe(map(r => r.data));
    }

    create(dto: CreateDepartmentDto): Observable<DepartmentDto> {
        return this.http.post<ApiResponse<DepartmentDto>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    update(dto: UpdateDepartmentDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }
}
